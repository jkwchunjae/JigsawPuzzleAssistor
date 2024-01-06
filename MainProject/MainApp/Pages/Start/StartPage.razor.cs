using MainApp.Service;
using Microsoft.AspNetCore.Components;

namespace MainApp.Pages.Start;

public partial class StartPage : ComponentBase
{
    [Inject] public WorkspaceService WorkspaceService { get; set; } = null!;

    private List<WorkspaceData>? Workspaces;

    private string? WorkspaceName;
    private string? WorkspacePath;

    protected override async Task OnInitializedAsync()
    {
        var workspaces = await WorkspaceService.GetWorkspaces();
        Workspaces = workspaces.ToList();
    }

    private async Task AddNewWorkspace(string? name, string? path)
    {
        if (string.IsNullOrEmpty(name)|| string.IsNullOrEmpty(path))
            return;

        var workspace = await WorkspaceService.AddNewWorkspace(name, path);
        if (workspace != null)
        {
            WorkspaceService.SelectWorkspace(workspace);
            var workspaces = await WorkspaceService.GetWorkspaces();
            Workspaces = workspaces.ToList();
            StateHasChanged();
        }
    }

    private void SelectWorkspace(WorkspaceData workspace)
    {
        WorkspaceService.SelectWorkspace(workspace);
    }
}
