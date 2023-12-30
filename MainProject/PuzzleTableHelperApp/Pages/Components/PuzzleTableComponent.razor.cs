
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

    private async Task AddTarget(int row, int column)
    {
        await Js.InvokeVoidAsync("console.log", row);
        if (!IsTarget(row, column))
        {
            _targets.Add((row, column));
        }
        StateHasChanged();
    }

    private Task RemoveTarget(int row, int column)
    {
        if (IsTarget(row, column))
        {
            _targets.Remove((row, column));
        }
        StateHasChanged();
        return Task.CompletedTask;
    }

    private bool IsTarget(int row, int column)
    {
        return _targets.Contains((row, column));
    }
}