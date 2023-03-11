namespace SquareGraphLib;

/// <summary>
/// Edge0~3 는 시계방향이다.
/// </summary>
public class SquareNode
{
    public int No { get; set; }

    public SquareEdge[] Edges { get; init; }

    public SquareEdge Edge0 => Edges[0];
    public SquareEdge Edge1 => Edges[1];
    public SquareEdge Edge2 => Edges[2];
    public SquareEdge Edge3 => Edges[3];

    public SquareNode(int no)
    {
        No = no;
        Edges = Enumerable.Range(0, 4)
            .Select(_ => new SquareEdge(this))
            .ToArray();
    }
}

