using Common.PieceInfo;
using JkwExtensions;
using System.Text.Json;

namespace MainApp.Service;

public class ConnectionInfoService
{
    private WorkspaceData workspace;
    public event EventHandler<ProgressEventArgs>? ProgressChanged;

    private ConnectInfo[]? connections;

    JsonSerializerOptions serializeOption = new JsonSerializerOptions
    {
        WriteIndented = true,
        Converters =
        {
            new PointFJsonConverter(),
            new PointArrayJsonConverter(),
            new PointFArrayJsonConverter(),
        },
    };

    public ConnectionInfoService(WorkspaceData workspace)
    {
        this.workspace = workspace;
    }

    public async Task Start(bool openFolder = false)
    {
        if (!Directory.Exists(workspace.ConnectionDir))
        {
            Directory.CreateDirectory(workspace.ConnectionDir);
        }

        var pieceInfos = (await Directory.GetFiles(workspace.InfoDir)
            .OrderBy(x => x)
            //.Take(100)
            .Select(x => ReadFromFile(x))
            .WhenAll())
            .ToArray();

        var connections = pieceInfos
            .Select(info => new ConnectInfo
            {
                PieceName = info.Name,
                Edges = info.Edges
                    .Select((x, i) => new ConnectEdge { Index = i, Connection = new() })
                    .ToArray(),
            })
            .ToList();

        var arr = pieceInfos.Zip(connections)
            .Select((x, i) =>
            (
                Index: i,
                PieceInfo: x.First,
                ConnectInfo: x.Second
            ))
            .ToArray();

        var total = arr.Length;
        var processed = 0;

        foreach (var (myIndex, myInfo, myConnectInfo) in arr)
        {
            var connection = arr
                .Where(x => x.Index > myIndex)
                .AsParallel()
                .Select(other =>
                {
                    var (otherIndex, otherInfo, otherConnectInfo) = other;

                    // Test is heavy operation
                    return Test(myInfo, otherInfo)
                        .Select(x =>
                        {
                            var (myEdge, otherEdge, value) = x;
                            return (
                                MyEdge: myEdge,
                                OtherInfo: otherInfo,
                                OtherConnectInfo: otherConnectInfo,
                                OtherEdge: otherEdge,
                                Value: value
                            );
                        })
                        .ToArray();
                })
                .SelectMany(x => x)
                .ToList();

            foreach (var (myEdge, otherInfo, otherConnectInfo, otherEdge, value) in connection)
            {
                myConnectInfo.Edges[myEdge].Connection.Add(new ConnectTarget
                {
                    PieceName = otherInfo.Name,
                    EdgeIndex = otherEdge,
                    Value = value,
                });
                otherConnectInfo.Edges[otherEdge].Connection.Add(new ConnectTarget
                {
                    PieceName = myInfo.Name,
                    EdgeIndex = myEdge,
                    Value = value,
                });
            }

            Interlocked.Increment(ref processed);

            ProgressChanged?.Invoke(this, new ProgressEventArgs
            {
                Total = total,
                Processed = processed,
            });
        }

        foreach (var connectInfo in connections)
        {
            connectInfo.Edges = connectInfo.Edges
                .Select(x => new ConnectEdge
                {
                    Index = x.Index,
                    Connection = x.Connection
                        .OrderBy(x => x.Value)
                        .GroupBy(x => x.PieceName)
                        .Select(x => x.First())
                        .ToList(),
                })
                .ToArray();
        }

        foreach (var connectInfo in connections)
        {
            var connectInfoPath = Path.Join(workspace.ConnectionDir, $"{connectInfo.PieceName}.json");
            var jsonText = JsonSerializer.Serialize(connectInfo, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(connectInfoPath, jsonText);
        }
    }
    public async Task<PieceInfo> ReadFromFile(string filepath)
    {

        var text = await File.ReadAllTextAsync(filepath);
        var pieceInfo = JsonSerializer.Deserialize<PieceInfo>(text, serializeOption);

        return pieceInfo!;
    }
    private async Task<ConnectInfo[]> LoadConnectInfoAsync()
    {
        return (await Directory.GetFiles(workspace.ConnectionDir, "*.json")
            .Select(path => File.ReadAllTextAsync(path))
            .WhenAll())
            .Select(x => JsonSerializer.Deserialize<ConnectInfo>(x)!)
            .ToArray();
    }

    private IEnumerable<(int MyEdge, int OtherEdge, float Value)> Test(PieceInfo me, PieceInfo other)
    {
        for (var i = 0; i < me.Edges.Count; i++)
        {
            for (var j = 0; j < other.Edges.Count; j++)
            {
                var (result, value) = me.Edges[i].Test(other.Edges[j]);
                if (result)
                {
                    yield return (i, j, value);
                }
            }
        }
    }

    public async Task UpdateAsync(string pieceName2)
    {
        var pieceInfos = (await Directory.GetFiles(workspace.InfoDir)
            .OrderBy(x => x)
            //.Take(100)
            .Select(x => ReadFromFile(x))
            .WhenAll())
            .ToArray();

        var pieceName = pieceInfos.First(x => x.Name.Contains(pieceName2)).Name;

        var connections = await LoadConnectInfoAsync();

        var arr = pieceInfos.Zip(connections)
            .Select((x, i) =>
            (
                Index: i,
                PieceInfo: x.First,
                ConnectInfo: x.Second
            ))
            .ToArray();


        // 기존 정보에서 자신의 정보를 전부 삭제
        foreach (var connectInfo in connections)
        {
            connectInfo.Edges = connectInfo.Edges
                .Select(edge =>
                {
                    var newConnection = edge.Connection
                        .Where(c => c.PieceName != pieceName)
                        .ToList();
                    return new ConnectEdge
                    {
                        Index = edge.Index,
                        Connection = newConnection,
                    };
                })
                .ToArray();
        }

        var myInfo = arr.First(x => x.PieceInfo.Name == pieceName).PieceInfo;
        var myIndex = arr.First(x => x.PieceInfo.Name == pieceName).Index;
        var myConnectInfo = arr.First(x => x.PieceInfo.Name == pieceName).ConnectInfo;

        var connection = arr
            .AsParallel()
            .Select(other =>
            {
                var (otherIndex, otherInfo, otherConnectInfo) = other;

                // Test is heavy operation
                return Test(myInfo, otherInfo)
                    .Select(x =>
                    {
                        var (myEdge, otherEdge, value) = x;
                        return (
                            MyEdge: myEdge,
                            OtherInfo: otherInfo,
                            OtherConnectInfo: otherConnectInfo,
                            OtherEdge: otherEdge,
                            Value: value
                        );
                    })
                    .ToArray();
            })
            .SelectMany(x => x)
            .ToList();

        foreach (var (myEdge, otherInfo, otherConnectInfo, otherEdge, value) in connection)
        {
            myConnectInfo.Edges[myEdge].Connection.Add(new ConnectTarget
            {
                PieceName = otherInfo.Name,
                EdgeIndex = otherEdge,
                Value = value,
            });
            otherConnectInfo.Edges[otherEdge].Connection.Add(new ConnectTarget
            {
                PieceName = myInfo.Name,
                EdgeIndex = myEdge,
                Value = value,
            });
        }

        foreach (var connectInfo in connections)
        {
            connectInfo.Edges = connectInfo.Edges
                .Select(x => new ConnectEdge
                {
                    Index = x.Index,
                    Connection = x.Connection
                        .OrderBy(x => x.Value)
                        .GroupBy(x => x.PieceName)
                        .Select(x => x.First())
                        .ToList(),
                })
                .ToArray();
        }

        foreach (var connectInfo in connections)
        {
            var connectInfoPath = Path.Join(workspace.ConnectionDir, $"{connectInfo.PieceName}.json");
            var jsonText = JsonSerializer.Serialize(connectInfo, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(connectInfoPath, jsonText);
        }
    }

    public async Task<ConnectInfo[]> GetAllConnectionsAsync()
    {
        if (connections == null)
        {
            connections = await LoadConnectInfoAsync();
        }

        return connections;
    }
}
