
using PuzzleTableHelperCore;

namespace MainCli;

internal class FindTargetTest : IMainRunner
{
    public Task Run()
    {
        var service = new PuzzleTableService(new PuzzleTableServiceOption
        {
            PieceInfoDirectory = @"/Users/jkwchunjae/Documents/GitHub/JigsawPuzzleSolver/MainProject/piece-info",
            ConnectInfoDirectory = @"/Users/jkwchunjae/Documents/GitHub/JigsawPuzzleSolver/MainProject/connection-info",
            PuzzleTableFilePath = @"/Users/jkwchunjae/Documents/GitHub/JigsawPuzzleSolver/MainProject/puzzle-table.json"
        });

        var limit = 7;
        var targets = new List<(int, int)>
        {
            (4, 0),
            (4, 1),
        };
        var suggestionSets = service.FindTarget(limit, targets).ToArray();

        Console.WriteLine(suggestionSets.Length);

        return Task.CompletedTask;
    }
}