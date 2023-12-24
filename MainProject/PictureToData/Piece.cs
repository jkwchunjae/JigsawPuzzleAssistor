using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureToData;

internal class Piece
{
    public string Name => Outline.Name;
    public Outline Outline { get; init; }
    public List<Edge> Edges { get; init; }
    public Point[] FirstContour { get; init; }
    public Point[] OrderContour1 { get; init; }

    public Piece(Outline outline, PointF[] corners)
    {
        if (corners.Count() != 4)
        {
            throw new ArgumentException("코너는 4개여야 합니다.");
        }
        Outline = outline;
        var contour = outline.GetContour();

        // 1. 첫 코너에서 가장 가까운 점을 고른다.
        // 2. 그 점에서 가장 가까운 점을 고른다. 계속 반복한다. (외곽점)
        // 3. 외곽점의 순서를 코너의 순서와 맞춘다.
        // 4. 외곽점을 코너에 맞춰서 나눈다. Edge를 만든다.

        // 1. 첫 코너에서 가장 가까운 점을 고른다.
        var nearests = corners
            .Select(corner => FindNearestPoint(contour, corner))
            .ToArray();
        // 2. 그 점에서 가장 가까운 점을 고른다. 계속 반복한다. (외곽점)
        var orderedContour = ReorderContour(contour, nearests[0]);
        // 3. 외곽점의 순서를 코너의 순서와 맞춘다.
        var contourIndex1 = orderedContour.FindIndex(p => p == nearests[1]);
        var contourIndex2 = orderedContour.FindIndex(p => p == nearests[2]);
        if (contourIndex1 > contourIndex2)
        {
            // contour의 순서와 corner의 순서가 반대다.
            // corner의 순서는 바뀌면 안된다. contour를 뒤집어서 사용한다.
            orderedContour = new[] { orderedContour.First() }
                .Concat(orderedContour.Skip(1).Reverse())
                .ToArray();
            nearests = corners
                .Select(corner => FindNearestPoint(orderedContour, corner))
                .ToArray();
        }
        // 4. 외곽점을 코너에 맞춰서 나눈다. Edge를 만든다.
        Edges = corners
            .Select((corner, cornerIndex) =>
            {
                var nextCorner = corners[(cornerIndex + 1) % 4];
                var beginContourPoint = nearests[cornerIndex];
                var endContourPoint = nearests[(cornerIndex + 1) % 4];
                var beginIndex = orderedContour.FindIndex(p => p == beginContourPoint);
                var endIndex = orderedContour.FindIndex(p => p == endContourPoint);

                var points = orderedContour
                    .Skip(beginIndex == 0 ? 0 : beginIndex - 1)
                    .Take(endIndex == 0 ? orderedContour.Count() - beginIndex : endIndex - beginIndex + 1)
                    .ToList();
                if (endIndex == 0)
                {
                    points.Add(orderedContour.First());
                }

                return new Edge(points, corner, nextCorner);
            })
            .ToList();
    }

    // 1. 첫 코너에서 가장 가까운 점을 고른다.
    private static Point FindNearestPoint(Point[] contour, PointF corner)
    {
        return contour
            .OrderBy(p => Utils.Distance(p, corner))
            .First();
    }

    // 2. 그 점에서 가장 가까운 점을 고른다. 계속 반복한다. (외곽점)
    private static Point[] ReorderContour(Point[] contour, Point first)
    {
        var bag = contour.ToList();
        var result = new List<Point>();

        while (bag.Any())
        {
            var curr = result.Any() ? result.Last() : first;
            var ordered = bag
                .Select(other => new { Point = other, Distance = Utils.Distance(curr, other) })
                .OrderBy(x => x.Distance);

            var nearest = ordered.First();
            bag.Remove(nearest.Point);
            result.Add(nearest.Point);
        }
        return result.ToArray();
    }

    public (bool, double, Edge, Edge) Test(Piece other, double threshold)
    {
        var tests = new List<(int, Edge, Edge)>();
        foreach (var hole in Edges.Where(edge => edge.IsHole))
        {
            foreach (var head in other.Edges.Where(edge => edge.IsHead))
            {
                var value = hole.Test(head);
                tests.Add(((int)value, hole, head));
            }
        }
        foreach (var head in Edges.Where(edge => edge.IsHead))
        {
            foreach (var hole in other.Edges.Where(edge => edge.IsHole))
            {
                var value = head.Test(hole);
                tests.Add(((int)value, head, hole));
            }
        }
        var min = tests.Select(x => x.Item1).Min();
        var edge1 = tests.First(x => x.Item1 == min).Item2;
        var edge2 = tests.First(x => x.Item1 == min).Item3;
        var success = min < threshold;
        return (success, min, edge1, edge2);
    }
}
