using System.Data;
using System.Text.Json;
using Common.PieceInfo;
using JkwExtensions;

namespace PuzzleTableHelperCore;

public class PuzzleTableServiceOption
{
    public required string PieceInfoDirectory { get; set; }
    public required string ConnectInfoDirectory { get; set; }
    public required string PuzzleTableFilePath { get; set; }
}

public class PuzzleTableService
{
    private readonly string _pieceInfoDirectory;
    private readonly string _connectInfoDirectory;
    private readonly string _puzzleTableFilePath;

    protected PieceInfo[] _pieceInfos = Array.Empty<PieceInfo>();
    protected ConnectInfo[] _connectInfos = Array.Empty<ConnectInfo>();
    protected PuzzleTable _puzzleTable = null;

    public PuzzleTable PuzzleTable => _puzzleTable;

    public PuzzleTableService(PuzzleTableServiceOption option)
    {
        _pieceInfoDirectory = option.PieceInfoDirectory;
        _connectInfoDirectory = option.ConnectInfoDirectory;
        _puzzleTableFilePath = option.PuzzleTableFilePath;
    }

    /// <summary>
    /// PuzzleTable에서 확정된 정보와 인접한 타겟의 정보를 찾는다.
    /// </summary>
    /// <param name="targets">
    ///    
    /// </param>
    /// <param name="suggestionSet">
    ///    backtracking을 위한 정보
    /// </param>
    /// <returns></returns>
    public IEnumerable<SuggestionSet> FindTarget(int limit, List<(int Row, int Column)> targets, SuggestionSet? suggestionSet = null)
    {
        if (targets.Count == 0)
        {
            yield return suggestionSet;
            yield break;
        }

        var target = targets.FirstOrDefault(t => HasNearCell(t, suggestionSet));
        var nearCells = GetNearCells(target, suggestionSet).ToArray();

        // target의 주변 퍼즐의 target쪽 연결 정보
        var nearConnections = nearCells
            .Select(nearCell =>
            {
                var connectInfo = _connectInfos.First(x => x.PieceName == nearCell.PieceName);
                var edge = connectInfo.Edges[nearCell.GetEdgeIndex(target)];
                var nearConnections = edge.Connection
                    // 확정된 조각은 제외한다.
                    .Where(cell => _puzzleTable.IsFixed(cell.PieceName) == false)
                    .Take(limit);
                return (NearCell: nearCell, Connections: nearConnections);
            })
            .SelectMany(x => x.Connections
                .Select(c => (NearCell: x.NearCell, ConnectTarget: c))
                .ToArray())
            .ToArray();

        var allConnectNames = nearConnections
            .Select(x => x.ConnectTarget.PieceName)
            .Distinct()
            .ToArray();
        var suggestionTargets1 = allConnectNames
            .Select(pieceName => nearConnections.Where(x => x.ConnectTarget.PieceName == pieceName).ToArray())
            .ToArray();
        var suggestionTargets2 = suggestionTargets1
            .Where(connections => connections.Length == nearCells.Length)
            .ToArray();
        var suggestionTargets3 = suggestionTargets2
            .OrderBy(connections => connections.Aggregate(1f, (acc, x) => acc * x.ConnectTarget.Value))
            .ToArray();
        var suggestionTargets = suggestionTargets3
            .Select(x =>
            {
                var (cearCell, connectTarget) = x.First();
                var targetEdge = connectTarget.EdgeIndex;
                var targetTopIndex = cearCell.CalcTargetTopIndex(target, targetEdge);
                return (ConnectTarget: connectTarget, TargetTopIndex: targetTopIndex, NearCells: x.Select(e => e.NearCell).ToArray());
            })
            .ToArray();

        suggestionSet ??= new SuggestionSet
        {
            Cells = new(),
        };
        foreach (var (connectTarget, targetTopIndex, _) in suggestionTargets)
        {
            var nextTarget = targets
                .Where(t => t != target)
                .ToList();
            var nextSuggestionSet = new SuggestionSet
            {
                Cells = suggestionSet.Cells
                    .Append(new PuzzleCell
                    {
                        Row = target.Row,
                        Column = target.Column,
                        PieceName = connectTarget.PieceName,
                        PieceNumber = _pieceInfos.First(x => x.Name == connectTarget.PieceName).Number,
                        TopEdgeIndex = targetTopIndex,
                    })
                    .ToList(),
            };

            foreach (var result in FindTarget(limit, nextTarget, nextSuggestionSet))
            {
                yield return result;
            }
        }
    }

    public async Task<PuzzleTable> SelectTableCell(List<PuzzleCell> selectedCells)
    {
        foreach (var cell in selectedCells)
        {
            _puzzleTable.Append(cell);
        }

        var tableText = JsonSerializer.Serialize(_puzzleTable, new JsonSerializerOptions
        {
            WriteIndented = true,
        });

        await File.WriteAllTextAsync(_puzzleTableFilePath, tableText);

        return _puzzleTable;
    }

    private bool HasNearCell((int Row, int Column) target, SuggestionSet? suggestionSet)
    {
        var direction = new(int Row, int Column)[] { (0, -1), (1, 0), (0, 1), (-1, 0) };
        if (direction.Any(d => _puzzleTable.GetCell(target.Row + d.Row, target.Column + d.Column) != null))
        {
            return true;
        }
        if (direction.Any(d => suggestionSet?.GetCell(target.Row + d.Row, target.Column + d.Column) != null))
        {
            return true;
        }
        return false;
    }

    private IEnumerable<PuzzleCell> GetNearCells((int Row, int Column) target, SuggestionSet? suggestionSet)
    {
        var direction = new(int Row, int Column)[] { (0, -1), (1, 0), (0, 1), (-1, 0) };
        var fromTable = direction
            .Select(d => _puzzleTable.GetCell(target.Row + d.Row, target.Column + d.Column))
            .Where(x => x != null)
            .Select(x => x!)
            .ToArray();

        var fromSuggestion = direction
            .Select(d => suggestionSet?.GetCell(target.Row + d.Row, target.Column + d.Column))
            .Where(x => x != null)
            .Select(x => x!)
            .ToArray();

        return fromTable.Concat(fromSuggestion).ToArray();
    }

    public async Task LoadFilesAsync()
    {
        _pieceInfos = await LoadPieceInfoAsync();
        _connectInfos = await LoadConnectInfoAsync();
        _puzzleTable = await LoadPuzzleTableAsync();
    }

    private async Task<PieceInfo[]> LoadPieceInfoAsync()
    {
        var serializeOption = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = 
            {
                new PointFJsonConverter(),
                new PointArrayJsonConverter(),
                new PointFArrayJsonConverter(),
            },
        };
        return (await Directory.GetFiles(_pieceInfoDirectory, "*.json")
            .Select(path => File.ReadAllTextAsync(path))
            .WhenAll())
            .Select(x => JsonSerializer.Deserialize<PieceInfo>(x, serializeOption)!)
            .ToArray();
    }

    private async Task<ConnectInfo[]> LoadConnectInfoAsync()
    {
        return (await Directory.GetFiles(_connectInfoDirectory, "*.json")
            .Select(path => File.ReadAllTextAsync(path))
            .WhenAll())
            .Select(x => JsonSerializer.Deserialize<ConnectInfo>(x)!)
            .ToArray();
    }

    private async Task<PuzzleTable> LoadPuzzleTableAsync()
    {
        if (!File.Exists(_puzzleTableFilePath))
        {
            return new PuzzleTable
            {
                Cells = new List<List<PuzzleCell?>>(),
            };
        }

        var text = await File.ReadAllTextAsync(_puzzleTableFilePath);
        return JsonSerializer.Deserialize<PuzzleTable>(text)!;
    }

    public float GetValueBetweenCell(PuzzleCell cell1, PuzzleCell cell2)
    {
        var connectInfo = _connectInfos.First(x => x.PieceName == cell1.PieceName);
        var edge = connectInfo.Edges[cell1.GetEdgeIndex((cell2.Row, cell2.Column))];
        return edge.Connection.First(x => x.PieceName == cell2.PieceName).Value;
    }
}