using Common.PieceInfo;
using JkwExtensions;
using SquareGraphLib;

namespace PuzzleTableHelperCore;

public class SquareGraphBuilder
{
    private PieceInfo[] _pieceInfos = Array.Empty<PieceInfo>();
    private ConnectInfo[] _connectInfos = Array.Empty<ConnectInfo>();

    public SquareGraphBuilder SetPieceInfos(PieceInfo[] pieceInfos)
    {
        _pieceInfos = pieceInfos
            .OrderBy(x => x.Name)
            .ToArray();
        return this;
    }

    public SquareGraphBuilder SetConnectInfos(ConnectInfo[] connectInfos)
    {
        _connectInfos = connectInfos
            .OrderBy(x => x.PieceName)
            .ToArray();
        return this;
    }

    public SquareGraph Build()
    {
        var nodeCount = _pieceInfos.Length;
        var names = _pieceInfos.Select(x => x.Name).ToArray();
        var graph = new SquareGraph(nodeCount, names);

        var triples = _pieceInfos
            .Zip(_connectInfos)
            .Select(x => (PieceInfo: x.First, ConnectInfo: x.Second, Node: graph.Nodes.First(y => y.Name == x.First.Name)))
            .ToArray();

        Dictionary<string, SquareNode> pieceNameToNode = triples
            .ToDictionary(x => x.PieceInfo.Name, x => x.Node);

        foreach (var (pieceInfo, connectInfo, node) in triples)
        {
            foreach (var edge in connectInfo.Edges)
            {
                foreach (var target in edge.Connection)
                {
                    var targetNode = pieceNameToNode[target.PieceName];
                    var targetEdge = targetNode.Edges[target.EdgeIndex];
                    node.Edges[edge.Index].Connect(targetEdge);
                }
            }
        }

        return graph;
    }
}