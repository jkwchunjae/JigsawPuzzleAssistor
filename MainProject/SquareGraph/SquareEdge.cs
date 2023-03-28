namespace SquareGraphLib;

public class SquareEdge
{
    public SquareNode Node { get; init; }
    public bool IsBorder => _connections.Empty();

    private List<SquareEdge> _connections = new();
    public IEnumerable<SquareEdge> Connections => _connections;
    public IEnumerable<SquareNode> ConnectNodes => _connections.Select(c => c.Node);

    /// <summary> 자신의 오른쪽 엣지를 반환 </summary>
    public SquareEdge Right
    {
        get
        {
            for (var i = 0; i < 4; i++)
            {
                if (Node.Edges[i] == this)
                    return Node.Edges[(i + 1) % 4];
            }
            throw new Exception();
        }
    }

    /// <summary> 자신의 왼쪽 엣지를 반환 </summary>
    public SquareEdge Left
    {
        get
        {
            for (var i = 0; i < 4; i++)
            {
                if (Node.Edges[i] == this)
                    return Node.Edges[(i + 3) % 4];
            }
            throw new Exception();
        }
    }

    public SquareEdge(SquareNode node)
    {
        Node = node;
    }

    public void Connect(SquareEdge target)
    {
        if (!_connections.Contains(target))
        {
            _connections.Add(target);
            target.Connect(this);
        }
    }

    public void Disconnect(SquareEdge target)
    {
        if (_connections.Contains(target))
        {
            _connections.Remove(target);
            target.Disconnect(this);
        }
    }

    public void DisconnectAll()
    {
        foreach (var target in _connections)
        {
            Disconnect(target);
        }
    }
}
