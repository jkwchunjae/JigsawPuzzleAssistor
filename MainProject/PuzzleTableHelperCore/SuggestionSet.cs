
namespace PuzzleTableHelperCore;

public class SuggestionSet
{
    public required List<PuzzleCell> Cells { get; set; }

    public PuzzleCell? GetCell(int row, int column)
    {
        return Cells.FirstOrDefault(c => c.Row == row && c.Column == column);
    }
}