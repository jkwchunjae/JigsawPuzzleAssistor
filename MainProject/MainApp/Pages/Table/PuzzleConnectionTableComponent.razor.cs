﻿using Emgu.CV.Dnn;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PuzzleTableHelperCore;

namespace MainApp.Pages.Table;

public partial class PuzzleConnectionTableComponent : ComponentBase
{
    [Inject] IJSRuntime Js { get; set; } = null!;
    [Parameter] public PuzzleTableService Service { get; set; } = null!;
    [Parameter] public PuzzleTable? Table { get; set; } = null!;
    [Parameter] public SuggestionSet? SuggestionSet { get; set; } = null;
    [Parameter] public List<(int Row, int Column)> Targets { get; set; } = null;
    [Parameter] public EventCallback<(int Row, int Column)> OnAddTarget { get; set; }
    [Parameter] public EventCallback<(int Row, int Column)> OnRemoveTarget { get; set; }

    bool ShowValue;
    bool ReverseLeftRight;
    List<(int Row, int Column)> Ignores = new();

    private Range RowRange = new Range(0, 1);
    private Range ColumnRange = new Range(0, 1);

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

    private Task ColumnStartChanged(ChangeEventArgs e)
    {
        var columnStart = GetValueFromEvent(e.Value);
        if (columnStart < 0)
            return Task.CompletedTask;
        var range = ColumnRange.End.Value - ColumnRange.Start.Value;
        ColumnRange = new Range(columnStart, columnStart + range);
        return Task.CompletedTask;
    }
    private Task ColumnEndChanged(ChangeEventArgs e)
    {
        var columnEnd = GetValueFromEvent(e.Value);
        if (columnEnd <= ColumnRange.Start.Value)
            return Task.CompletedTask;
        ColumnRange = new Range(ColumnRange.Start, columnEnd);
        return Task.CompletedTask;
    }

    private Task RowStartChanged(ChangeEventArgs e)
    {
        var rowStart = GetValueFromEvent(e.Value);
        if (rowStart < 0)
            return Task.CompletedTask;
        var range = RowRange.End.Value - RowRange.Start.Value;
        RowRange = new Range(rowStart, rowStart + range);
        return Task.CompletedTask;
    }
    private Task RowEndChanged(ChangeEventArgs e)
    {
        var rowEnd = GetValueFromEvent(e.Value);
        if (rowEnd <= RowRange.Start.Value)
            return Task.CompletedTask;
        RowRange = new Range(RowRange.Start, rowEnd);
        return Task.CompletedTask;
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
}
