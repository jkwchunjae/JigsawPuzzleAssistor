using System.Drawing;
using System.Text.Json.Serialization;
using System.Text.Json;
using JkwExtensions;

namespace Common.PieceInfo;

public class PointFArrayJsonConverter : JsonConverter<PointF[]>
{
    public override PointF[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var pointArrayText = reader.GetString(); // [(1.78, 2.874), (3, 4), ... ]
        var sep = new[] { '(', ')', ',', '[', ']' };
        var numberTextArray = pointArrayText!.Split(sep, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var numberArray = numberTextArray.Select(float.Parse).ToArray();

        var points = new List<PointF>();
        for (var i = 0; i < numberArray.Length; i += 2)
        {
            points.Add(new PointF(numberArray[i], numberArray[i + 1]));
        }
        return points.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, PointF[] points, JsonSerializerOptions options)
    {
        var pointArrayText = points.Select(p => $"({p.X:0.000}, {p.Y:0.000})").StringJoin("[", ", ", "]");
        writer.WriteStringValue(pointArrayText);
    }
}

