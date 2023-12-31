
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
    List<(int Row, int Column, int Number, HintFlag Flag)> _hint = new();

    int displayColumnStart = 0;
    int displayRowStart = 0;

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

    private void GetSuggestionSets(int limit = 0)
    {
        if (_targets?.Any() ?? false)
        {
            if (limit == 0)
            {
                limit = targetLimit;
            }
            var sets = _service.FindTarget(limit, _targets);
            UpdateSuggestionSets(sets);
        }
    }

    private void UpdateSuggestionSets(IEnumerable<SuggestionSet> sets)
    {
        _suggestionSets = sets
            .Where(set =>
            {
                if (_hint?.Any() ?? false)
                {
                    var relatedHint = _hint.Where(x => set.Cells.Any(cell => cell.Row == x.Row && cell.Column == x.Column));
                    var yesHints = relatedHint.Where(x => x.Flag == HintFlag.Yes);
                    var noHints = relatedHint.Where(x => x.Flag == HintFlag.No);

                    var resultYes = yesHints.All(hint => set.Cells.Any(cell => cell.Row == hint.Row && cell.Column == hint.Column && cell.PieceNumber == hint.Number));
                    var resultNo = noHints.Any(hint => set.Cells.Any(cell => cell.Row == hint.Row && cell.Column == hint.Column && cell.PieceNumber == hint.Number));

                    return resultYes && (noHints.Any() ? !resultNo : true);
                }
                else
                {
                    return true;
                }
            })
            .ToList();
    }

    private async Task SelectSuggestionSet(SuggestionSet set)
    {
        _puzzleTable = await _service.SelectTableCell(set.Cells);
        _targets = new();
        _suggestionSets = new();
        StateHasChanged();
    }

    private void HintYes(int row, int column, int number)
    {
        _hint.Add((row, column, number, HintFlag.Yes));
        UpdateSuggestionSets(_suggestionSets);
    }

    private void HintNo(int row, int column, int number)
    {
        _hint.Add((row, column, number, HintFlag.No));
        UpdateSuggestionSets(_suggestionSets);
    }
}

public enum HintFlag
{
    Yes,
    No,
}