using System.Text.Json.Serialization;

namespace PuzzleTableHelperCore;

[JsonConverter(typeof(PuzzleTableJsonConverter))]
public class PuzzleTable
{
    public required List<List<PuzzleCell>> Cells { get; set; }
}

public class PuzzleCell
{
    public required int Row { get; set; }
    public required int Column { get; set; }
    public required string PieceName { get; set; }
    public required int PieceNumber { get; set; }
    public required int TopEdgeIndex { get; set; }
    public int RightEdgeIndex => (TopEdgeIndex + 1) % 4;
    public int BottomEdgeIndex => (TopEdgeIndex + 2) % 4;
    public int LeftEdgeIndex => (TopEdgeIndex + 3) % 4;
}
