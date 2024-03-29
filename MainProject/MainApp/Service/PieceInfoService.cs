﻿using Common.PieceInfo;
using PictureToData;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json;

namespace MainApp.Service;

public class InfoResult
{
    public required string FileName { get; set; }
    public string? Error { get; set; }
}

public class PieceInfoService
{
    private WorkspaceData workspace;
    public event EventHandler<ProgressEventArgs>? InfoProgress;
    public PieceInfoService(WorkspaceData workspace)
    {
        this.workspace = workspace;
    }

    public async Task<InfoResult[]> Start(CornerDetectArgument argument, bool openFolder = false)
    {
        if (!Directory.Exists(workspace.InfoDir))
        {
            Directory.CreateDirectory(workspace.InfoDir);
        }

        ISinglePieceImageProcessor processor = new SinglePieceImageProcessor();

        var processed = 0;
        var files = Directory.GetFiles(workspace.ResizeDir);
        Dictionary<string, PieceInfo?> fileInfoDictionary = files
            .ToDictionary(file => file, _ => (PieceInfo?)null);

        ConcurrentBag<InfoResult> results = new ConcurrentBag<InfoResult>();

        await Parallel.ForEachAsync(files, async (file, ct) =>
        {
            try
            {
                var (hasPredefined, predefinedCorners) = await CheckPredefinedCorner(file);
                if (hasPredefined)
                {
                    var info = await processor.MakePieceInfoWithPredefinedCornerAsync(file, predefinedCorners!);
                    fileInfoDictionary[file] = info;
                }
                else
                {
                    var info = await processor.MakePieceInfoAsync(file, argument);
                    fileInfoDictionary[file] = info;
                }
            }
            catch (Exception ex)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                results.Add(new InfoResult
                {
                    FileName = fileName,
                    Error = ex.Message,
                });
            }
            finally
            {
                Interlocked.Increment(ref processed);
                InfoProgress?.Invoke(this, new ProgressEventArgs
                {
                    Total = files.Length,
                    Processed = processed,
                });
            }
        });

        var serializeOption = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new PointFJsonConverter(),
                new PointArrayJsonConverter(),
                new PointFArrayJsonConverter(),
            },
        };

        foreach (var (file, info) in fileInfoDictionary)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var targetPath = Path.Join(workspace.InfoDir, $"{fileName}.json");
            if (info != null)
            {
                await File.WriteAllTextAsync(targetPath, JsonSerializer.Serialize(info, serializeOption));
            }
        }

        if (openFolder)
        {
            Process.Start("explorer.exe", workspace.InfoDir);
        }

        return results.ToArray();
    }

    public async Task CreatePieceInfoWithPredefinedCorner(string input, PointF[] corners)
    {
        ISinglePieceImageProcessor processor = new SinglePieceImageProcessor();
        var pieceInfo = await processor.MakePieceInfoWithPredefinedCornerAsync(input, corners);
        pieceInfo.PredefinedCorners = corners;

        var serializeOption = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new PointFJsonConverter(),
                new PointArrayJsonConverter(),
                new PointFArrayJsonConverter(),
            },
        };
        if (!Directory.Exists(workspace.InfoDir))
        {
            Directory.CreateDirectory(workspace.InfoDir);
        }
        var fileName = Path.GetFileNameWithoutExtension(input);
        var targetPath = Path.Join(workspace.InfoDir, $"{fileName}.json");

        await File.WriteAllTextAsync(targetPath, JsonSerializer.Serialize(pieceInfo, serializeOption));
    }

    public async Task<(bool Exists, PointF[]? Corners)> CheckPredefinedCorner(string filePath)
    {
        var infoPath = Path.Join(workspace.InfoDir, $"{Path.GetFileNameWithoutExtension(filePath)}.json");

        if (File.Exists(infoPath))
        {
            JsonSerializerOptions serializeOption = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new PointFJsonConverter(),
                    new PointArrayJsonConverter(),
                    new PointFArrayJsonConverter(),
                },
            };

            var text = await File.ReadAllTextAsync(infoPath);
            var pieceInfo = JsonSerializer.Deserialize<PieceInfo>(text, serializeOption);

            if (pieceInfo?.PredefinedCorners?.Any() ?? false)
            {
                return (true, pieceInfo.PredefinedCorners!);
            }
        }

        return (false, null);
    }
}
