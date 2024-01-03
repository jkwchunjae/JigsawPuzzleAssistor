using System.Drawing;
using System.Text.Json.Serialization;

namespace Common.PieceInfo;

public class EdgeInfo
{
    //public required Point[] OriginPoints { get; set; }
    //public required PointF OriginCorner1 { get; set; }
    //public required PointF OriginCorner2 { get; set; }
    public required PointF[] NormalizedPoints { get; set; }
    //public required PointF NormalizedCorner1 { get; set; }
    //public required PointF NormalizedCorner2 { get; set; }
    public required float Length { get; set; }
    public required EdgeType Type { get; set; }

    [JsonIgnore] public bool IsLine => Type == EdgeType.Line;
    [JsonIgnore] public bool IsHead => Type == EdgeType.Head;
    [JsonIgnore] public bool IsHole => Type == EdgeType.Hole;

    private Dictionary<EdgeType, EdgeType> AllowTypeMap = new()
    {
        { EdgeType.Head, EdgeType.Hole },
        { EdgeType.Hole, EdgeType.Head },
        { EdgeType.Line, EdgeType.None },
    };

    public (bool Result, float Value) Test(EdgeInfo other)
    {
        var allowType = AllowTypeMap[Type];
        if (other.Type != allowType)
        {
            return (false, 0);
        }

        if (!CheckLength(Length, other.Length))
        {
            // 길이가 3% 이내로 차이나면 true
            return (false, 0);
        }

        var shortPoints = NormalizedPoints.Length < other.NormalizedPoints.Length ? NormalizedPoints : other.NormalizedPoints;
        var longPoints = NormalizedPoints.Length < other.NormalizedPoints.Length ? other.NormalizedPoints : NormalizedPoints;

        float distanceSum = shortPoints
            .Sum(left => longPoints.Min(right => Distance(left, right)));
        var result = distanceSum / shortPoints.Length;

        if (result < 5)
        {
            // 5 이내로 차이나면 true
            return (true, result);
        }
        else
        {
            return (false, 0);
        }

        bool CheckLength(float a, float b)
        {
            // 3% 이내로 차이나면 true
            var min = Math.Min(a, b);
            var max = Math.Max(a, b);
            return (max - min) / min < 0.03;
        }

        float Distance(PointF a, PointF b)
        {
            var x = a.X - b.X;
            var y = a.Y - b.Y;
            return (float)Math.Sqrt(x * x + y * y);
        }
    }

    public float DiffLength(EdgeInfo other)
    {
        var shortLength = Math.Min(Length, other.Length);
        var longLength = Math.Max(Length, other.Length);
        return (longLength - shortLength) / shortLength;
    }
}

