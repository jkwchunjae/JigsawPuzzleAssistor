using System.Drawing;

namespace Common.PieceInfo;

public class PieceInfo
{
    public required PointF[] Outline { get; set; }
    public required PointF[] Corners { get; set; }
    public required List<EdgeInfo> Edges { get; set; }
}

