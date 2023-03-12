namespace zTest_SquareGraph;

public class GraphBuilder
{
    private int _width;
    private int _height;
    private bool _shuffle;

    public GraphBuilder SetSize(int width, int height)
    {
        _width = width;
        _height = height;

        return this;
    }

    public GraphBuilder Shuffle()
    {
        _shuffle = true;

        return this;
    }

    public SquareGraph Build()
    {
        var N = _width * _height;
        var g = new SquareGraph(N);

        int[][] table = Enumerable.Range(1, N)
            .Chunk(_width).ToArray();

        for (var column = 0; column < _width; column++)
        {
            for (var row = 0; row < _height; row++)
            {
                var currNode = g.GetNode(table[row][column])!;
                if (column < _width - 1)
                {
                    var rightNode = g.GetNode(table[row][column + 1])!;
                    currNode.Edge1.Connect(rightNode.Edge3);
                }
                if (row < _height - 1)
                {
                    var bottomNode = g.GetNode(table[row + 1][column])!;
                    currNode.Edge2.Connect(bottomNode.Edge0);
                }
            }
        }

        return g;
    }
}

