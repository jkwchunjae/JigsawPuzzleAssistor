using MainApp.Service;
using Microsoft.AspNetCore.Components;

namespace MainApp.Pages.Outline;

public partial class OutlinePage : ComponentBase
{
    [Inject] NavigationManager NavigationManager { get; set; } = null!;
    [Inject] WorkspaceService WorkspaceService { get; set; } = null!;

    WorkspaceData? workspace => WorkspaceService.CurrentWorkspace;

    int OutlineProgress = 0;

    protected override void OnInitialized()
    {
        if (workspace == null)
        {
            NavigationManager.NavigateTo("/start");
        }
        else if (!WorkspaceService.HasCroppedImage())
        {
            NavigationManager.NavigateTo("/outline");
        }
    }

    Task StartOutline(int thickness)
    {
        OutlineProgress = 0;

        var outlineService = new ImageOutlineService(workspace!);
        outlineService.OutlineProgress += (sender, e) =>
        {
            InvokeAsync(() =>
            {
                OutlineProgress = (int)(e.Processed / (double)e.Total * 100);
                StateHasChanged();
            });
        };

        _ = outlineService.StartOutline(thickness);
        return Task.CompletedTask;
    }
}
