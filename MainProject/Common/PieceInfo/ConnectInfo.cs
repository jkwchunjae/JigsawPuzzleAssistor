using System.Drawing;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Common.PieceInfo;

public class ConnectInfo
{
    public required string PieceName { get; set; }
    public required ConnectEdge[] Edges { get; set; }

    public IEnumerable<(int MyEdge, int OtherEdge, float Value)> Test(PieceInfo me, PieceInfo other, ConnectInfo otherConnectInfo)
    {
        for (var i = 0; i < me.Edges.Count; i++)
        {
            for (var j = 0; j < other.Edges.Count; j++)
            {
                var (result, value) = me.Edges[i].Test(other.Edges[j]);
                if (result)
                {
                    yield return (i, j, value);
                }
            }
        }
    }
}

public class ConnectEdge
{
    public required int Index { get; set; }
    public required List<ConnectTarget> Connection { get; set; }
}

[JsonConverter(typeof(ConnectTargetJsonConverter))]
public class ConnectTarget
{
    public required string PieceName { get; set; }
    public required int EdgeIndex { get; set; }
    public required float Value { get; set; }
}

public class ConnectTargetJsonConverter : JsonConverter<ConnectTarget>
{
    public override ConnectTarget Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.GetString(); // P23, 3, 23.123
        var sep = new[] { ',' };
        var arr = text!.Split(sep, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var pieceName = arr[0];
        var edgeIndex = int.Parse(arr[1]);
        var value = float.Parse(arr[2]);

        return new ConnectTarget
        {
            PieceName = pieceName,
            EdgeIndex = edgeIndex,
            Value = value,
        };
    }

    public override void Write(Utf8JsonWriter writer, ConnectTarget point, JsonSerializerOptions options)
    {
        var text = $"{point.PieceName}, {point.EdgeIndex}, {point.Value:0.000}";
        writer.WriteStringValue(text);
    }
}

