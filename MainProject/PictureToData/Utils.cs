using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureToData;

internal static class Utils
{
    public static double CalculateAngleBetweenPoints(PointF point1, PointF point2)
    {
        double deltaX = point2.X - point1.X;
        double deltaY = point2.Y - point1.Y;

        // 아크탄젠트 함수를 사용하여 각도를 계산합니다.
        double angleInRadians = Math.Atan2(deltaY, deltaX);

        return angleInRadians;
    }

    public static double CalculateDistance(PointF p1, PointF p2, PointF p3)
    {
        double px = p3.X - p2.X;
        double py = p3.Y - p2.Y;
        double something = px * px + py * py;
        double u = ((p1.X - p2.X) * px + (p1.Y - p2.Y) * py) / something;

        if (u > 1)
        {
            u = 1;
        }
        else if (u < 0)
        {
            u = 0;
        }

        double x = p2.X + u * px;
        double y = p2.Y + u * py;
        double dx = x - p1.X;
        double dy = y - p1.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        return dist;
    }

    public static PointF RotatePointAroundOrigin(PointF point, double angleRad)
    {
        var x = point.X;
        var y = point.Y;

        var rotatedX = x * Math.Cos(angleRad) - y * Math.Sin(angleRad);
        var rotatedY = x * Math.Sin(angleRad) + y * Math.Cos(angleRad);

        return new PointF((float)rotatedX, (float)rotatedY);
    }

    public static double Distance(PointF p1, PointF p2)
    {
        double dx = p2.X - p1.X;
        double dy = p2.Y - p1.Y;

        return Math.Sqrt(dx * dx + dy * dy);
    }

    public static Point ToPoint(this PointF point)
    {
        return new Point((int)point.X, (int)point.Y);
    }

    public static MCvScalar ToScalar(this Bgr bgr)
    {
        return new MCvScalar(bgr.Blue, bgr.Green, bgr.Red, 0); // The fourth value is alpha, typically 0
    }

    public static Bgr ToBgr(this MCvScalar scalar)
    {
        return new Bgr(scalar.V2, scalar.V1, scalar.V0);
    }

    public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var found = source.Select((x, i) => new { x, i })
            .FirstOrDefault(x => predicate(x.x));
        return found?.i ?? -1;
    }
}
