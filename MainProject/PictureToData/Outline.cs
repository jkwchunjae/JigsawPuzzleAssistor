using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureToData;

internal class Outline
{
    public string Name { get; init; }

    private VectorOfVectorOfPoint ContourVector;
    private int ContourIndex = -1;
    private Point[] Contour;
    private Image<Bgr, byte> _raw;
    private Image<Bgr, byte> _outlineImage;

    public Outline(string imagePath)
    {
        Name = Path.GetFileNameWithoutExtension(imagePath);
        Mat image = CvInvoke.Imread(imagePath);
        Image<Bgr, byte> rawImage = new Image<Bgr, byte>(image.Width, image.Height);
        image.CopyTo(rawImage);

        _raw = rawImage;
    }

    public Task ProcessAsync()
    {
        return Task.Run(() =>
        {
            var image = _raw;
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
            CvInvoke.FindContours(cannyResult, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxNone);

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
            // image에 outline을 그렸음
            // CvInvoke.DrawContours(image, contours, puzzle_contours, new MCvScalar(0, 255, 0), 2);

            var outline = new Image<Bgr, byte>(image.Width, image.Height, new Bgr(0, 0, 0));
            CvInvoke.DrawContours(outline, contours, puzzleContoursIndex, new MCvScalar(0, 255, 0), 1);


            var contour = new List<Point>();
            for (var i = 0; i < puzzleContours.Size; i++)
            {
                if (contour.Contains(puzzleContours[i]))
                {
                }
                else
                {
                    contour.Add(puzzleContours[i]);
                }
            }
            Contour = contour.ToArray();
            ContourVector = contours;
            ContourIndex = puzzleContoursIndex;
            _outlineImage = outline;
        });
    }

    public Image<Bgr, byte> GetImage(int thickness = 1)
    {
        if (_outlineImage == -1)
        {
            throw new Exception("run process");
        }
        var outlineImage = new Image<Bgr, byte>(_raw.Width, _raw.Height, new Bgr(0, 0, 0));
        CvInvoke.DrawContours(outlineImage, ContourVector, ContourIndex, new MCvScalar(0, 255, 0), thickness);
        return outlineImage;
        //if (_outlineImage == null)
        //{
        //    throw new Exception("run process");
        //}

        //return _outlineImage;
    }

    public Point[] GetContour()
    {
        if (Contour == null)
        {
            throw new Exception("run process");
        }

        return Contour;
    }
}
