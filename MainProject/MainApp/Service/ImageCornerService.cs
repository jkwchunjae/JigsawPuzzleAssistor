using PictureToData;
using System.Diagnostics;
using System.Drawing;

namespace MainApp.Service;

public class ImageCornerService
{
    private WorkspaceData workspace;
    public event EventHandler<ProgressEventArgs>? CornerProgress;
    public ImageCornerService(WorkspaceData workspace)
    {
        this.workspace = workspace;
    }

    public async Task StartCorner(CornerDetectArgument cornerArgs, int thickness = 1, bool openFolder = false)
    {
        if (!Directory.Exists(workspace.CornerDir))
        {
            Directory.CreateDirectory(workspace.CornerDir);
        }

        var pieceService = new PieceService();

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
        });

        if (openFolder)
        {
            Process.Start("explorer.exe", workspace.CornerDir);
        }
    }
}
