using System.Text.Json.Serialization;

namespace PuzzleTableHelperCore;

/// <summary>
/// 확정된 정보만 저장한다.
/// </summary>
[JsonConverter(typeof(PuzzleTableJsonConverter))]
public class PuzzleTable
{
    public required List<List<PuzzleCell?>> Cells { get; set; }

    public PuzzleCell? GetCell(int row, int column)
    {
        if (row < 0 || row >= Cells.Count)
            return null;
        if (column < 0 || column >= Cells[row].Count)
            return null;
        return Cells[row][column];
    }

    public void Append(PuzzleCell cell)
    {
        while (cell.Row >= Cells.Count)
        {
            Cells.Add(new List<PuzzleCell?>());
        }
        while (cell.Column >= Cells[cell.Row].Count)
        {
            Cells[cell.Row].Add(null);
        }
        Cells[cell.Row][cell.Column] = cell;
    }
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

    public int GetEdgeIndex((int row, int column) target)
    {
        var (row, column) = target;
        if (row == Row - 1 && column == Column)
            return TopEdgeIndex;
        if (row == Row && column == Column + 1)
            return RightEdgeIndex;
        if (row == Row + 1 && column == Column)
            return BottomEdgeIndex;
        if (row == Row && column == Column - 1)
            return LeftEdgeIndex;
        throw new Exception("invalid row, column: {row}, {column}");
    }

    public int CalcTargetTopIndex((int row, int column) target, int targetEdgeIndex)
    {
        var (row, column) = target;
        if (row == Row - 1 && column == Column)
            return (targetEdgeIndex + 2) % 4;
        if (row == Row && column == Column + 1)
            return (targetEdgeIndex + 1) % 4;
        if (row == Row + 1 && column == Column)
            return targetEdgeIndex;
        if (row == Row && column == Column - 1)
            return (targetEdgeIndex + 3) % 4;
        throw new Exception("invalid row, column: {row}, {column}");
    }
}
