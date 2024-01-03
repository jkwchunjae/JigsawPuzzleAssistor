
using PuzzleTableHelperCore;

namespace MainCli;

public class ReviewPuzzleTable : IMainRunner
{
    public async Task Run()
    {
        var service = new PuzzleTableReviewService(new PuzzleTableServiceOption
        {
            PieceInfoDirectory = @"/Users/jkwchunjae/Documents/GitHub/JigsawPuzzleSolver/MainProject/piece-info",
            ConnectInfoDirectory = @"/Users/jkwchunjae/Documents/GitHub/JigsawPuzzleSolver/MainProject/connection-info",
            PuzzleTableFilePath = @"/Users/jkwchunjae/Documents/GitHub/JigsawPuzzleSolver/MainProject/puzzle-table.json"
        });
        await service.LoadFilesAsync();
        var result = service.Review();

        foreach (var r in result)
        {
            Console.WriteLine($"{r.PieceName} {r.Row} {r.Column} {r.EdgeIndex} {r.Direction} {r.Valid} {r.DiffLength} {r.DiffRatio}");
        }
    }
}

