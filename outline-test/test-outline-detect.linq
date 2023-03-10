<Query Kind="Program">
  <NuGetReference>Jkw.Extensions</NuGetReference>
  <Namespace>JkwExtensions</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <IncludeAspNet>true</IncludeAspNet>
</Query>

void Main()
{
	var originPath = Util.CurrentQuery.Location + @"\Pieces_first.png";
	var outlinePath = Util.CurrentQuery.Location + @"\Pieces_outline.bmp";
	
	var buttonContainer = new DumpContainer();
	var originContainer = new DumpContainer();
	var imageContainer = new DumpContainer();

	Func<string, Contour> GetContour = (imagePath) =>
	{
		using (Bitmap outlineImage = new Bitmap(imagePath))
		{
			return GetRedsContour(outlineImage);
		}
	};
	Contour outline = GetContour(outlinePath);

	using (Bitmap image = new Bitmap(originPath))
	{
		originContainer.Content = Util.Image(image, Util.ScaleMode.ResizeTo(500));
		
		int chunk = 32;
		var target = new Bitmap(image.Width / chunk + 1, image.Height / chunk + 1);
		foreach (var p in Iterate(image, chunk))
		{
			var averageColor = GetAverage(image, chunk, p.Row, p.Column);
			// target.SetPixel(p.Column, p.Row, averageColor);
			FillColor(image, chunk, p.Row, p.Column, averageColor);
		};
		
		outline.Points.ForEach(p => image.SetPixel(p.X, p.Y, Color.Red));
		
		imageContainer.Content = Util.Image(image, Util.ScaleMode.ResizeTo(500));
	}
	
	imageContainer.Dump();
}

Color GetAverage(Bitmap image, int chunkSize, int row, int column)
{
	var result = Iterate(image, chunkSize, row, column)
		.Select(p => image.GetPixel(p.X, p.Y))
		.Aggregate<Color, (int R, int G, int B, int A, int Count)>((0, 0, 0, 0, 0), (sum, p) => (sum.R + p.R, sum.G + p.G, sum.B + p.B, sum.A + p.A, sum.Count + 1));
		
	var count = result.Count;
	return Color.FromArgb(result.A / count, result.R / count, result.G / count, result.B / count);
}

void FillColor(Bitmap image, int chunkSize, int row, int column, Color color)
{
	Iterate(image, chunkSize, row, column)
		.ForEach(p => image.SetPixel(p.X, p.Y, color));
}

IEnumerable<(int Row, int Column)> Iterate(Image image, int chunkSize)
{
	for (var row = 0; row < image.Height; row += chunkSize)
	{
		for (var column = 0; column < image.Width; column += chunkSize)
		{
			yield return (row / chunkSize, column / chunkSize);
		}
	}
}

IEnumerable<(int X, int Y)> Iterate(Image image, int chunkSize, int row, int column)
{
	var beginY = chunkSize * row;
	var endY = beginY + chunkSize;
	var beginX = chunkSize * column;
	var endX = beginX + chunkSize;
	
	return Iterate(beginX, endX, beginY, endY)
		.Where(p => p.X >= 0 && p.X < image.Width)
		.Where(p => p.Y >= 0 && p.Y < image.Height);
}

IEnumerable<(int X, int Y)> Iterate(int x1, int x2, int y1, int y2)
{
	for (var y = y1; y < y2; y++)
	{
		for (var x = x1; x < x2; x++)
		{
			yield return (x, y);
		}
	}
}

Contour GetRedsContour(Bitmap image)
{
	return Enumerable.Range(0, image.Height)
		.Join(Enumerable.Range(0, image.Width), y => true, x => true, (y, x) => (X: x, Y: y))
		.Where(p => image.GetPixel(p.X, p.Y) is { R: > 200, G: < 30 })
		.Aggregate(new Contour(), (contour, p) => contour.Add(p.X, p.Y));
}

double Similar(Color c1, Color c2)
{
	return Math.Sqrt(Math.Pow(c1.R - c2.R, 2) + Math.Pow(c1.G - c2.G, 2) + Math.Pow(c1.B - c2.B, 2));
}

public class Contour
{
	public List<(int X, int Y)> Points = new();
	
	public Contour Add((int X, int Y) point)
	{
		Points.Add(point);
		return this;
	}
	public Contour Add(int x, int y)
	{
		Points.Add((x, y));
		return this;
	}
}