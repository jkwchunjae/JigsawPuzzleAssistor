
namespace SquareGraphLib;

/// <summary>
/// Edge0~3 는 시계방향이다.
/// </summary>
public class SquareNode
{
    public int No { get; set; }
    public string Name { get; set; }
    public SquareEdge[] Edges { get; init; }

    public SquareEdge Top => Edges[0];
    public SquareEdge Right => Edges[1];
    public SquareEdge Bottom => Edges[2];
    public SquareEdge Left => Edges[3];

    public bool IsCorner => Edges.Count(edge => edge.IsBorder) == 2;
    public bool IsBorder => Edges.Count(edge => edge.IsBorder) == 1;
    public bool IsInner => Edges.Count(edge => edge.IsBorder) == 0;

    public SquareNode(int no, string name)
    {
        No = no;
        Name = name;
        Edges = Enumerable.Range(0, 4)
            .Select(_ => new SquareEdge(this))
            .ToArray();
    }

    public SquareNode(int no, string name, SquareEdge[] edges)
    {
        No = no;
        Name = name;
        Edges = edges;
    }

    public SquareNode RotateUntil(Func<SquareNode, bool> condition)
    {
        var thisNode = this;
        for (var i = 0; i < 4; i++)
        {
            if (condition(thisNode))
            {
                return thisNode;
            }
            else
            {
                thisNode = thisNode.Rotate();
            }
        }
        throw new DoNotMeetConditionException();
    }
    public SquareNode Rotate() => RotateRight(); // 어느방향으로 돌려도 큰 의미 없을 것 같다.

    public SquareNode RotateRight()
    {
        var newEdges = new [] { Left, Top, Right, Bottom };
        var node = new SquareNode(No, Name, newEdges);

        return node;
    }

    public SquareNode RotateLeft()
    {
        var newEdges = new [] { Right, Bottom, Left, Top };
        var node = new SquareNode(No, Name, newEdges);

        return node;
    }
}

