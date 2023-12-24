using System.Drawing;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Common.PieceInfo;

public class PointFJsonConverter : JsonConverter<PointF>
{
    public override PointF Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var pointText = reader.GetString(); // (1.78, 2.874)
        var sep = new[] { '(', ')', ',' };
        var numberTextArray = pointText!.Split(sep, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var numberArray = numberTextArray.Select(float.Parse).ToArray();

        return new PointF(numberArray[0], numberArray[1]);
    }

    public override void Write(Utf8JsonWriter writer, PointF point, JsonSerializerOptions options)
    {
        var pointText = $"({point.X}, {point.Y})";
        writer.WriteStringValue(pointText);
    }
}

