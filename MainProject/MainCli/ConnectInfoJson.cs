using System.Text.Json;
using Common.PieceInfo;
using JkwExtensions;

namespace MainCli;

internal class ConnectInfoJson : IMainRunner
{
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

    public async Task Run()
    {
        var pieceInfoDirectory = @"../piece-info";
        var connectionInfoDirectory = @"../connection-info";

        if (!Directory.Exists(connectionInfoDirectory))
        {
            Directory.CreateDirectory(connectionInfoDirectory);
        }

        var pieceInfos = (await Directory.GetFiles(pieceInfoDirectory)
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

        foreach (var (myIndex, myInfo, myConnectInfo) in arr)
        {
            Console.WriteLine(myInfo.Name);
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
            var connectInfoPath = Path.Join(connectionInfoDirectory, $"{connectInfo.PieceName}.json");
            var jsonText = JsonSerializer.Serialize(connectInfo, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(connectInfoPath, jsonText);
        }
    }

    public async Task<PieceInfo> ReadFromFile(string filepath)
    {
        var text = await File.ReadAllTextAsync(filepath);
        var pieceInfo = JsonSerializer.Deserialize<PieceInfo>(text, serializeOption);

        return pieceInfo;
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
}