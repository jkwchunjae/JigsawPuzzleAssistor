
using System.Text.Json;
using Common.PieceInfo;
using JkwExtensions;
using MudBlazor;
using PuzzleTableHelperCore;

namespace MainApp.Service;

public class RecommendedData
{
    public int TargetRow { get; set; }
    public int TargetColumn { get; set; }
    public Edge Fixed { get; set; }
    public Edge Recommended { get; set; }
    public float Value { get; set; }

    public int FixedNumber => Fixed.PieceName.Right(5).ToInt();
    public int RecommendedNumber => Recommended.PieceName.Right(5).ToInt();

    public RecommendedData(PuzzleCell cell, Edge key, Edge target, float value)
    {
        Fixed = key;
        Recommended = target;
        Value = value;

        (TargetRow, TargetColumn) = GetTargetRowColumn(cell, key.EdgeIndex);
    }

    private (int Row, int Column) GetTargetRowColumn(PuzzleCell cell, int edgeIndex)
    {
        if (cell.TopEdgeIndex == edgeIndex)
            return (cell.Row - 1, cell.Column);
        if (cell.BottomEdgeIndex == edgeIndex)
            return (cell.Row + 1, cell.Column);
        if (cell.LeftEdgeIndex == edgeIndex)
            return (cell.Row, cell.Column - 1);
        if (cell.RightEdgeIndex == edgeIndex)
            return (cell.Row, cell.Column + 1);
        throw new Exception("Invalid edge index");
    }
}

public record Edge(string PieceName, int EdgeIndex);

public class EdgeWithValue
{
    public required Edge Edge1 { get; set; }
    public required Edge Edge2 { get; set; }
    public required float Value { get; set; }

    public bool Contains(string pieceName, int edgeIndex)
    {
        var edge = new Edge(pieceName, edgeIndex);
        return Edge1 == edge || Edge2 == edge;
    }
}

public record ExcludeData(int Row, int Column, string PieceName);

public class RecommendationService
{
    private readonly WorkspaceData _workspace;
    private readonly PuzzleTable _table;
    private readonly ConnectInfo[] _connections;

    private Dictionary<Edge, List<(Edge Key, Edge Target, float Value)>> edgeConnections = new();

    private List<ExcludeData> excluded = new();
    public RecommendationService(WorkspaceData workspace, PuzzleTable table, ConnectInfo[] connections)
    {
        _workspace = workspace;
        _table = table;
        _connections = connections;
        var conn = connections
            .SelectMany(x => x.Edges.SelectMany(y => y.Connection
                .Where(z => z.Value < 2)
                .Select(z => new EdgeWithValue
                {
                    Edge1 = new Edge(x.PieceName, y.Index),
                    Edge2 = new Edge(z.PieceName, z.EdgeIndex),
                    Value = z.Value,
                })))
            .ToArray();

        var conn1 = conn.Select(x => (Key: x.Edge1, Target: x.Edge2, Value: x.Value));
        var conn2 = conn.Select(x => (Key: x.Edge2, Target: x.Edge1, Value: x.Value));
        edgeConnections = conn1.Concat(conn2)
            .GroupBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.OrderBy(e => e.Value).ToList());

        LoadExcludedData();
    }

    private void LoadExcludedData()
    {
        var excludedPath = _workspace.RecommendationExcludedPath();
        if (File.Exists(excludedPath))
        {
            var text = File.ReadAllText(excludedPath);
            excluded = JsonSerializer.Deserialize<List<ExcludeData>>(text)!;
        }
    }

    public async Task Exclude(ExcludeData edgeInfo)
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

        (PuzzleCell Cell, int Edge)[] fixedEdges = _table.Cells
            .Where((_, row) => row >= rowRange.Start.Value && row <= rowRange.End.Value)
            .SelectMany(row => row.Where((_, column) => column >= columnRange.Start.Value && column <= columnRange.End.Value))
            .SelectMany(cell => GetTargetEdges(cell))
            .ToArray();

        var tablePieces = _table.Cells
            .SelectMany(row => row.Select(cell => cell?.PieceName))
            .Where(name => name != null)
            .ToHashSet();

        var result = fixedEdges
            .Where(x => edgeConnections.ContainsKey(new Edge(x.Cell.PieceName, x.Edge)))
            .Select(fixedEdge =>
            {
                var connectionEdges = edgeConnections[new Edge(fixedEdge.Cell.PieceName, fixedEdge.Edge)]
                    .Where(edgeInfo => !tablePieces.Contains(edgeInfo.Target.PieceName))
                    .Select(edgeInfo => new RecommendedData(fixedEdge.Cell, edgeInfo.Key, edgeInfo.Target, edgeInfo.Value));

                return connectionEdges;
            })
            .SelectMany(x => x)
            .GroupBy(x => (x.TargetRow, x.TargetColumn, x.Fixed, x.Recommended))
            .Select(x => x.First())
            .Where(x =>
            {
                var edata = new ExcludeData(x.TargetRow, x.TargetColumn, x.Recommended.PieceName);
                return !excluded.Contains(edata);
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