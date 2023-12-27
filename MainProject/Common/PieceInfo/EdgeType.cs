using System.Text.Json.Serialization;

namespace Common.PieceInfo;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EdgeType
{
    None,
    Hole,
    Head,
    Line,
}

