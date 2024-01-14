using System.Drawing;
using System.Text.Json.Serialization;
using JkwExtensions;

namespace Common.PieceInfo;

public class PieceInfo
{
    public required string Name { get; set; }
    //public required Point[] Outline { get; set; }
    public PointF[]? Corners { get; set; }
    public PointF[]? PredefinedCorners { get; set; }
    public List<EdgeInfo>? Edges { get; set; }

    [JsonIgnore]
    public int Number => Name[(Name.IndexOf('_') + 1)..].ToInt();
}

