using Common.PieceInfo;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace zTest_Common;

public class PointConverterTest
{
    class PointClass
    {
        public PointF Point { get; set; }
    }
    [Fact]
    public void PointF_serialize_test()
    {
        var point = new PointF(1.78f, 2.874f);
        var obj = new PointClass { Point = point };
        var option = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new PointFJsonConverter() },
        };
        var pointJsonText = JsonSerializer.Serialize(obj, option);

        var expected = @"{
  ""Point"": ""(1.78, 2.874)""
}";
        Assert.Equal(expected, pointJsonText);
    }

    [Fact]
    public void PointF_deserialize_test()
    {
        var pointJsonText = @"{
    ""Point"": ""(1.78, 2.874)""
}";
        var option = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new PointFJsonConverter() },
        };
        var obj = JsonSerializer.Deserialize<PointClass>(pointJsonText, option);

        var expected = new PointF(1.78f, 2.874f);
        Assert.Equal(expected, obj!.Point);
    }

    class PointArrayClass
    {
        public PointF[] Points { get; set; }
    }

    [Fact]
    public void PointArray_serialize_test()
    {
        var points = new PointF[]
        {
            new(1.78f, 2.874f),
            new(3, 4),
        };
        var obj = new PointArrayClass { Points = points };

        var option = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new PointArrayJsonConverter() },
        };
        var pointJsonText = JsonSerializer.Serialize(obj, option);

        var expected = @"{
  ""Points"": ""[(1.78, 2.874), (3, 4)]""
}";
        Assert.Equal(expected, pointJsonText);
    }

    [Fact]
    public void PointArray_deserialize_test()
    {
        var pointJsonText = @"{
  ""Points"": ""[(1.78, 2.874), (3, 4)]""
}";
        var option = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new PointArrayJsonConverter() },
        };
        var obj = JsonSerializer.Deserialize<PointArrayClass>(pointJsonText, option);

        var expected = new PointF[]
        {
            new(1.78f, 2.874f),
            new(3, 4),
        };
        Assert.Equal(expected, obj!.Points);
    }
}