using System.Text.Json;
using PuzzleTableHelperCore;

namespace zTest_PuzzleTableConverter;

public class PuzzleTableConverterTest
{
    [Fact]
    public void PuzzleTable_serialize_test()
    {
        var table = new PuzzleTable
        {
            Cells = new List<List<PuzzleCell?>>
            {
                new List<PuzzleCell?>
                {
                    new PuzzleCell { Row = 0, Column = 0, PieceName = "puzzle_00000", PieceNumber = 0, TopEdgeIndex = 0 },
                    new PuzzleCell { Row = 0, Column = 1, PieceName = "puzzle_00009", PieceNumber = 9, TopEdgeIndex = 1 },
                    new PuzzleCell { Row = 0, Column = 2, PieceName = "puzzle_00002", PieceNumber = 2, TopEdgeIndex = 3 },
                    new PuzzleCell { Row = 0, Column = 3, PieceName = "puzzle_00003", PieceNumber = 3, TopEdgeIndex = 2 },
                },
                new List<PuzzleCell?>
                {
                    new PuzzleCell { Row = 1, Column = 0, PieceName = "puzzle_00010", PieceNumber = 10, TopEdgeIndex = 0 },
                    new PuzzleCell { Row = 1, Column = 1, PieceName = "puzzle_00019", PieceNumber = 19, TopEdgeIndex = 1 },
                    new PuzzleCell { Row = 1, Column = 2, PieceName = "puzzle_00012", PieceNumber = 12, TopEdgeIndex = 3 },
                    new PuzzleCell { Row = 1, Column = 3, PieceName = "puzzle_00013", PieceNumber = 13, TopEdgeIndex = 2 },
                },
            }
        };

        var option = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        var tableJsonText = JsonSerializer.Serialize(table, option);

        var expected = @"[
  ""(0,0) (9,1) (2,3) (3,2)"",
  ""(10,0) (19,1) (12,3) (13,2)""
]";
        Assert.Equal(expected, tableJsonText);
    }

    [Fact]
    public void PuzzleTable_deserialize_test()
    {
        var tableJson = @"[
  ""(0,0) (9,1) (2,3) (3,2)"",
  ""(10,0) (19,1) (12,3) (13,2)""
]";
        var table = JsonSerializer.Deserialize<PuzzleTable>(tableJson);

        Assert.Equal(2, table!.Cells.Count);
        Assert.Equal(4, table.Cells[0].Count);
        Assert.Equal(4, table.Cells[1].Count);

        Assert.Equal(0, table.Cells[0][0]!.Row);
        Assert.Equal(0, table.Cells[0][0]!.Column);
        Assert.Equal("puzzle_00000", table.Cells[0][0]!.PieceName);
        Assert.Equal(0, table.Cells[0][0]!.PieceNumber);
        Assert.Equal(0, table.Cells[0][0]!.TopEdgeIndex);

        Assert.Equal(0, table.Cells[0][1]!.Row);
        Assert.Equal(1, table.Cells[0][1]!.Column);
        Assert.Equal("puzzle_00009", table.Cells[0][1]!.PieceName);
        Assert.Equal(9, table.Cells[0][1]!.PieceNumber);
        Assert.Equal(1, table.Cells[0][1]!.TopEdgeIndex);

        Assert.Equal(0, table.Cells[0][2]!.Row);
        Assert.Equal(2, table.Cells[0][2]!.Column);
        Assert.Equal("puzzle_00002", table.Cells[0][2]!.PieceName);
        Assert.Equal(2, table.Cells[0][2]!.PieceNumber);
        Assert.Equal(3, table.Cells[0][2]!.TopEdgeIndex);

        Assert.Equal(0, table.Cells[0][3]!.Row);
        Assert.Equal(3, table.Cells[0][3]!.Column);
        Assert.Equal("puzzle_00003", table.Cells[0][3]!.PieceName);
        Assert.Equal(3, table.Cells[0][3]!.PieceNumber);
        Assert.Equal(2, table.Cells[0][3]!.TopEdgeIndex);

        Assert.Equal(1, table.Cells[1][0]!.Row);
        Assert.Equal(0, table.Cells[1][0]!.Column);
        Assert.Equal("puzzle_00010", table.Cells[1][0]!.PieceName);
        Assert.Equal(10, table.Cells[1][0]!.PieceNumber);
        Assert.Equal(0, table.Cells[1][0]!.TopEdgeIndex);

        Assert.Equal(1, table.Cells[1][1]!.Row);
        Assert.Equal(1, table.Cells[1][1]!.Column);
        Assert.Equal("puzzle_00019", table.Cells[1][1]!.PieceName);
        Assert.Equal(19, table.Cells[1][1]!.PieceNumber);
        Assert.Equal(1, table.Cells[1][1]!.TopEdgeIndex);

        Assert.Equal(1, table.Cells[1][2]!.Row);
        Assert.Equal(2, table.Cells[1][2]!.Column);
        Assert.Equal("puzzle_00012", table.Cells[1][2]!.PieceName);
        Assert.Equal(12, table.Cells[1][2]!.PieceNumber);
        Assert.Equal(3, table.Cells[1][2]!.TopEdgeIndex);

        Assert.Equal(1, table.Cells[1][3]!.Row);
        Assert.Equal(3, table.Cells[1][3]!.Column);
        Assert.Equal("puzzle_00013", table.Cells[1][3]!.PieceName);
        Assert.Equal(13, table.Cells[1][3]!.PieceNumber);
        Assert.Equal(2, table.Cells[1][3]!.TopEdgeIndex);
    }

    [Fact]
    public void EmptyPuzzleTable_serialize_test()
    {
        var table = new PuzzleTable
        {
            Cells = new List<List<PuzzleCell?>>(),
        };

        var option = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        var tableJsonText = JsonSerializer.Serialize(table, option);

        var expected = @"[]";
        Assert.Equal(expected, tableJsonText);
    }

    [Fact]
    public void EmptyPuzzleTable_deserialize_test()
    {
        var tableJson = @"[]";
        var table = JsonSerializer.Deserialize<PuzzleTable>(tableJson);

        Assert.Empty(table!.Cells);
    }

    [Fact]
    public void PuzzleTable_null_serialize_test()
    {
        var table = new PuzzleTable
        {
            Cells = new List<List<PuzzleCell?>>
            {
                new List<PuzzleCell?>
                {
                    new PuzzleCell { Row = 0, Column = 0, PieceName = "puzzle_00000", PieceNumber = 0, TopEdgeIndex = 0 },
                    null,
                    new PuzzleCell { Row = 0, Column = 2, PieceName = "puzzle_00002", PieceNumber = 2, TopEdgeIndex = 3 },
                },
            }
        };

        var option = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        var tableJsonText = JsonSerializer.Serialize(table, option);

        var expected = @"[
  ""(0,0) null (2,3)""
]";
        Assert.Equal(expected, tableJsonText);
    }

    [Fact]
    public void PuzzleTable_null_deserialize_test()
    {
        var tableJson = @"[
  ""(0,0) null (2,3)""
]";
        var table = JsonSerializer.Deserialize<PuzzleTable>(tableJson);

        Assert.Single(table!.Cells);
        Assert.Equal(3, table.Cells[0].Count);

        Assert.Equal(0, table.Cells[0][0]!.Row);
        Assert.Equal(0, table.Cells[0][0]!.Column);
        Assert.Equal("puzzle_00000", table.Cells[0][0]!.PieceName);
        Assert.Equal(0, table.Cells[0][0]!.PieceNumber);
        Assert.Equal(0, table.Cells[0][0]!.TopEdgeIndex);

        Assert.Null(table.Cells[0][1]);

        Assert.Equal(0, table.Cells[0][2]!.Row);
        Assert.Equal(2, table.Cells[0][2]!.Column);
        Assert.Equal("puzzle_00002", table.Cells[0][2]!.PieceName);
        Assert.Equal(2, table.Cells[0][2]!.PieceNumber);
        Assert.Equal(3, table.Cells[0][2]!.TopEdgeIndex);
    }
}