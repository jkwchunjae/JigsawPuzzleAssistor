
using MainApp.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using PuzzleTableHelperCore;

namespace MainApp.Pages.Table;

public partial class PuzzleTableRecommendationComponent : ComponentBase
{
    [Inject] WorkspaceService WorkspaceService { get; set; } = null!;
    [Inject] IJSRuntime js { get; set; } = null!;

    [Parameter] public TableService TableService { get; set; } = null!;
    [Parameter] public PuzzleTableService PuzzleTableService { get; set; } = null!;
    [Parameter] public PuzzleTable Table { get; set; } = null!;
    [Parameter] public (Range RowRange, Range ColumnRange) TargetRange { get; set; } = (new Range(0, 1), new Range(0, 1));
    [Parameter] public EventCallback<RecommendedData> OnSelect { get; set; }
    [Parameter] public EventCallback<(int Row, int Column)> OnHover { get; set; }

    List<RecommendedData> RecommendedDataAll = null;
    RecommendedData[] RecommendedDatas = Array.Empty<RecommendedData>();

    RecommendationService RecommendationService = null!;

    protected override async Task OnInitializedAsync()
    {
        var connectionService = new ConnectionInfoService(WorkspaceService.CurrentWorkspace!, PuzzleTableService);
        var connections = PuzzleTableService.ConnectInfo;
        var pieceInfos = PuzzleTableService.PieceInfo;

        RecommendationService = new RecommendationService(WorkspaceService.CurrentWorkspace!, Table, connections, pieceInfos);
    }

    protected Task Refresh()
    {
        RecommendedDataAll = RecommendationService.Recommend(TargetRange.RowRange, TargetRange.ColumnRange)
            .OrderBy(x => x.Value)
            .ToList();
        RecommendedDatas = RecommendedDataAll.Take(10).ToArray();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task Select(RecommendedData recommendedData)
    {
        await OnSelect.InvokeAsync(recommendedData);
        RecommendedDataAll.Remove(recommendedData);
        RecommendedDatas = RecommendedDataAll.Take(10).ToArray();
    }

    private async Task Exclude(RecommendedData recommendedData)
    {
        var excludeData = new ExcludeData
        (
            Row: recommendedData.TargetRow,
            Column: recommendedData.TargetColumn,
            PieceName: recommendedData.Recommended.PieceName
        );
        await RecommendationService.Exclude(excludeData);
        RecommendedDataAll.Remove(recommendedData);
        RecommendedDatas = RecommendedDataAll.Take(10).ToArray();
    }

    private async Task RowMouseEnter(TableRowHoverEventArgs<RecommendedData> arg)
    {
        await OnHover.InvokeAsync((arg.Item.TargetRow, arg.Item.TargetColumn));
    }

    private Task RowMouseLeave(TableRowHoverEventArgs<RecommendedData> arg)
    {
        return OnHover.InvokeAsync(default);
    }
}