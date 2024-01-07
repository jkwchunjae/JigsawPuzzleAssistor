using PictureToData;
using System.Diagnostics;

namespace MainApp.Service;

public class ImageOutlineService
{
    private WorkspaceData workspace;
    public event EventHandler<ProgressEventArgs>? OutlineProgress;
    public ImageOutlineService(WorkspaceData workspace)
    {
        this.workspace = workspace;
    }
    public async Task StartOutline(int thickness = 1, bool openFolder = false)
    {
        if (!Directory.Exists(workspace.OutlineDir))
        {
            Directory.CreateDirectory(workspace.OutlineDir);
        }

        var puzzleService = new PieceService();

        var processed = 0;
        var files = Directory.GetFiles(workspace.ResizeDir);
        await Parallel.ForEachAsync(files, async (file, ct) =>
        {
            var fileName = Path.GetFileName(file);
            var outputPath = Path.Join(workspace.OutlineDir, fileName);
            await puzzleService.MakeOutlineImageAsync(file, outputPath, thickness);
            Interlocked.Increment(ref processed);
            OutlineProgress?.Invoke(this, new ProgressEventArgs
            {
                Total = files.Length,
                Processed = processed,
            });
        });

        if (openFolder)
        {
            Process.Start("explorer.exe", workspace.OutlineDir);
        }
    }
}
