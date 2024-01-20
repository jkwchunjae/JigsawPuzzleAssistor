using Common.PieceInfo;
using PictureToData;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text.Json;

namespace MainApp.Service;

public class CornerErrorResult
{
    public required string FullPath { get; set; }
    public required string FileName { get; set; }
    public required string Error { get; set; }
}

public class ImageCornerService
{
    private WorkspaceData workspace;
    public event EventHandler<ProgressEventArgs>? CornerProgress;

    private PieceService pieceService = new PieceService();

    public ImageCornerService(WorkspaceData workspace)
    {
        this.workspace = workspace;
    }

    public async Task<(CornerErrorResult[] Errors, string[] Files)> StartCorner(CornerDetectArgument cornerArgs, int thickness = 1, bool openFolder = false)
    {
        if (!Directory.Exists(workspace.CornerDir))
        {
            Directory.CreateDirectory(workspace.CornerDir);
        }

        var errors = new ConcurrentBag<CornerErrorResult>();

        var processed = 0;
        var files = Directory.GetFiles(workspace.ResizeDir);
        await Parallel.ForEachAsync(files, async (file, ct) =>
        {
            var fileName = Path.GetFileName(file);
            var outputPath = Path.Join(workspace.CornerDir, fileName);
            var (hasPredefined, predefinedCorners) = await CheckPredefinedCorner(file);

            var corners = hasPredefined ? predefinedCorners! : await pieceService.GetCornerWithArgument(file, cornerArgs);
            await pieceService.MakeCornerAssistImageAsync(file, outputPath, corners, _ =>
            {
                return (Radius: 5, color: Color.White, thickness);
            });
            Interlocked.Increment(ref processed);
            CornerProgress?.Invoke(this, new ProgressEventArgs
            {
                Total = files.Length,
                Processed = processed,
            });
            if (corners.Length != 4)
            {
                const string errorNot4Corners = "코너의 개수가 4개가 아닙니다.";
                errors.Add(new CornerErrorResult
                {
                    FullPath = file,
                    FileName = fileName,
                    Error = errorNot4Corners,
                });
            }
            else if (!pieceService.IsRectangle(corners))
            {
                const string errorNotRectangle = "코너가 사각형이 아닙니다.";
                errors.Add(new CornerErrorResult
                {
                    FullPath = file,
                    FileName = fileName,
                    Error = errorNotRectangle,
                });
            }
        });

        if (openFolder)
        {
            Process.Start("explorer.exe", workspace.CornerDir);
        }

        var errors2 = errors
            .OrderBy(x => x.FileName)
            .ToArray();
        var cornerFiles = Directory.GetFiles(workspace.CornerDir);

        return (errors2, files);
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

    public async Task SaveCornerImage(string imageFullPath, PointF[] corners, int thickness = 1)
    {
        var resizeImage = Path.Join(workspace.ResizeDir, Path.GetFileName(imageFullPath));
        await pieceService.MakeCornerAssistImageAsync(resizeImage, imageFullPath, corners, _ =>
        {
            return (Radius: 5, color: Color.White, thickness: thickness);
        });
    }

    public async Task<(string FileName, PointF[] Corners)> MakeCornerSelectionFile(string input, List<PointF> selected)
    {
        var argument = new CornerDetectArgument
        {
            MaxCorners = 30,
            BlockSize = 3,
            MinDistance = 30,
            QualityLevel = 0.01,
        };

        var corners = await pieceService.GetCornerWithArgument(input, argument);
        var circleFunc = (System.Drawing.PointF point) =>
        {
            if (selected?.Any(c => c == point) ?? false)
            {
                return (10, Color.AliceBlue, 2);
            }
            else
            {
                return (10, Color.Red, 2);
            }
        };

        var output = workspace.CornerSelectionImagePath(Path.GetFileName(input));
        await pieceService.MakeCornerAssistImageAsync(input, output, corners, circleFunc);

        return (output, corners);
    }
}
