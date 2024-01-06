using JkwExtensions;

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
}
