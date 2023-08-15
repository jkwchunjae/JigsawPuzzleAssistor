<Query Kind="Program">
  <Reference Relative="..\..\JkwExtensions\JkwExtensions\bin\Debug\netcoreapp3.1\JkwExtensions.dll">D:\jkw\major\GitHub\JkwExtensions\JkwExtensions\bin\Debug\netcoreapp3.1\JkwExtensions.dll</Reference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>JkwExtensions</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Text.Json</Namespace>
</Query>

void Main()
{
	var options = new JsonSerializerOptions
	{
		AllowTrailingCommas = true,
		DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
		WriteIndented = true,
	};
	options.Converters.Add(new PointArrayJsonConverter());
	options.Converters.Add(new PointJsonConverter());
	options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
	
	var p1 = new Point(123, 421);

	var result = System.Text.Json.JsonSerializer.Serialize(p1, options);
	result.Dump();

	var p2 = System.Text.Json.JsonSerializer.Deserialize<Point>(result, options);
	p2.Dump();

	var points = new Point[] { new Point(1, 2), new Point(842673, 943782) };
	var r = System.Text.Json.JsonSerializer.Serialize(points, options);
	r.Dump();
	var points2 = System.Text.Json.JsonSerializer.Deserialize<Point[]>(r, options);
	points2.Dump();
	
	var edge = new Edge
	{
		EdgeType = EdgeType.Hole,
		OriginPoints = new[] { new Point(123, 423), new Point(4356, 231), new Point(56, 98423)},
		OriginCorner1 = new Point(743, 42398),
		OriginCorner2 = new Point(943578, 23978),
	};
	
	var edgeText = System.Text.Json.JsonSerializer.Serialize(edge, options);
	
	edgeText.Dump();
	
	var edge2 = System.Text.Json.JsonSerializer.Deserialize<Edge>(edgeText, options);
	edge2.Dump();
}

public class Edge
{
	public Point[] OriginPoints { get; init; }
	public Point OriginCorner1 { get; init; }
	public Point OriginCorner2 { get; init; }
	public EdgeType EdgeType { get; init; }
}

public enum EdgeType
{
	Hole, Head, Line,
}

public record Point(int X, int Y);

public class EdgeJsonConverter : System.Text.Json.Serialization.JsonConverter<Edge>
{
	public override Edge Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, Edge value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}

public class PointJsonConverter : System.Text.Json.Serialization.JsonConverter<Point>
{
	private static readonly Regex _regex = new Regex(@"\s*\(\s*(\d+)\s*,\s*(\d+)\s*\)\s*");
	public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var text = reader.GetString();
		var match = _regex.Match(text);
		
		if (match.Success)
		{
			var x = match.Groups[1].Value.ToInt();
			var y = match.Groups[2].Value.ToInt();
			return new Point(x, y);
		}

		throw new Exception($"Point convert error: {text}");
	}

	public override void Write(Utf8JsonWriter writer, Point point, JsonSerializerOptions options)
	{
		var pointText = $"({point.X},{point.Y})";
		writer.WriteStringValue(pointText);
	}
}

public class PointArrayJsonConverter : System.Text.Json.Serialization.JsonConverter<Point[]>
{
	private static readonly Regex _regex = new Regex(@"\(\s*(\d+)\s*,\s*(\d+)\s*\)(?:,\s*\(\s*(\d+)\s*,\s*(\d+)\s*\))*");
	public override Point[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var text = reader.GetString();
		var match = _regex.Match(text);
		var result = new List<Point>();
		for (var i = 1; i < match.Groups.Count; i+=2)
		{
			var x = match.Groups[i].Value.ToInt();
			var y = match.Groups[i + 1].Value.ToInt();
			var point = new Point(x, y);
			result.Add(point);
		}
		return result.ToArray();
	}

	public override void Write(Utf8JsonWriter writer, Point[] points, JsonSerializerOptions options)
	{
		var pointsText = string.Join(",", points.Select(p => $@"({p.X},{p.Y})"));
		writer.WriteStringValue(pointsText);
	}
}

