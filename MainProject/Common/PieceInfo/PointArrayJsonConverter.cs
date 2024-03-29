﻿using System.Drawing;
using System.Text.Json.Serialization;
using System.Text.Json;
using JkwExtensions;

namespace Common.PieceInfo;

public class PointArrayJsonConverter : JsonConverter<Point[]>
{
    public override Point[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var pointArrayText = reader.GetString(); // [(1, 2), (3, 4), ... ]
        var sep = new[] { '(', ')', ',', '[', ']' };
        var numberTextArray = pointArrayText!.Split(sep, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var numberArray = numberTextArray.Select(int.Parse).ToArray();

        var points = new List<Point>();
        for (var i = 0; i < numberArray.Length; i += 2)
        {
            points.Add(new Point(numberArray[i], numberArray[i + 1]));
        }
        return points.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, Point[] points, JsonSerializerOptions options)
    {
        var pointArrayText = points.Select(p => $"({p.X}, {p.Y})").StringJoin("[", ", ", "]");
        writer.WriteStringValue(pointArrayText);
    }
}

