using PictureToData;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;

namespace MainApp.Service;

public class CornerErrorResult
{
    public required string FileName { get; set; }
    public required string Error { get; set; }
}

public class ImageCornerService
{
    private WorkspaceData workspace;
    public event EventHandler<ProgressEventArgs>? CornerProgress;
    public ImageCornerService(WorkspaceData workspace)
    {
        this.workspace = workspace;
    }

    public async Task<CornerErrorResult[]> StartCorner(CornerDetectArgument cornerArgs, int thickness = 1, bool openFolder = false)
    {
        if (!Directory.Exists(workspace.CornerDir))
        {
            Directory.CreateDirectory(workspace.CornerDir);
        }

        var pieceService = new PieceService();

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
                    FileName = fileName,
                    Error = errorNot4Corners,
                });
            }
        });

        if (openFolder)
        {
            Process.Start("explorer.exe", workspace.CornerDir);
        }

        return errors
            .OrderBy(x => x.FileName)
            .ToArray();
    }
}
