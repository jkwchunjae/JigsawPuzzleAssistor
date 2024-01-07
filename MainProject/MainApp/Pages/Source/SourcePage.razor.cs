using MainApp.Service;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;
using System.Drawing;

namespace MainApp.Pages.Source;

public partial class SourcePage : ComponentBase
{
    [Inject] NavigationManager NavigationManager { get; set; } = null!;
    [Inject] WorkspaceService WorkspaceService { get; set; } = null!;

    WorkspaceData? workspace => WorkspaceService.CurrentWorkspace;

    bool HasSourceImage;

    int CropProgress = 0;

    protected override void OnInitialized()
    {
        HasSourceImage = WorkspaceService.HasSourceImage();

        if (workspace == null)
        {
            NavigationManager.NavigateTo("/start");
        }
    }

    void OpenSourceFolder()
    {
        if (!string.IsNullOrEmpty(workspace?.SourceDir))
        {
            var dir = workspace!.SourceDir;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            Process.Start("explorer.exe",dir);
        }
    }
    void Check()
    {
        HasSourceImage = WorkspaceService.HasSourceImage();
    }
    Task StartCutting()
    {
        CropProgress = 0;

        var cropService = new ImageCropService(workspace!);
        cropService.CutProgress += (sender, e) =>
        {
            InvokeAsync(() =>
            {
                CropProgress = (int)(e.Processed / (double)e.Total * 100);
                StateHasChanged();
            });
        };

        var initRoi = new Rectangle(200, 400, 700, 900);
        _ = cropService.StartCrop(initRoi);

        return Task.CompletedTask;
    }
}
