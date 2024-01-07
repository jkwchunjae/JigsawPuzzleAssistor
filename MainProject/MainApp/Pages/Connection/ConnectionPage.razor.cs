using MainApp.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MainApp.Pages.Connection;

public partial class ConnectionPage : ComponentBase
{
    [Inject] NavigationManager NavigationManager { get; set; } = null!;
    [Inject] WorkspaceService WorkspaceService { get; set; } = null!;
    [Inject] IJSRuntime Js { get; set; } = null!;

    WorkspaceData? workspace => WorkspaceService.CurrentWorkspace;

    int ProgressValue = 0;

    protected override Task OnInitializedAsync()
    {
        if (workspace == null)
        {
            NavigationManager.NavigateTo("/start");
            return Task.CompletedTask;
        }
        else if (!WorkspaceService.HasCroppedImage())
        {
            NavigationManager.NavigateTo("/outline");
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    Task Start()
    {
        var connectionService = new ConnectionInfoService(workspace!);
        connectionService.ProgressChanged += (sender, e) =>
        {
            InvokeAsync(() =>
            {
                ProgressValue = (int)(e.Processed / (double)e.Total * 100);
                StateHasChanged();
            });
        };
        _ = connectionService.Start();
        return Task.CompletedTask;
    }
}
