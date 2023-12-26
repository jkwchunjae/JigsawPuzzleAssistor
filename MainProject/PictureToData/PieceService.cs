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

        var cornerDetector = new CornerDetector(outline);
        var corners = cornerDetector.GetCornerWithArgument(detectArgument);

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
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        }
        CvInvoke.Imwrite(outputPath, outlineImage);
    }
}
