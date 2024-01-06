using JkwExtensions;

namespace MainApp.Service;

public class WorkspaceService
{
    WorkspaceData? workspace;
    public void SetCurrentWorkspace(WorkspaceData workspace)
    {
        this.workspace = workspace;
    }
    public bool HasSourceImage()
    {
        if (workspace == null)
            return false;

        if (!Directory.Exists(workspace.SourceDir))
            return false;

        var sources = Directory.GetFiles(workspace.SourceDir);
        if (sources.Length == 0)
            return false;

        return true;
    }
    public async Task<List<(string Name, byte[] Image)>> GetSources()
    {
        var files = await Directory.GetFiles(workspace!.SourceDir)
            .Select(async path =>
            {
                var name = Path.GetFileNameWithoutExtension(path);
                var bytes = await File.ReadAllBytesAsync(path);
                return (name, bytes);
            })
            .WhenAll();

        return files.ToList();
    }
}
