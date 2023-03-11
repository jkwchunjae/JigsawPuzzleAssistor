namespace zTest_SquareGraph;

public class UnitTest1
{
    [Fact]
    public void 단순한_2x2_퍼즐을_완성한다()
    {
        var g = new SquareGraph(4);

        // 1 2
        // 3 4
        var n1 = g.GetNode(1)!;
        var n2 = g.GetNode(2)!;
        var n3 = g.GetNode(3)!;
        var n4 = g.GetNode(4)!;

        n1.Edge2.Connect(n2.Edge1);
        n1.Edge3.Connect(n3.Edge0);
        n2.Edge0.Connect(n4.Edge3);
        n3.Edge1.Connect(n4.Edge2);

        var result = g.Solve();

        Assert.Equal(2, result.GetLength(0));
        Assert.Equal(2, result.GetLength(1));
        Assert.Equal(1, result[0, 0]);
        Assert.Equal(2, result[0, 1]);
        Assert.Equal(3, result[1, 0]);
        Assert.Equal(4, result[1, 1]);
    }
}


