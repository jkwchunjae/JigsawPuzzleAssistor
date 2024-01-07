using JkwExtensions;
using System.Text;
using System.Text.Json;

namespace MainApp.Service;

public class WorkspaceService
{
    WorkspaceData? workspace;
    public void SetCurrentWorkspace(WorkspaceData? workspace)
    {
        this.workspace = workspace;
    }
    public WorkspaceData? CurrentWorkspace => workspace;
    public bool HasSourceImage()
    {
        if (workspace == null)
            return false;

        if (!Directory.Exists(workspace.SourceDir))
            return false;

        var sources = Directory.EnumerateFiles(workspace.SourceDir);
        if (sources.Empty())
            return false;

        return true;
    }
    public bool HasCroppedImage()
    {
        if (workspace == null)
            return false;

        if (!Directory.Exists(workspace.ResizeDir))
            return false;

        var sources = Directory.EnumerateFiles(workspace.ResizeDir);
        if (sources.Empty())
            return false;

        return true;
    }
    public async Task SaveCornerErrorsAsync(CornerErrorResult[] errors)
    {
        if (workspace == null)
            return;

        if (!Directory.Exists(Directory.GetParent(workspace.CornerErrorsPath)!.FullName))
        {
            Directory.CreateDirectory(Directory.GetParent(workspace.CornerErrorsPath)!.FullName);
        }

        var json = JsonSerializer.Serialize(errors, new JsonSerializerOptions
        {
            WriteIndented = true,
        });
        await File.WriteAllTextAsync(workspace.CornerErrorsPath, json, Encoding.UTF8);
    }
    public async Task<CornerErrorResult[]> GetCornerErrorsAsync()
    {
        if (workspace == null)
            return Array.Empty<CornerErrorResult>();

        if (!File.Exists(workspace.CornerErrorsPath))
            return Array.Empty<CornerErrorResult>();

        var json = await File.ReadAllTextAsync(workspace.CornerErrorsPath);
        var errors = JsonSerializer.Deserialize<CornerErrorResult[]>(json);
        return (errors ?? Array.Empty<CornerErrorResult>())
            .OrderBy(x => x.FileName)
            .ToArray();
    }
}
