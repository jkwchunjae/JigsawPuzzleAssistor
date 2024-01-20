using MainApp.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PictureToData;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MainApp.Pages.Corner;

public partial class CornerPage : ComponentBase
{
    [Inject] NavigationManager NavigationManager { get; set; } = null!;
    [Inject] WorkspaceService WorkspaceService { get; set; } = null!;
    [Inject] IJSRuntime Js { get; set; } = null!;

    WorkspaceData? workspace => WorkspaceService.CurrentWorkspace;

    int CornerProgress = 0;

    CornerErrorResult[] CornerErrors = Array.Empty<CornerErrorResult>();

    CornerErrorResult? SelectedError = null;
    string cornerSelectionImage = string.Empty;
    PointF[] corners = Array.Empty<PointF>();
    List<PointF> selected = new List<PointF>();

    string[] normalImages = Array.Empty<string>();
    string selectedImage = string.Empty;

    string currentCornerImage = string.Empty;

    string selectType = string.Empty;

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
            MinDistance = 160,
            BlockSize = 9,
        };

        var (errors, files) = await cornerService.StartCorner(cornerDetectArgument, thickness);

        CornerErrors = errors
            .OrderBy(x => x.FileName)
            .ToArray();

        normalImages = files;

        await WorkspaceService.SaveCornerErrorsAsync(CornerErrors);
    }

    async Task ErrorSelectChanged(CornerErrorResult? error)
    {
        await Js.InvokeVoidAsync("console.log", error);
        SelectedError = error;
        selected.Clear();
        corners = Array.Empty<PointF>();
        cornerSelectionImage = string.Empty;
        if (error != null)
        {
            var cornerService = new ImageCornerService(workspace!);
            var (cornerSelectionFileName, corners) = await cornerService.MakeCornerSelectionFile(error!.FullPath, selected);
            this.corners = corners;
            cornerSelectionImage = cornerSelectionFileName;
            currentCornerImage = Path.Join(workspace!.CornerDir, Path.GetFileName(cornerSelectionImage));

            this.selectType = "error";
        }
    }

    private async Task Click(Point point)
    {
        var cornerService = new ImageCornerService(workspace!);
        await Js!.InvokeVoidAsync("console.log", point);
        if (corners?.Any() ?? false)
        {
            var currentTarget = new PointF((float)point.X, (float)point.Y);
            var selectCorner = corners
                .OrderBy(corner => Distance(corner, currentTarget))
                .First();

            if (selected?.Any(s => s == selectCorner) ?? false)
            {
                selected = selected
                    .Where(x => x != selectCorner)
                    .ToList();
            }
            else
            {
                selected ??= new List<PointF>();
                selected.Add(selectCorner);
            }

            if (selectType == "error")
            {
                var (fileName, _) = await cornerService.MakeCornerSelectionFile(SelectedError!.FullPath, selected);
            }
            else if (selectType == "normal")
            {
                var (fileName, _) = await cornerService.MakeCornerSelectionFile(selectedImage, selected);
            }
        }

        if (selected.Count == 4)
        {
            if (selectType == "error")
            {
                var pieceInfoService = new PieceInfoService(workspace!);
                await pieceInfoService.CreatePieceInfoWithPredefinedCorner(SelectedError!.FullPath, selected.ToArray());
                await cornerService.SaveCornerImage(cornerSelectionImage, selected.ToArray(), 3);

                var index = CornerErrors.Select((x, i) => (Item: x, Index: i))
                    .First(x => x.Item == SelectedError).Index;
                var nextIndex = index + 1;
                if (nextIndex < CornerErrors.Length)
                {
                    await ErrorSelectChanged(CornerErrors[nextIndex]);
                }
            }
            else if (selectType == "normal")
            {
                var pieceInfoService = new PieceInfoService(workspace!);
                await pieceInfoService.CreatePieceInfoWithPredefinedCorner(selectedImage, selected.ToArray());
                await cornerService.SaveCornerImage(currentCornerImage, selected.ToArray(), 3);

                var index = normalImages.Select((x, i) => (Item: x, Index: i))
                    .First(x => x.Item == selectedImage).Index;
                var nextIndex = index + 1;
                if (nextIndex < normalImages.Length)
                {
                    await ImageSelectChanged(normalImages[nextIndex]);
                }
            }
        }
    }

    async Task ImageSelectChanged(string? normalImage)
    {
        await Js.InvokeVoidAsync("console.log", normalImage);
        selectedImage = normalImage;
        selected.Clear();
        corners = Array.Empty<PointF>();
        cornerSelectionImage = string.Empty;
        if (normalImage != null)
        {
            var cornerService = new ImageCornerService(workspace!);
            var (cornerSelectionFileName, corners) = await cornerService.MakeCornerSelectionFile(normalImage, selected);
            this.corners = corners;
            cornerSelectionImage = cornerSelectionFileName;
            currentCornerImage = Path.Join(workspace!.CornerDir, Path.GetFileName(normalImage));

            this.selectType = "normal";
        }
    }

    async Task Next()
    {
        var index = normalImages.Select((x, i) => (Item: x, Index: i))
            .First(x => x.Item == selectedImage).Index;
        var nextIndex = index + 1;
        if (nextIndex < normalImages.Length)
        {
            await ImageSelectChanged(normalImages[nextIndex]);
        }
    }

    static double Distance(System.Drawing.PointF p1, System.Drawing.PointF p2)
    {
        double dx = p2.X - p1.X;
        double dy = p2.Y - p1.Y;

        return Math.Sqrt(dx * dx + dy * dy);
    }
}
