using System.Text.Json;
using System.Text.Json.Serialization;
using JkwExtensions;

namespace PuzzleTableHelperCore;

/*
Sample:
[
  "(0,0) (9,1) (2,3) (3,2)",
  "(10,0) (19,1) (12,3) (13,2)"
]
*/
public class PuzzleTableJsonConverter : JsonConverter<PuzzleTable>
{
    public override PuzzleTable Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();
        reader.Read();

        var cells = new List<List<PuzzleCell>>();
        while (reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException();
            var text = reader.GetString();
            var sep = new[] { ' ' };
            var arr = text!.Split(sep, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var row = new List<PuzzleCell>();
            for (var i = 0; i < arr.Length; i++)
            {
                var sep2 = new[] { ',', '(', ')' };
                var arr2 = arr[i].Split(sep2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                var pieceNumber = int.Parse(arr2[0]);
                var topEdgeIndex = int.Parse(arr2[1]);

                row.Add(new PuzzleCell
                {
                    Row = cells.Count,
                    Column = i,
                    PieceName = $"Piece_{pieceNumber:00000}",
                    PieceNumber = pieceNumber,
                    TopEdgeIndex = topEdgeIndex,
                });
            }
            cells.Add(row);

            reader.Read();
        }

        return new PuzzleTable
        {
            Cells = cells,
        };
    }

    public override void Write(Utf8JsonWriter writer, PuzzleTable value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var row in value.Cells)
        {
            var rowText = row.Select(x => $"({x.PieceNumber},{x.TopEdgeIndex})")
                .StringJoin(" ");
            writer.WriteStringValue(rowText);
        }
        writer.WriteEndArray();
    }
}

