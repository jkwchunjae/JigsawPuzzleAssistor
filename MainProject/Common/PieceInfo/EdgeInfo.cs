using System.Drawing;

namespace Common.PieceInfo;

public class EdgeInfo
{
    public required PointF[] OriginPoints { get; set; }
    public required PointF OriginCorner1 { get; set; }
    public required PointF OriginCorner2 { get; set; }
    public required PointF[] NormalizedPoints { get; set; }
    public required PointF NormalizedCorner1 { get; set; }
    public required PointF NormalizedCorner2 { get; set; }
    public required EdgeType Type { get; set; }
}

