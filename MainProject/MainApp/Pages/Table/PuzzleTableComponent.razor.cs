using MainApp.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PuzzleTableHelperCore;

namespace MainApp.Pages.Table;

public enum HintFlag
{
    Yes,
    No,
}

public partial class PuzzleTableComponent : ComponentBase
{
    [Inject] IJSRuntime Js { get; set; } = null!;
    [Parameter] public PuzzleTableServiceOption Option { get; set; } = null!;
    [Parameter] public TableService TableService { get; set; } = null!;

    PuzzleTableService _service;
    PuzzleTable? _puzzleTable = null;

    List<(int Row, int Column)> _targets = new();
    int targetLimit = 10;
    List<SuggestionSet> _suggestionSets = new();
    SuggestionSet? _testSet = null;
    List<(int Row, int Column, int Number, HintFlag Flag)> _hint = new();


    (Range RowRange, Range ColumnRange) _targetRange = (new Range(0, 1), new Range(0, 1));
    (int Row, int Column) _hoverRecommendation = default;

    protected override async Task OnInitializedAsync()
    {
        _service = new PuzzleTableService(Option);
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
        _testSet = null;
        StateHasChanged();
    }
    private Task TestSuggestionSet(SuggestionSet set)
    {
        _testSet = set;
        StateHasChanged();
        return Task.CompletedTask;
    }
    private Task CancelSuggestionSet(SuggestionSet set)
    {
        _testSet = null;
        StateHasChanged();
        return Task.CompletedTask;
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

    private Task RangeChanged(Range rowRange, Range columnRange)
    {
        _targetRange = (rowRange, columnRange);
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task SelectRecommended(RecommendedData recommendedData)
    {
        var rData = recommendedData;
        var cell = _service.MakePuzzleCell(rData.Fixed.PieceName, rData.Fixed.EdgeIndex, rData.Recommended.PieceName, rData.Recommended.EdgeIndex);
        _puzzleTable = await _service.SelectTableCell(new() { cell });
        _targets = new();
        _suggestionSets = new();
        _testSet = null;
        StateHasChanged();
    }
}
