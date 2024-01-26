using MainApp.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PuzzleTableHelperCore;

namespace MainApp.Pages.Table;

public partial class PuzzleConnectionTableComponent : ComponentBase
{
    [Inject] IJSRuntime Js { get; set; } = null!;
    [Parameter] public PuzzleTableService Service { get; set; } = null!;
    [Parameter] public TableService TableService { get; set; } = null!;
    [Parameter] public PuzzleTable? Table { get; set; } = null!;
    [Parameter] public SuggestionSet? SuggestionSet { get; set; } = null;
    [Parameter] public List<(int Row, int Column)> Targets { get; set; } = null;
    [Parameter] public (int Row, int Column) HoverRecommendation { get; set; }
    [Parameter] public EventCallback<(int Row, int Column)> OnAddTarget { get; set; }
    [Parameter] public EventCallback<(int Row, int Column)> OnRemoveTarget { get; set; }
    [Parameter] public EventCallback<(Range RowRange, Range ColumnRange)> OnRangeChanged { get; set; }

    bool ShowValue;
    bool ReverseLeftRight;
    List<(int Row, int Column)> Ignores = new();

    private Range RowRange = new Range(0, 1);
    private Range ColumnRange = new Range(0, 1);

    PuzzleTableInitOption TableInitOption => new PuzzleTableInitOption
    {
        ReverseLeftRight = ReverseLeftRight,
        RowBegin = RowRange.Start.Value,
        RowEnd = RowRange.End.Value,
        ColumnBegin = ColumnRange.Start.Value,
        ColumnEnd = ColumnRange.End.Value,
    };

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var tableOption = await TableService.LoadTableInitOption(Path.GetFileName(Service.TableFilePath));
        if (tableOption != null)
        {
            ReverseLeftRight = tableOption.ReverseLeftRight;
            RowRange = new Range(tableOption.RowBegin, tableOption.RowEnd);
            ColumnRange = new Range(tableOption.ColumnBegin, tableOption.ColumnEnd);
            await OnRangeChanged.InvokeAsync((RowRange, ColumnRange));
        }
    }

    private IEnumerable<int> Rows()
    {
        return Enumerable.Range(RowRange.Start.Value, RowRange.End.Value - RowRange.Start.Value + 1);
    }

    private IEnumerable<int> Columns()
    {
        var range = Enumerable.Range(ColumnRange.Start.Value, ColumnRange.End.Value - ColumnRange.Start.Value + 1);
        if (ReverseLeftRight)
        {
            return range.Reverse();
        }
        else
        {
            return range;
        }
    }

    private async Task ColumnStartChanged(ChangeEventArgs e)
    {
        var columnStart = GetValueFromEvent(e.Value);
        if (columnStart < 0)
            return;
        var range = ColumnRange.End.Value - ColumnRange.Start.Value;
        ColumnRange = new Range(columnStart, columnStart + range);
        await OnRangeChanged.InvokeAsync((RowRange, ColumnRange));
        await SaveTableInitOption();
        return;
    }
    private async Task ColumnEndChanged(ChangeEventArgs e)
    {
        var columnEnd = GetValueFromEvent(e.Value);
        if (columnEnd <= ColumnRange.Start.Value)
            return;
        ColumnRange = new Range(ColumnRange.Start, columnEnd);
        await OnRangeChanged.InvokeAsync((RowRange, ColumnRange));
        await SaveTableInitOption();
        return;
    }

    private async Task RowStartChanged(ChangeEventArgs e)
    {
        var rowStart = GetValueFromEvent(e.Value);
        if (rowStart < 0)
            return;
        var range = RowRange.End.Value - RowRange.Start.Value;
        RowRange = new Range(rowStart, rowStart + range);
        await OnRangeChanged.InvokeAsync((RowRange, ColumnRange));
        await SaveTableInitOption();
        return;
    }
    private async Task RowEndChanged(ChangeEventArgs e)
    {
        var rowEnd = GetValueFromEvent(e.Value);
        if (rowEnd <= RowRange.Start.Value)
            return;
        RowRange = new Range(RowRange.Start, rowEnd);
        await OnRangeChanged.InvokeAsync((RowRange, ColumnRange));
        await SaveTableInitOption();
        return;
    }

    private int GetValueFromEvent(object? eValue)
    {
        if (eValue is string strValue && int.TryParse(strValue, out var columnStart))
        {
            return columnStart;
        }
        else if (eValue is int intValue)
        {
            return intValue;
        }
        else if (eValue is float floatValue)
        {
            return (int)floatValue;
        }
        else if (eValue is double doubleValue)
        {
            return (int)doubleValue;
        }
        return 0;
    }

    private int GetCellNumber(int row, int column)
    {
        var suggCell = SuggestionSet?.GetCell(row, column);
        if (suggCell != null)
            return suggCell.PieceNumber;

        if (Table == null)
            return 0;
        if (row < 0 || row >= Table.RowCount)
            return 0;

        var cells = Table.Cells[row];

        if (column < 0 || column >= cells.Count)
            return 0;
        var cell = cells[column];
        return cell?.PieceNumber ?? 0;
    }

    private PuzzleCell? GetCellFromTable(int row, int column)
    {
        if (Table == null)
            return null;
        if (row < 0 || row >= Table.RowCount)
            return null;

        var cells = Table.Cells[row];

        if (column < 0 || column >= cells.Count)
            return null;
        var cell = cells[column];
        return cell;
    }

    private float GetValueBetweenCell(int row1, int column1, int row2, int column2)
    {
        var cell1 = GetCellFromTable(row1, column1) ?? SuggestionSet?.GetCell(row1, column1);
        var cell2 = GetCellFromTable(row2, column2) ?? SuggestionSet?.GetCell(row2, column2);

        return GetValueBetweenCell(cell1, cell2);
    }

    private float GetValueBetweenCell(PuzzleCell? cell1, PuzzleCell? cell2)
    {
        if (cell1 == null || cell2 == null)
            return 0;

        return Service.GetValueBetweenCell(cell1, cell2);
    }

    private bool IsTarget(int row, int column)
    {
        return Targets?.Contains((row, column)) ?? false;
    }
    private async Task AddTarget(int row, int column)
    {
        if (!IsTarget(row, column))
        {
            await OnAddTarget.InvokeAsync((row, column));
        }
    }

    private async Task RemoveTarget(int row, int column)
    {
        if (IsTarget(row, column))
        {
            await OnRemoveTarget.InvokeAsync((row, column));
        }
    }

    private bool Ignored(int row, int column)
    {
        return Ignores.Contains((row, column));
    }
    private Task Ignore(int row, int column)
    {
        if (!Ignored(row, column))
        {
            Ignores.Add((row, column));
            Service.SetIgnores(Ignores);
        }
        return Task.CompletedTask;
    }
    private Task Unignore(int row, int column)
    {
        if (Ignored(row, column))
        {
            Ignores.Remove((row, column));
            Service.SetIgnores(Ignores);
        }
        return Task.CompletedTask;
    }

    private Task SaveTableInitOption()
    {
        var fileName = Path.GetFileName(Service.TableFilePath);
        return TableService.SaveTableInitOption(fileName, TableInitOption);
    }

    private bool IsHoverRecommendation(int row, int column)
    {
        return HoverRecommendation == (row, column);
    }
}
