using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PictureToData;

public class PieceService
{
    public async Task<PointF[]> GetCornerWithArgument(string imagePath, CornerDetectArgument detectArgument)
    {
        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException($"File not found: {imagePath}");
        }
        if (!CvInvoke.Init())
        {
            throw new Exception("Unable to initialize CvInvoke");
        }

        var outline = new Outline(imagePath);
        await outline.ProcessAsync();

        var cornerDetector = new CornerDetector(outline, detectArgument);
        var corners = cornerDetector.Process();

        return corners;
    }

    public async Task MakeCornerAssistImageAsync(string imagePath, string outputPath, PointF[] corners, Func<PointF, (int Radius, Color color, int thickness)> circleInfo)
    {
        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException($"File not found: {imagePath}");
        }
        if (!CvInvoke.Init())
        {
            throw new Exception("Unable to initialize CvInvoke");
        }

        var outline = new Outline(imagePath);
        await outline.ProcessAsync();
        var outlineImage = outline.GetImage();

        foreach (var corner in corners)
        {
            var (radius, rgb, thickness) = circleInfo(corner);
            var color = new Rgb(rgb).MCvScalar;
            CvInvoke.Circle(outlineImage, corner.ToPoint(), radius, color, thickness);
        }

        if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        }
        CvInvoke.Imwrite(outputPath, outlineImage);
    }

    public async Task MakeOutlineImageAsync(string imagePath, string outputPath, int thickness = 1)
    {
        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException($"File not found: {imagePath}");
        }
        if (!CvInvoke.Init())
        {
            throw new Exception("Unable to initialize CvInvoke");
        }

        var outline = new Outline(imagePath);
        await outline.ProcessAsync();
        var outlineImage = outline.GetImage(thickness);

        if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        }

        CvInvoke.Imwrite(outputPath, outlineImage);
    }

    public bool IsRectangle(PointF[] points)
    {
        var d1 = Utils.Distance(points[0], points[1]);
        var d2 = Utils.Distance(points[1], points[2]);
        var d3 = Utils.Distance(points[2], points[3]);
        var d4 = Utils.Distance(points[3], points[0]);

        var diagonal1 = Utils.Distance(points[0], points[2]);
        var diagonal2 = Utils.Distance(points[1], points[3]);

        var ratio1 = Ratio(d1, d3);
        var ratio2 = Ratio(d2, d4);
        var ratio3 = Ratio(diagonal1, diagonal2);

        var isRectangle = ratio1 > 0.8 && ratio2 > 0.8 && ratio3 > 0.8;

        return isRectangle;

        double Ratio(double a, double b) => Math.Min(a, b) / Math.Max(a, b);
    }
}
