using PictureToData;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;

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
            var corners = await pieceService.GetCornerWithArgument(file, cornerArgs);
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

    public async Task<(string FileName, PointF[] Corners)> MakeCornerSelectionFile(string input, List<PointF> selected)
    {
        var argument = new CornerDetectArgument
        {
            MaxCorners = 20,
            BlockSize = 5,
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
