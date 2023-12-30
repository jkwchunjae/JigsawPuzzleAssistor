
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PuzzleTableHelperCore;

namespace PuzzleTableHelperApp.Pages.Components;
public partial class PuzzleTableComponent : ComponentBase
{
    [Inject] IJSRuntime Js { get; set; } = null!;
    PuzzleTableService _service;
    PuzzleTable? _puzzleTable = null;

    List<(int Row, int Column)> _targets = new();
    int targetLimit = 10;
    List<SuggestionSet> _suggestionSets = new();

    protected override async Task OnInitializedAsync()
    {
        _service = new PuzzleTableService(new PuzzleTableServiceOption
        {
            PieceInfoDirectory = @"/Users/jkwchunjae/Documents/GitHub/JigsawPuzzleSolver/MainProject/piece-info",
            ConnectInfoDirectory = @"/Users/jkwchunjae/Documents/GitHub/JigsawPuzzleSolver/MainProject/connection-info",
            PuzzleTableFilePath = @"/Users/jkwchunjae/Documents/GitHub/JigsawPuzzleSolver/MainProject/puzzle-table.json"
        });
        await _service.LoadFilesAsync();
        _puzzleTable = _service.PuzzleTable;
    }

    private void AddTarget(int row, int column)
    {
        if (!IsTarget(row, column))
        {
            _targets.Add((row, column));
        }
    }

    private void RemoveTarget(int row, int column)
    {
        if (IsTarget(row, column))
        {
            _targets.Remove((row, column));
        }
    }

    private bool IsTarget(int row, int column)
    {
        return _targets.Contains((row, column));
    }

    private void GetSuggestionSets()
    {
        if (_targets?.Any() ?? false)
        {
            var sets = _service.FindTarget(targetLimit, _targets);
            _suggestionSets = sets.ToList();
        }
    }

    private async Task SelectSuggestionSet(SuggestionSet set)
    {
        await Js.InvokeVoidAsync("console.log", set);
        _puzzleTable = await _service.SelectTableCell(set.Cells);
        _targets = new();
        StateHasChanged();
    }
}