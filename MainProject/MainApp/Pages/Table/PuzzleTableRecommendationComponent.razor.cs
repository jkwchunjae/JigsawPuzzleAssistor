
using MainApp.Service;
using Microsoft.AspNetCore.Components;
using PuzzleTableHelperCore;

namespace MainApp.Pages.Table;

public partial class PuzzleTableRecommendationComponent : ComponentBase
{
    [Inject] WorkspaceService WorkspaceService { get; set; } = null!;

    [Parameter] public TableService TableService { get; set; } = null!;
    [Parameter] public PuzzleTable Table { get; set; } = null!;
    [Parameter] public (Range RowRange, Range ColumnRange) TargetRange { get; set; } = (new Range(0, 1), new Range(0, 1));

    RecommendedData[] RecommendedDatas = Array.Empty<RecommendedData>();

    RecommendationService RecommendationService = null!;

    protected override async Task OnInitializedAsync()
    {
        var connectionService = new ConnectionInfoService(WorkspaceService.CurrentWorkspace!);
        var connections = await connectionService.GetAllConnectionsAsync();

        RecommendationService = new RecommendationService(WorkspaceService.CurrentWorkspace!, Table, connections);
    }

    protected override Task OnParametersSetAsync()
    {
        RecommendedDatas = RecommendationService.Recommend(TargetRange.RowRange, TargetRange.ColumnRange)
            .OrderBy(x => x.Value)
            .Take(100)
            .ToArray();
        StateHasChanged();
        return base.OnParametersSetAsync();
    }

}