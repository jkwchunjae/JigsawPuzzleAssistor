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

    string PieceName = string.Empty;
    string UpdateText = "갱신";

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

    async Task Update()
    {
        var connectionService = new ConnectionInfoService(workspace!);

        if (PieceName.Length >= 4)
        {
            UpdateText = "갱신중...";
            await connectionService.UpdateAsync(PieceName);
            UpdateText = "갱신 완료";
        }
        else
        {
            UpdateText = "4글자 이상 입력하세요.";
        }
    }
}
