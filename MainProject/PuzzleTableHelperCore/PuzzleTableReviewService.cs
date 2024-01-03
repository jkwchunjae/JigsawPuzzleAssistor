
using JkwExtensions;

namespace PuzzleTableHelperCore;

public class ReviewCellData
{
    public required string PieceName { get; init; }
    public int Row { get; set; }
    public int Column { get; set; }
    public bool Valid { get; set; }
    public string Direction { get; set; }
    public int EdgeIndex { get; set; }
    public float DiffLength { get; set; }
    public float DiffRatio { get; set; }
}
public class PuzzleTableReviewService : PuzzleTableService
{
    public PuzzleTableReviewService(PuzzleTableServiceOption option) : base(option)
    {
    }

    public List<ReviewCellData> Review()
    {
        List<ReviewCellData> result = new();
        for (var row = 0; row < PuzzleTable.Cells.Count; row++)
        {
            for (var column = 0; column < PuzzleTable.Cells[row].Count; column++)
            {
                var cell = PuzzleTable.Cells[row][column];
                if (cell != null)
                {
                    var diffResult = ReviewCell(cell);
                    result.AddRange(diffResult);
                }
            }
        }
        return result;
    }

    private List<ReviewCellData> ReviewCell(PuzzleCell cell)
    {
        var result = new List<ReviewCellData>
        {
            new ReviewCellData { PieceName = cell.PieceName, Row = cell.Row, Column = cell.Column, Valid = false },
            new ReviewCellData { PieceName = cell.PieceName, Row = cell.Row, Column = cell.Column, Valid = false },
            new ReviewCellData { PieceName = cell.PieceName, Row = cell.Row, Column = cell.Column, Valid = false },
            new ReviewCellData { PieceName = cell.PieceName, Row = cell.Row, Column = cell.Column, Valid = false },
        };

        var pieceInfo = _pieceInfos.FirstOrDefault(x => x.Name == cell.PieceName);
        var connectionInfo = _connectInfos.FirstOrDefault(x => x.PieceName == cell.PieceName);
        var topCell = PuzzleTable.GetCell(cell.Row - 1, cell.Column);
        var rightCell = PuzzleTable.GetCell(cell.Row, cell.Column + 1);
        var bottomCell = PuzzleTable.GetCell(cell.Row + 1, cell.Column);
        var leftCell = PuzzleTable.GetCell(cell.Row, cell.Column - 1);

        if (pieceInfo == null || connectionInfo == null)
        {
            return result;
        }

        if (topCell != null)
        {
            var topInfo = _pieceInfos.FirstOrDefault(x => x.Name == topCell.PieceName);
            var topCell_BottomEdgeIndex = (topCell.TopEdgeIndex + 2) % 4;

            if (topInfo != null)
            {
                var diffLength = topInfo.Edges[topCell_BottomEdgeIndex].DiffLength(pieceInfo.Edges[cell.TopEdgeIndex]);
                var diffRatio = connectionInfo.Edges[cell.TopEdgeIndex].Connection.FirstOrDefault(x => x.PieceName == topCell.PieceName)?.Value;

                result[cell.TopEdgeIndex].Valid = true;
                result[cell.TopEdgeIndex].Direction = "top";
                result[cell.TopEdgeIndex].EdgeIndex = cell.TopEdgeIndex;
                result[cell.TopEdgeIndex].DiffLength = (float)Math.Round(diffLength * 100, 2);
                result[cell.TopEdgeIndex].DiffRatio = diffRatio ?? 0;
            }
        }

        if (rightCell != null)
        {
            var rightInfo = _pieceInfos.FirstOrDefault(x => x.Name == rightCell.PieceName);
            var rightCell_LeftEdgeIndex = (rightCell.TopEdgeIndex + 3) % 4;

            if (rightInfo != null)
            {
                var diffLength = rightInfo.Edges[rightCell_LeftEdgeIndex].DiffLength(pieceInfo.Edges[(cell.TopEdgeIndex + 1) % 4]);
                var diffRatio = connectionInfo.Edges[(cell.TopEdgeIndex + 1) % 4].Connection.FirstOrDefault(x => x.PieceName == rightCell.PieceName)?.Value;

                result[(cell.TopEdgeIndex + 1) % 4].Valid = true;
                result[(cell.TopEdgeIndex + 1) % 4].Direction = "right";
                result[(cell.TopEdgeIndex + 1) % 4].EdgeIndex = (cell.TopEdgeIndex + 1) % 4;
                result[(cell.TopEdgeIndex + 1) % 4].DiffLength = (float)Math.Round(diffLength * 100, 2);
                result[(cell.TopEdgeIndex + 1) % 4].DiffRatio = diffRatio ?? 0;
            }
        }

        if (bottomCell != null)
        {
            var bottomInfo = _pieceInfos.FirstOrDefault(x => x.Name == bottomCell.PieceName);
            var bottomCell_TopEdgeIndex = (bottomCell.TopEdgeIndex + 0) % 4;

            if (bottomInfo != null)
            {
                var diffLength = bottomInfo.Edges[bottomCell_TopEdgeIndex].DiffLength(pieceInfo.Edges[(cell.TopEdgeIndex + 2) % 4]);
                var diffRatio = connectionInfo.Edges[(cell.TopEdgeIndex + 2) % 4].Connection.FirstOrDefault(x => x.PieceName == bottomCell.PieceName)?.Value;

                result[(cell.TopEdgeIndex + 2) % 4].Valid = true;
                result[(cell.TopEdgeIndex + 2) % 4].Direction = "bottom";
                result[(cell.TopEdgeIndex + 2) % 4].EdgeIndex = (cell.TopEdgeIndex + 2) % 4;
                result[(cell.TopEdgeIndex + 2) % 4].DiffLength = (float)Math.Round(diffLength * 100, 2);
                result[(cell.TopEdgeIndex + 2) % 4].DiffRatio = diffRatio ?? 0;
            }
        }

        if (leftCell != null)
        {
            var leftInfo = _pieceInfos.FirstOrDefault(x => x.Name == leftCell.PieceName);
            var leftCell_RightEdgeIndex = (leftCell.TopEdgeIndex + 1) % 4;

            if (leftInfo != null)
            {
                var diffLength = leftInfo.Edges[leftCell_RightEdgeIndex].DiffLength(pieceInfo.Edges[(cell.TopEdgeIndex + 3) % 4]);
                var diffRatio = connectionInfo.Edges[(cell.TopEdgeIndex + 3) % 4].Connection.FirstOrDefault(x => x.PieceName == leftCell.PieceName)?.Value;

                result[(cell.TopEdgeIndex + 3) % 4].Valid = true;
                result[(cell.TopEdgeIndex + 3) % 4].Direction = "left";
                result[(cell.TopEdgeIndex + 3) % 4].EdgeIndex = (cell.TopEdgeIndex + 3) % 4;
                result[(cell.TopEdgeIndex + 3) % 4].DiffLength = (float)Math.Round(diffLength * 100, 2);
                result[(cell.TopEdgeIndex + 3) % 4].DiffRatio = diffRatio ?? 0;
            }
        }

        return result;
    }
}