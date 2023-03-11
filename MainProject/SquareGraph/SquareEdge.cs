namespace SquareGraphLib;

public class SquareEdge
{
    public SquareNode Node { get; init; }

    private List<SquareEdge> _connection = new();
    public IEnumerable<SquareEdge> Connection => _connection;

    public SquareEdge(SquareNode node)
    {
        Node = node;
    }

    public void Connect(SquareEdge target)
    {
        if (!_connection.Contains(target))
        {
            _connection.Add(target);
            target.Connect(this);
        }
    }

    public void Disconnect(SquareEdge target)
    {
        if (_connection.Contains(target))
        {
            _connection.Remove(target);
            target.Disconnect(this);
        }
    }

    public void DisconnectAll()
    {
        foreach (var target in _connection)
        {
            Disconnect(target);
        }
    }
}

