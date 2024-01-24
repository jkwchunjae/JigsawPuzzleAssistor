
using System.Text.Json;
using Common.PieceInfo;
using MudBlazor;
using PuzzleTableHelperCore;

namespace MainApp.Service;

internal class RecommendedData
{
    public int Row { get; set; }
    public int Column { get; set; }
    public string FixedPieceName { get; set; }
    public int FixedEdgeIndex { get; set; }
    public string RecommendedPieceName { get; set; }
    public int RecommendedEdgeIndex { get; set; }
    public float Value { get; set; }
    public EdgeInfo EdgeInfo { get; set; }

    public RecommendedData(EdgeInfo edgeInfo, (PuzzleCell Cell, int Edge) targetEdge)
    {
        EdgeInfo = edgeInfo;
        FixedPieceName = targetEdge.Cell.PieceName;
        FixedEdgeIndex = targetEdge.Edge;
        RecommendedPieceName = edgeInfo.PieceName1 == FixedPieceName ? edgeInfo.PieceName2 : edgeInfo.PieceName1;
        RecommendedEdgeIndex = edgeInfo.PieceName1 == FixedPieceName ? edgeInfo.EdgeIndex2 : edgeInfo.EdgeIndex1;
        Value = edgeInfo.Value;
        Row = targetEdge.Cell.Row;
        Column = targetEdge.Cell.Column;
    }
}

internal class EdgeInfo
{
    public required string PieceName1 { get; set; }
    public required int EdgeIndex1 { get; set; }
    public required string PieceName2 { get; set; }
    public required int EdgeIndex2 { get; set; }
    public required float Value { get; set; }

    public bool Contains(string pieceName, int edgeIndex)
    {
        return (PieceName1 == pieceName && EdgeIndex1 == edgeIndex) ||
            (PieceName2 == pieceName && EdgeIndex2 == edgeIndex);
    }
}

internal class RecommendationService
{
    private readonly WorkspaceData _workspace;
    private readonly PuzzleTable _table;
    private readonly ConnectInfo[] _connections;

    private readonly EdgeInfo[] edgeConnections = Array.Empty<EdgeInfo>();

    private List<EdgeInfo> excluded = new();
    public RecommendationService(WorkspaceData workspace, PuzzleTable table, ConnectInfo[] connections)
    {
        _workspace = workspace;
        _table = table;
        _connections = connections;
        edgeConnections = connections
            .SelectMany(x => x.Edges.SelectMany(y => y.Connection.Select(z => new EdgeInfo
            {
                PieceName1 = x.PieceName,
                EdgeIndex1 = y.Index,
                PieceName2 = z.PieceName,
                EdgeIndex2 = z.EdgeIndex,
                Value = z.Value,
            })))
            .ToArray();

        LoadExcludedData();
    }

    private void LoadExcludedData()
    {
        var excludedPath = _workspace.RecommendationExcludedPath();
        if (File.Exists(excludedPath))
        {
            var text = File.ReadAllText(excludedPath);
            excluded = JsonSerializer.Deserialize<List<EdgeInfo>>(text)!;
        }
    }

    public async Task Exclude(EdgeInfo edgeInfo)
    {
        excluded.Add(edgeInfo);
        var text = JsonSerializer.Serialize(excluded, new JsonSerializerOptions { WriteIndented = true });
        var excludedPath = _workspace.RecommendationExcludedPath();
        await File.WriteAllTextAsync(excludedPath, text);
    }

    public RecommendedData[] Recommend(Range rowRange, Range columnRange)
    {
        // 1. 영역을 돌면서 주위에 빈 칸이 있는 셀을 찾는다.
        // 2. 빈 칸 방향 엣지 정보를 모은다.
        // 3. 빈 칸 방향 엣지 정보를 기반으로 추천을 한다.

        (PuzzleCell Cell, int Edge)[] targetEdges = _table.Cells
            .Where((_, row) => row >= rowRange.Start.Value && row <= rowRange.End.Value)
            .SelectMany(row => row.Where((_, column) => column >= columnRange.Start.Value && column <= columnRange.End.Value))
            .SelectMany(cell => GetTargetEdges(cell))
            .ToArray();

        RecommendedData[] result = edgeConnections
            // 이미 맞춰진 엣지는 제외한다.
            .Where(e => !(_table.Contains(e.PieceName1) && _table.Contains(e.PieceName2)))
            // 한 쪽이 맞춰진 엣지를 찾는다.
            .Where(edgeInfo => targetEdges.Any(t => edgeInfo.Contains(t.Cell.PieceName, t.Edge)))
            .Select(edgeInfo =>
            {
                var targetEdge = targetEdges.First(t => edgeInfo.Contains(t.Cell.PieceName, t.Edge));
                return new RecommendedData(edgeInfo, targetEdge);
            })
            .ToArray();

        return result;

        IEnumerable<(PuzzleCell Cell, int Edge)> GetTargetEdges(PuzzleCell? cell)
        {
            if (cell == null)
                yield break;

            var row = cell.Row;
            var column = cell.Column;

            if (_table.GetCell(row - 1, column) == null)
            {
                yield return (cell, cell.TopEdgeIndex);
            }
            if (_table.GetCell(row + 1, column) == null)
            {
                yield return (cell, cell.BottomEdgeIndex);
            }
            if (_table.GetCell(row, column - 1) == null)
            {
                yield return (cell, cell.LeftEdgeIndex);
            }
            if (_table.GetCell(row, column + 1) == null)
            {
                yield return (cell, cell.RightEdgeIndex);
            }
        }
    }
}