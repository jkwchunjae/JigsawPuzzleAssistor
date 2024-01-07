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

    CornerErrorResult[] CornerErrors = Array.Empty<CornerErrorResult>();

    protected override async Task OnInitializedAsync()
    {
        if (workspace == null)
        {
            NavigationManager.NavigateTo("/start");
            return;
        }
        else if (!WorkspaceService.HasCroppedImage())
        {
            NavigationManager.NavigateTo("/outline");
            return;
        }

        CornerErrors = await WorkspaceService.GetCornerErrorsAsync();
    }

    async Task StartCorner(int thickness)
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

        var errors = await cornerService.StartCorner(cornerDetectArgument, thickness);

        CornerErrors = errors
            .OrderBy(x => x.FileName)
            .ToArray();

        await WorkspaceService.SaveCornerErrorsAsync(CornerErrors);
    }
}
