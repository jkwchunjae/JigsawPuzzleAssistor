using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureToData;

internal class CornerDetector
{
    public string Name => Outline.Name;
    public Outline Outline { get; init; }
    private PointF[] Corners { get; set; }

    public CornerDetector(Outline outline)
    {
        Outline = outline;

        Process();
    }

    private void Process()
    {
        if (Corners != null)
            return;

        var d = new Emgu.CV.Features2D.GFTTDetector(4, 0.01, 200, 9);
        var corners = d.Detect(Outline.GetImage());

        // 점을 시계방향으로 정렬
        var baseCorner = corners.OrderBy(c => c.Point.Y).First();
        var otherCorners = corners.OrderBy(c => c.Point.Y).Skip(1).ToArray();
        otherCorners = otherCorners
            .Select(c => new { Corner = c, Angle = Utils.CalculateAngleBetweenPoints(baseCorner.Point, c.Point) })
            .OrderBy(c => c.Angle)
            .Select(x => x.Corner)
            .ToArray();
        var orderedCorner = new[] { baseCorner }.Concat(otherCorners).ToArray();

        Corners = orderedCorner
            .Select(c => c.Point)
            .Select(p => (PointF)NewCorner3(p.ToPoint()))
            .ToArray();
    }


    public Point NewCorner3(Point corner)
    {
        // CvInvoke.Circle(image, corner, 1, new MCvScalar(0, 255, 0), -1);

        var near = Outline.GetContour()
            .Where(c => Utils.Distance(corner, c) < 30)
            .Select((c, i) => new
            {
                Point = c,
                Index = i,
                DistanceFromCorner = Utils.Distance(corner, c),
            })
            .ToArray();

        foreach (var point in near)
        {
            //image[point.Point] = new Bgr(Color.LightGreen);
        }

        var minPoint = new Point(near.Min(p => p.Point.X) - 10, near.Min(p => p.Point.Y) - 10);
        var maxPoint = new Point(near.Max(p => p.Point.X) + 10, near.Max(p => p.Point.Y) + 10);

        var orderedNear = near.OrderByDescending(x => x.DistanceFromCorner).ToArray();
        Point farPoint1 = orderedNear.First().Point;
        Point farPoint2 = orderedNear
            .Where(x => Utils.Distance(x.Point, farPoint1) > 20)
            .First()
            .Point;

        var line1 = MedianLine(farPoint1);
        //DrawLine(line1);
        var line2 = MedianLine(farPoint2);
        //DrawLine(line2);

        //CvInvoke.Circle(image, farPoint1, 2, new MCvScalar(255, 0, 0), -1);
        //CvInvoke.Circle(image, farPoint2, 2, new MCvScalar(255, 255, 0), -1);

        var intersection = CalculateIntersection(line1, line2);
        //CvInvoke.Circle(image, intersection.ToPoint(), 1, new MCvScalar(255, 0, 255), -1);

        return intersection.ToPoint();

        void DrawLine(LineData line)
        {
            if (line.horizotal)
            {
                var p1 = new Point((int)line.xValue, minPoint.Y);
                var p2 = new Point((int)line.xValue, maxPoint.Y);
                //CvInvoke.Line(image, p1, p2, new MCvScalar(0, 255, 255), 1);
            }
            else
            {
                var p1 = new Point(minPoint.X, (int)(line.angle * minPoint.X + line.yValue));
                var p2 = new Point(maxPoint.X, (int)(line.angle * maxPoint.X + line.yValue));
                //CvInvoke.Line(image, p1, p2, new MCvScalar(0, 255, 255), 1);
            }
        }

        LineData MedianLine(Point farPoint)
        {
            var fromMe = near
                .OrderBy(x => Utils.Distance(farPoint, x.Point))
                .Where(x => Utils.Distance(farPoint, x.Point) > 10)
                .ToList();
            //.SkipWhile(x => Distance(farPoint1, x.Point) < 10)
            //.TakeWhile(x => x.DistanceFromCorner > 2)
            //.ToArray();
            var firstTarget = fromMe.First();
            var targets = new[] { firstTarget }.ToList();
            while (fromMe.Any())
            {
                var last = targets.Last();
                var closeFromLast = fromMe
                    .OrderBy(x => Utils.Distance(last.Point, x.Point))
                    .First();

                if (closeFromLast.DistanceFromCorner < 4)
                    break;
                targets.Add(closeFromLast);
                fromMe.Remove(closeFromLast);
            }

            foreach (var point in targets)
            {
                //image[point.Point] = new Bgr(Color.Red);
            }
            var lines = targets
                .Select(target => MakeLine(farPoint, target.Point))
                .ToArray();

            // v1 median
            var medianLine = lines
                .OrderBy(line => line.angle)
                .ToArray()
                [lines.Count() / 2];
            return medianLine;

            // v2 average
            //var avgAngle = lines.Select(line => line.angle).Average();
            //
            //if (avgAngle > 99999)
            //{
            //	var xValue = lines.Where(line => line.horizotal).Select(line => line.xValue).Average();
            //	return new LineData(999999, 0, true, xValue);
            //}
            //else
            //{
            //	var avgYValue = lines.Select(line => line.yValue).Average();
            //	return new LineData(avgAngle, avgYValue, false, 0);
            //}
        }

        LineData MakeLine(PointF p1, PointF p2)
        {
            if (p1.X == p2.X)
            {
                return new LineData(999999, 0, true, p1.X);
            }
            else
            {
                double m = (p2.Y - p1.Y) / (p2.X - p1.X);
                double b = p1.Y - m * p1.X;
                return new LineData(m, b, false, 0);
            }
        }
    }

    public PointF[] GetCorners()
    {
        return Corners;
    }

    public void WriteTo(Image<Bgr, byte> image, int radius)
    {
        foreach (var corner in Corners)
        {
            CvInvoke.Circle(image, corner.ToPoint(), radius, new MCvScalar(255, 0, 255), 1);
        }
    }

    record LineData(double angle, double yValue, bool horizotal, double xValue);

    private PointF CalculateIntersection(LineData line1, LineData line2)
    {
        if (line1.horizotal)
        {
            var x = line1.xValue;
            var y = line2.angle * x + line2.yValue;
            return new PointF((float)x, (float)y);
        }
        else if (line2.horizotal)
        {
            var x = line2.xValue;
            double y = line1.angle * x + line1.yValue;

            return new PointF((float)x, (float)y);
        }
        else
        {
            double x = (line2.yValue - line1.yValue) / (line1.angle - line2.angle);
            double y = line1.angle * x + line1.yValue;

            return new PointF((float)x, (float)y);
        }
    }
}
