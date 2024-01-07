using MainApp.Service;
using Microsoft.AspNetCore.Components;
using PictureToData;

namespace MainApp.Pages.Corner;

public partial class CornerPage : ComponentBase
{
    [Inject] NavigationManager NavigationManager { get; set; } = null!;
    [Inject] WorkspaceService WorkspaceService { get; set; } = null!;

    WorkspaceData? workspace => WorkspaceService.CurrentWorkspace;

    int CornerProgress = 0;

    protected override void OnInitialized()
    {
        if (workspace == null)
        {
            NavigationManager.NavigateTo("/start");
        }
        if (!WorkspaceService.HasCroppedImage())
        {
            NavigationManager.NavigateTo("/outline");
        }
    }

    Task StartCorner(int thickness)
    {
        CornerProgress = 0;

        var cornerService = new ImageCornerService(workspace!);
        cornerService.CornerProgress += (sender, e) =>
        {
            InvokeAsync(() =>
            {
                CornerProgress = (int)(e.Processed / (double)e.Total * 100);
                StateHasChanged();
            });
        };

        var cornerDetectArgument = new CornerDetectArgument
        {
            MaxCorners = 4,
            QualityLevel = 0.01,
            MinDistance = 200,
            BlockSize = 9,
        };

        _ = cornerService.StartCorner(cornerDetectArgument, thickness);
        return Task.CompletedTask;
    }
}
