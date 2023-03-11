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
            node.Edge0.DisconnectAll();
            node.Edge1.DisconnectAll();
            node.Edge2.DisconnectAll();
            node.Edge3.DisconnectAll();

            Nodes.Remove(node);
        }
    }

    public int[,] Solve()
    {
        throw new NotImplementedException();
    }
}


