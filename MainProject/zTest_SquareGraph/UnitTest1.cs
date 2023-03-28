namespace zTest_SquareGraph;

public class UnitTest1
{
    [Fact]
    public void 그래프의_노드가_정상적으로_연결된다()
    {
        SquareGraph g = new GraphBuilder()
            .SetSize(2, 2)
            .Build();

        var first = g.Nodes[0];
        var second = g.Nodes[1];
        var third = g.Nodes[2];
        var fourth = g.Nodes[3];
        Assert.Contains(second, first.Right.ConnectNodes);
        Assert.Contains(third, first.Bottom.ConnectNodes);
        Assert.Contains(fourth, third.Right.ConnectNodes);
        Assert.Contains(fourth, second.Bottom.ConnectNodes);
    }

    [Fact]
    public void 단순한_2x2_퍼즐을_완성한다()
    {
        SquareGraph g = new GraphBuilder()
            .SetSize(2, 2)
            .Build();

        var result = g.Solve();

        Assert.Equal(2, result.GetLength(0));
        Assert.Equal(2, result.GetLength(1));
        Assert.Equal(1, result[0, 0]);
        Assert.Equal(2, result[0, 1]);
        Assert.Equal(3, result[1, 0]);
        Assert.Equal(4, result[1, 1]);
    }

    [Fact]
    public void 회전하는_2x2_퍼즐을_완성한다()
    {
        var g = new SquareGraph(4);

        var node1 = g.Nodes[0];
        var node2 = g.Nodes[1];
        var node3 = g.Nodes[2];
        var node4 = g.Nodes[3];

        node1.Top.Connect(node3.Bottom);
        node3.Right.Connect(node2.Bottom);
        node2.Right.Connect(node4.Bottom);
        node4.Right.Connect(node1.Right);

        var result = g.Solve();

        Assert.Equal(2, result.GetLength(0));
        Assert.Equal(2, result.GetLength(1));
        Assert.Equal(1, result[0, 0]);
        Assert.Equal(3, result[0, 1]);
        Assert.Equal(4, result[1, 0]);
        Assert.Equal(2, result[1, 1]);
    }

    [Fact]
    public void 첫퍼즐이_코너가아닌_2x2_퍼즐을_완성한다()
    {
        var g = new SquareGraph(4);

        var node1 = g.Nodes[0];
        var node2 = g.Nodes[1];
        var node3 = g.Nodes[2];
        var node4 = g.Nodes[3];

        node1.Top.Connect(node3.Bottom);
        node3.Right.Connect(node2.Bottom);
        node2.Right.Connect(node4.Bottom);
        node4.Right.Connect(node1.Right);
        node1.Left.Connect(node3.Left); // 1, 3번 퍼즐한테 의미없는 연결을 추가

        var result = g.Solve();

        Assert.Equal(2, result.GetLength(0));
        Assert.Equal(2, result.GetLength(1));
        Assert.Equal(2, result[0, 0]);
        Assert.Equal(4, result[0, 1]);
        Assert.Equal(3, result[1, 0]);
        Assert.Equal(1, result[1, 1]);
    }

    [Fact]
    public void 단순한_3x3_퍼즐을_완성한다()
    {
        SquareGraph g = new GraphBuilder()
            .SetSize(3, 3)
            .Build();

        var result = g.Solve();

        Assert.Equal(3, result.GetLength(0));
        Assert.Equal(3, result.GetLength(1));
        Assert.Equal(1, result[0, 0]);
        Assert.Equal(2, result[0, 1]);
        Assert.Equal(3, result[0, 2]);
        Assert.Equal(4, result[1, 0]);
        Assert.Equal(5, result[1, 1]);
        Assert.Equal(6, result[1, 2]);
        Assert.Equal(7, result[1, 0]);
        Assert.Equal(8, result[1, 1]);
        Assert.Equal(9, result[1, 2]);
    }
}


