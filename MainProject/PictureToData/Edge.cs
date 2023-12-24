using Common.PieceInfo;
using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureToData;

internal class Edge
{
    public PointF[] OriginPoints;
    public PointF OriginCorner1;
    public PointF OriginCorner2;

    public PointF[] NormalizedPoints;
    public PointF NormalizedCorner1;
    public PointF NormalizedCorner2;

    public EdgeType Type;
    public Edge(IEnumerable<PointF> points, PointF corner1, PointF corner2)
    {
        OriginPoints = points.ToArray();
        OriginCorner1 = corner1;
        OriginCorner2 = corner2;

        var normalized = Normalize(OriginPoints, corner1, corner2);
        NormalizedPoints = normalized.Points;
        NormalizedCorner1 = normalized.Corner1;
        NormalizedCorner2 = normalized.Corner2;

        Type = GetType(NormalizedPoints);
        if (IsHole)
        {
            NormalizedPoints = Reverse(NormalizedPoints, NormalizedCorner1, NormalizedCorner2);
        }
    }
    public Edge(IEnumerable<Point> points, Point corner1, Point corner2)
        : this(points.Select(p => (PointF)p), corner1, corner2)
    {
    }

    public bool IsHead => Type == EdgeType.Head;
    public bool IsHole => Type == EdgeType.Hole;
    public bool IsLine => Type == EdgeType.Line;

    private static EdgeType GetType(PointF[] normalizedPoint)
    {
        var maxY = Math.Abs(normalizedPoint.Max(p => p.Y));
        var minY = Math.Abs(normalizedPoint.Min(p => p.Y));

        if (maxY < 30 && minY < 30)
        {
            return EdgeType.Line;
        }
        else if (maxY > minY)
        {
            return EdgeType.Hole;
        }
        else if (maxY < minY)
        {
            return EdgeType.Head;
        }
        throw new Exception();
    }

    public double Test(Edge other)
    {
        //if (Math.Abs(NormalizedCorner2.X - other.NormalizedCorner2.X) > 5)
        //	return 9999;

        double distanceSum = NormalizedPoints
            .Sum(thisPoint => other.NormalizedPoints.Min(otherPoint => Utils.Distance(thisPoint, otherPoint)));

        return distanceSum;
    }

    public static (PointF[] Points, PointF Corner1, PointF Corner2) Normalize(IEnumerable<PointF> points, PointF corner1, PointF corner2)
    {
        var angle = Utils.CalculateAngleBetweenPoints(corner1, corner2);
        var result = points
            .Select(point => new PointF(point.X - corner1.X, point.Y - corner1.Y))
            .Select(point => Utils.RotatePointAroundOrigin(point, -angle))
            .ToArray();
        var newCorner2 = Utils.RotatePointAroundOrigin(new PointF(corner2.X - corner1.X, corner2.Y - corner1.Y), -angle);
        return (result, new PointF(0, 0), newCorner2);
    }

    public static PointF[] Reverse(PointF[] points, PointF corner1, PointF corner2)
    {
        var angle = Utils.CalculateAngleBetweenPoints(corner2, corner1);
        var reversed = points
            .Select(p => new PointF(p.X - corner2.X, p.Y - corner2.Y))
            .Select(p => Utils.RotatePointAroundOrigin(p, -angle))
            .ToArray();
        return reversed;
    }

    public void NormalizedPointPrintTo(Image<Bgr, byte> board, Point basePoint, Bgr color)
    {
        foreach (var point in NormalizedPoints)
        {
            var x = point.X + basePoint.X;
            var y = point.Y + basePoint.Y;
            var newPoint = new Point((int)x, (int)y);

            // CvInvoke.Circle(board, , 1, color, -1);
            board[newPoint] = color;
        }
    }

    public void OriginPointPrintTo(Image<Bgr, byte> image, Bgr color)
    {
        foreach (var point in OriginPoints)
        {
            //image[point.ToPoint()] = color;
            CvInvoke.Circle(image, point.ToPoint(), 1, color.ToScalar(), -1);
        }
    }
}
