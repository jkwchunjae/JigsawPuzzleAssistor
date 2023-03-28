namespace SquareGraphLib;

public class SquareGraph
{
    public List<SquareNode> Nodes { get; init; }

    public SquareGraph(int nodeCount)
    {
        Nodes = Enumerable.Range(1, nodeCount)
            .Select(no => new SquareNode(no))
            .ToList();
    }

    public SquareNode? GetNode(int nodeNo)
    {
        return Nodes.FirstOrDefault(node => node.No == nodeNo);
    }

    public void Remove(int nodeNo)
    {
        var node = GetNode(nodeNo);

        if (node != null)
        {
            node.Top.DisconnectAll();
            node.Right.DisconnectAll();
            node.Bottom.DisconnectAll();
            node.Left.DisconnectAll();

            Nodes.Remove(node);
        }
    }

    public int[,] Solve()
    {
        var solver = new Solver1();
        return solver.Solve(Nodes);
    }
}

public interface ISolver
{
    int[,] Solve(List<SquareNode> nodes);
}

// 
public class Solver1 : ISolver
{
    private List<SquareNode>? _nodes;

    public int[,] Solve(List<SquareNode> nodes)
    {
        _nodes = nodes;

        var seedNode = FindFirst4Node();

        return seedNode;
    }

    public int[,] FindFirst4Node()
    {
        var corners = _nodes!.Where(node => node.IsCorner).ToArray();

        foreach (var corner in corners)
        {
            if (TryFind4Node(corner, out var node4))
            {
                return node4;
            }
        }
        throw new NotFoundFirstNodeException();
    }

    private bool TryFind4Node(SquareNode corner, out int[,] node4)
    {
        corner = AlignCorner();

        var aa = FindRightBottomNodeViaRight()
            .SelectMany(x => x.LastNodes.Select(e => (x.RightNode, LastNode: e)))
            .ToArray();
        var bb = FindRightBottomNodeViaBottom()
            .SelectMany(x => x.LastNodes.Select(e => (x.BottomNode, LastNode: e)))
            .ToArray();

        var joined = aa.Join(bb,
                a => a.LastNode,
                b => b.LastNode,
                (a, b) => (a.RightNode, b.BottomNode, a.LastNode))
            .ToArray();

        if (joined.Count() == 0)
        {
            node4 = new int[2, 2];
            return false;
        }
        else if (joined.Count() == 1)
        {
            var result = joined.First();
            node4 = MakeResult(result);
            return true;
        }
        else // if (joined.Count() >= 2)
        {
            var results = joined
                .Select(x => MakeResult(x))
                .ToArray();
            throw new FoundManyFirst4NodesException(results);
        }

        SquareNode AlignCorner()
        {
            var newcorner = corner;
            for (var i = 0; i < 4; i++)
            {
                if (newcorner.Top.IsBorder && newcorner.Left.IsBorder)
                {
                    return newcorner;
                }
                newcorner = newcorner.Rotate();
            }
            throw new CornerIsNotCornerException();
        }

        IEnumerable<(SquareNode RightNode, SquareNode[] LastNodes)> FindRightBottomNodeViaRight()
        {
            foreach (var node1 in corner.Right.ConnectNodes)
            {
                var rightNode = node1.RotateUntil(self => self.Left.ConnectNodes.Any(node => node.No == corner.No));
                yield return (rightNode, rightNode.Bottom.Connections.Select(e => e.Node).ToArray());
            }
        }
        IEnumerable<(SquareNode BottomNode, SquareNode[] LastNodes)> FindRightBottomNodeViaBottom()
        {
            foreach (var node1 in corner.Bottom.ConnectNodes)
            {
                var bottomNode = node1.RotateUntil(self => self.Top.ConnectNodes.Any(node => node.No == corner.No));
                yield return (bottomNode, bottomNode.Right.Connections.Select(e => e.Node).ToArray());
            }
        }
        int[,] MakeResult((SquareNode Right, SquareNode Bottom, SquareNode Last) resultItem)
        {
            return new int[2, 2]
            {
                { corner.No, resultItem.Right.No },
                { resultItem.Bottom.No, resultItem.Last.No },
            };
        }
    }
}
