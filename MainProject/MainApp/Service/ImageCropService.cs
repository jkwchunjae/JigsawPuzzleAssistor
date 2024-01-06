using PuzzleCropper;
using System.Diagnostics;
using System.Drawing;

namespace MainApp.Service;

public class ProgressEventArgs
{
    public int Total { get; set; }
    public int Processed { get; set; }
}

public class ImageCropService
{
    private WorkspaceData workspace;
    public event EventHandler<ProgressEventArgs>? CutProgress;
    public ImageCropService(WorkspaceData workspace)
    {
        this.workspace = workspace;
    }
    public async Task StartCrop(Rectangle initRoi, bool openDirectory = false)
    {
        if (!Directory.Exists(workspace.ResizeDir))
        {
            Directory.CreateDirectory(workspace.ResizeDir);
        }

        var cropper = new Cropper();

        var processed = 0;
        var files = Directory.GetFiles(workspace.SourceDir);
        await Parallel.ForEachAsync(files, async (file, ct) =>
        {
            var fileName = Path.GetFileName(file);
            var outputPath = Path.Join(workspace.ResizeDir, fileName);
            await cropper.CropUsingOutline(file, outputPath, initRoi);
            Interlocked.Increment(ref processed);
            CutProgress?.Invoke(this, new ProgressEventArgs
            {
                Total = files.Length,
                Processed = processed,
            });
        });

        if (openDirectory)
        {
            Process.Start("explorer.exe", workspace.ResizeDir);
        }
    }
}
