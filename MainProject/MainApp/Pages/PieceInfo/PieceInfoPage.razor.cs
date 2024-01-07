using MainApp.Service;
using Microsoft.AspNetCore.Components;
using PictureToData;

namespace MainApp.Pages.PieceInfo;

public partial class PieceInfoPage : ComponentBase
{
    [Inject] NavigationManager NavigationManager { get; set; } = null!;
    [Inject] WorkspaceService WorkspaceService { get; set; } = null!;

    WorkspaceData? workspace => WorkspaceService.CurrentWorkspace;

    int InfoProgress = 0;
    List<InfoResult> InfoResults = new List<InfoResult>();

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
    async Task Start()
    {
        InfoProgress = 0;

        var pieceInfoService = new PieceInfoService(workspace!);
        pieceInfoService.InfoProgress += (sender, e) =>
        {
            InvokeAsync(() =>
            {
                InfoProgress = (int)(e.Processed / (double)e.Total * 100);
                StateHasChanged();
            });
        };

        var cornerDetectArgument = new CornerDetectArgument
        {
            MaxCorners = 4,
            QualityLevel = 0.01,
            MinDistance = 100,
            BlockSize = 9,
        };

        var errors = await pieceInfoService.Start(cornerDetectArgument);
        InfoResults = errors
            .OrderBy(x => x.FileName)
            .ToList();
    }
}
