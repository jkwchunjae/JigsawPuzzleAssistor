using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PictureToData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectCornersManually.Components.Pages;

public partial class SelectCorner : ComponentBase
{
    [Inject] IJSRuntime? Js { get; set; }
    [Parameter] public string Image { get; set; }

    PieceService PieceService { get; set; }

    string CornerImage = string.Empty;
    System.Drawing.PointF[] corners = null;
    List<System.Drawing.PointF> selected = null;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        PieceService = new PieceService();
    }

    private async Task Click(Point point)
    {
        await Js!.InvokeVoidAsync("console.log", point);
        if (corners?.Any() ?? false)
        {
            var currentTarget = new System.Drawing.PointF((float)point.X, (float)point.Y);
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
                selected ??= new List<System.Drawing.PointF>();
                selected.Add(selectCorner);
            }

            await GetOutlineCornerImage();
        }
    }

    private async Task GetOutlineCornerImage()
    {
        var argument = new CornerDetectArgument
        {
            MaxCorners = 20,
            BlockSize = 9,
            MinDistance = 30,
            QualityLevel = 0.01,
        };
        var points = await PieceService.GetCornerWithArgument(Image, argument);
        var outputPath = GetOutputPath(Image);
        var circleFunc = (System.Drawing.PointF point) =>
        {
            if (selected?.Any(c => c == point) ?? false)
            {
                return (10, System.Drawing.Color.AliceBlue, 2);
            }
            else
            {
                return (10, System.Drawing.Color.Red, 2);
            }
        };
        await PieceService.MakeCornerAssistImageAsync(Image, outputPath, points, circleFunc);

        corners = points;
        CornerImage = outputPath;
    }

    private string GetOutputPath(string imagePath)
    {
        var filename = Path.GetFileNameWithoutExtension(imagePath);
        var outputName = $"{filename}.png";
        var tempDir = Path.Join(Directory.GetParent(imagePath).Parent.FullName, "temp");
        var outputPath = Path.Join(tempDir, outputName);
        return outputPath;
    }
    static double Distance(System.Drawing.PointF p1, System.Drawing.PointF p2)
    {
        double dx = p2.X - p1.X;
        double dy = p2.Y - p1.Y;

        return Math.Sqrt(dx * dx + dy * dy);
    }
}
