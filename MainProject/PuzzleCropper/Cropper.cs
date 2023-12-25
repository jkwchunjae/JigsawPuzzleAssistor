using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;

namespace PuzzleCropper;

public class Cropper
{
    public async Task CropUsingOutline(string inputPath, string outputPath, Rectangle initRoi)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException($"File not found: {inputPath}");
        }
        if (!CvInvoke.Init())
        {
            throw new Exception("Unable to initialize CvInvoke");
        }

        var firstCropped = await CropAsync(inputPath, initRoi);
        var outline = await GetOutlineAsync(firstCropped);
        var croppedRoi = GetRectangleFromOutline(outline);

        var padding = 30;
        var newRoi = new Rectangle(
            initRoi.X + croppedRoi.X - padding,
            initRoi.Y + croppedRoi.Y - padding,
            croppedRoi.Width + padding * 2,
            croppedRoi.Height + padding * 2
            );
        var secondCropped = await CropAsync(inputPath, newRoi);

        CvInvoke.Imwrite(outputPath, secondCropped);
    }

    private Task<Mat> CropAsync(string inputPath, Rectangle roi)
    {
        return Task.Run(() =>
        {
            Mat image = CvInvoke.Imread(inputPath);
            var ratio = 1.0;
            Image<Gray, byte> resized = new Image<Gray, byte>(image.Width, image.Height);
            CvInvoke.Resize(image, resized, new Size(0, 0), ratio, ratio, Inter.Area);
            var cropped = new Mat(resized.Mat, roi);
            return cropped;
        });
    }

    private Task<VectorOfPoint> GetOutlineAsync(Mat inputImage)
    {
        return Task.Run(() =>
        {
            Image<Bgr, byte> image = inputImage.ToImage<Bgr, byte>();

            Image<Gray, byte> gray = new Image<Gray, byte>(image.Width, image.Height);
            CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
            Image<Gray, byte> gaussian = new Image<Gray, byte>(image.Width, image.Height);
            CvInvoke.GaussianBlur(gray, gaussian, new Size(5, 5), 0);
            Image<Gray, byte> blackwhite = new Image<Gray, byte>(image.Width, image.Height);
            CvInvoke.Threshold(gaussian, blackwhite, 127, 255, ThresholdType.Binary);

            Image<Gray, byte> cannyResult = new Image<Gray, byte>(image.Width, image.Height);
            CvInvoke.Canny(blackwhite, cannyResult, 50, 255);

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(cannyResult, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxSimple);

            VectorOfPoint puzzleContours = contours[0];
            int puzzleContoursIndex = 0;
            for (var i = 0; i < contours.Size; i++)
            {
                if (puzzleContours.Size < contours[i].Size)
                {
                    puzzleContours = contours[i];
                    puzzleContoursIndex = i;
                }
            }

            return puzzleContours;
        });
    }

    private Rectangle GetRectangleFromOutline(VectorOfPoint outline)
    {
        var minX = outline[0].X;
        var maxX = outline[0].X;
        var minY = outline[0].Y;
        var maxY = outline[0].Y;

        for (var i = 1; i < outline.Size; i++)
        {
            var x = outline[i].X;
            var y = outline[i].Y;
            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);
        }

        var width = maxX - minX + 1;
        var height = maxY - minY + 1;

        var roi = new Rectangle(minX, minY, width, height);
        return roi;
    }
}
