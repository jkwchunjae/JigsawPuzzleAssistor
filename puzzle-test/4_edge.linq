<Query Kind="Program">
  <Output>DataGrids</Output>
  <NuGetReference>Emgu.CV</NuGetReference>
  <NuGetReference>Emgu.CV.runtime.windows</NuGetReference>
  <Namespace>System.Net</Namespace>
  <Namespace>Emgu.CV</Namespace>
  <Namespace>Emgu.CV.Structure</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>Emgu.CV.CvEnum</Namespace>
  <Namespace>Emgu.CV.Features2D</Namespace>
  <Namespace>Emgu.CV.Util</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	var init = CvInvoke.Init();

	var sourceDir = @"D:\puzzle\1_resize";

	var pairs = Directory.GetFiles(sourceDir)
		.Select(source => new
		{
			SourcePath = source,
			TargetPath = source.Replace("1_resize", "4_edge"),
		})
		.ToList();

	foreach (var pair in pairs.Where(p => p.SourcePath.Contains("00002")))
	{
		var outline = new Outline(pair.SourcePath);
		await outline.ProcessAsync();

		var outlineImage = outline.GetImage();
		var corner = new Corner(outline);
		var corners = corner.GetCorners();
		// corner.WriteTo(outlineImage, radius: 10);
		var piece = new Piece(corner);

		if (piece.NewCorner?.Count() == 4)
		{
			outlineImage[piece.NewCorner[0]] = new Bgr(Color.White);
			outlineImage[piece.NewCorner[1]] = new Bgr(Color.White);
			outlineImage[piece.NewCorner[2]] = new Bgr(Color.White);
			outlineImage[piece.NewCorner[3]] = new Bgr(Color.White);
			
			outlineImage[corners[0].ToPoint()] = new Bgr(Color.Red);
			outlineImage[corners[1].ToPoint()] = new Bgr(Color.Red);
			outlineImage[corners[2].ToPoint()] = new Bgr(Color.Red);
			outlineImage[corners[3].ToPoint()] = new Bgr(Color.Red);
			
			//CvInvoke.Circle(outlineImage, piece.NewCorner[0], 1, new MCvScalar(0, 255, 255));
			//CvInvoke.Circle(outlineImage, piece.NewCorner[1], 1, new MCvScalar(0, 255, 255));
			//CvInvoke.Circle(outlineImage, piece.NewCorner[2], 1, new MCvScalar(0, 255, 255));
			//CvInvoke.Circle(outlineImage, piece.NewCorner[3], 1, new MCvScalar(0, 255, 255));
		}
		var edges = piece.Edges;
		if (edges.Count() == 4)
		{
			edges[0].OriginPointPrintTo(outlineImage, new Bgr(0, 0, 255));
			edges[1].OriginPointPrintTo(outlineImage, new Bgr(0, 255, 255));
			edges[2].OriginPointPrintTo(outlineImage, new Bgr(255, 0, 255));
			edges[3].OriginPointPrintTo(outlineImage, new Bgr(255, 255, 0));
		}
		CvInvoke.Imwrite(pair.TargetPath, outlineImage);
	}
}

public class Piece
{
	public string Name => Corner.Name;
	public Corner Corner { get; init; }
	public List<Edge> Edges { get; init; }
	public Point[] FirstContour { get; init; }
	public Point[] OrderContour1 { get; init; }
	public Point[] NewCorner { get; init; }
	
	public Piece(Corner corner)
	{
		Corner = corner;

		if (corner.GetCorners().Count() != 4)
		{
			Edges = new();
			return;
		}

		var puzzleContours = Corner.Outline.GetContour();
		var corners = Corner.GetCorners();
		var orderedContour = ReorderContour(puzzleContours, corners.First().ToPoint());
		FirstContour = orderedContour;
		var orderedCorners = corners
			.Select((corner, i) =>
			{
				var newCorner = orderedContour
					.Select((point, contourIndex) => new
					{
						Index = i,
						Point = point,
						ContourIndex = contourIndex,
						Distance = Distance(corner, point),
					})
					.OrderBy(x => x.Distance)
					.ToArray();
				return newCorner.First();
			})
			.ToArray();
			
		if (orderedCorners[1].ContourIndex < orderedCorners[2].ContourIndex)
		{
			// contour의 순서와 corner의 순서가 같다.
		}
		else
		{
			// contour의 순서와 corner의 순서가 반대다.
			// corner의 순서는 바뀌면 안된다. contour를 뒤집어서 사용한다.
			orderedContour = new [] { orderedContour.First() }
				.Concat(orderedContour.Skip(1).Reverse())
				.ToArray();
			orderedCorners = orderedCorners
				.Select(corner => new
				{
					corner.Index,
					corner.Point,
					ContourIndex = orderedContour
						.Select((point, contourIndex) => new { Point = point, ContourIndex = contourIndex })
						.Where(x => x.Point == corner.Point)
						.First()
						.ContourIndex,
					corner.Distance,
				})
				.ToArray();
		}
		NewCorner = orderedCorners.Select(x => x.Point).ToArray();
		OrderContour1 = orderedContour;

		Edges = orderedCorners
			.Select(corner =>
			{
				var nextCorner = orderedCorners[(corner.Index + 1) % 4];
				var beginIndex = corner.ContourIndex;
				var endIndex = nextCorner.ContourIndex;
				
				var edge = orderedContour
					.Skip(beginIndex == 0 ? 0 : beginIndex - 1)
					.Take(endIndex == 0 ? orderedContour.Count() - beginIndex : endIndex - beginIndex + 1)
					.ToList();
				if (endIndex == 0)
				{
					edge.Add(orderedContour.First());
				}
					
				return edge.ToArray();
			})
			.Select(points => new Edge(points))
			.ToList();
	}

	Point[] ReorderContour(Point[] contour, Point first)
	{
		var bag = contour.ToList();
		var result = new List<Point>();

		while (bag.Any())
		{
			var curr = result.Any() ? result.Last() : first;
			var ordered = bag
				.Select(other => new { Point = other, Distance = Distance(curr, other) })
				.OrderBy(x => x.Distance);

			var nearest = ordered.First();
			bag.Remove(nearest.Point);
			if (nearest.Distance < 100)
			{
				result.Add(nearest.Point);
			}
			else
			{
				var a = 1;
			}
		}
		return result.ToArray();
	}
}

public enum EdgeType
{
	Hole, Head, Line,
}

public class Edge
{
	public PointF[] Pointss;
	public PointF[] NormalizedPoints;
	public EdgeType Type;
	public Edge(IEnumerable<PointF> points)
	{
		Pointss = points.ToArray();
		//Pointss = points.ToArray();
		NormalizedPoints = Normalize(Pointss);
		Type = GetType(NormalizedPoints);
		var interpolated = NormalizedPoints;
		if (IsHole)
		{
			NormalizedPoints = Reverse(interpolated);
		}
		else
		{
			NormalizedPoints = interpolated;
		}
	}
	public Edge(IEnumerable<Point> points)
		:this (points.Select(p => (PointF)p))
	{
	}

	public bool IsHead => Type == EdgeType.Head;
	public bool IsHole => Type == EdgeType.Hole;
	public bool IsLine => Type == EdgeType.Line;


	private static EdgeType GetType(PointF[] normalizedPoint)
	{
		var maxY = Math.Abs(normalizedPoint.Max(p => p.Y));
		var minY = Math.Abs(normalizedPoint.Min(p => p.Y));

		if (maxY < 10 && minY < 10)
		{
			return EdgeType.Line;
		}
		else if (maxY > minY)
		{
			return EdgeType.Hole;
		}
		else if (maxY < minY)
		{
			return EdgeType.Head;
		}
		throw new Exception();
	}

	public double Test(Edge other)
	{
		double distanceSum = Pointss
			.Sum(thisPoint => other.Pointss.Min(otherPoint => Distance(thisPoint, otherPoint)));

		return distanceSum;
	}
	
	public static PointF[] Normalize(IEnumerable<PointF> points)
	{
		var angle = CalculateAngleBetweenPoints(points.First(), points.Last());
		var result = points
			.Select(point => new PointF(point.X - points.First().X, point.Y - points.First().Y))
			.Select(point => RotatePointAroundOrigin(point, -angle))
			.ToArray();
			
		return result;
	}

	public static PointF[] Reverse(PointF[] points)
	{
		PointF first = points.OrderBy(p => p.X).First();
		PointF last = points.OrderBy(p => p.X).Last();

		var angle = CalculateAngleBetweenPoints(last, first);
		var reversed = points
			.Select(p => new PointF(p.X - last.X, p.Y - last.Y))
			.Select(p => RotatePointAroundOrigin(p, -angle))
			.ToArray();
		return reversed;
	}

	public void NormalizedPointPrintTo(Image<Bgr, byte> board, PointF basePoint, MCvScalar color)
	{
		foreach (var point in NormalizedPoints)
		{
			var x = point.X + basePoint.X;
			var y = point.Y + basePoint.Y;

			CvInvoke.Circle(board, new Point((int)x, (int)y), 1, color, -1);
		}
	}
	
	public void OriginPointPrintTo(Image<Bgr, byte> image, Bgr color)
	{
		foreach (var point in Pointss)
		{
			//image[point.ToPoint()] = color;
			CvInvoke.Circle(image, point.ToPoint(), 1, color.ToScalar(), -1);
		}
	}
}

public class Corner
{
	public string Name => Outline.Name;
	public Outline Outline { get; init; }
	private PointF[] Corners { get; set; }
	
	public Corner(Outline outline)
	{
		Outline = outline;
		
		Process();
	}
	
	private void Process()
	{
		if (Corners != null)
			return;

		var d = new Emgu.CV.Features2D.GFTTDetector(4, 0.01, 200, 3);
		var corners = d.Detect(Outline.GetImage());

		// 점을 시계방향으로 정렬
		var baseCorner = corners.OrderBy(c => c.Point.Y).First();
		var otherCorners = corners.OrderBy(c => c.Point.Y).Skip(1).ToArray();
		otherCorners = otherCorners
			.Select(c => new { Corner = c, Angle = CalculateAngleBetweenPoints(baseCorner.Point, c.Point) })
			.OrderBy(c => c.Angle)
			.Select(x => x.Corner)
			.ToArray();
		var orderedCorner = new[] { baseCorner }.Concat(otherCorners).ToArray();
		
		Corners = orderedCorner
			.Select(c => c.Point)
			.Select(p => (PointF)NewCorner3(p.ToPoint()))
			.ToArray();
	}


	public Point NewCorner3(Point corner)
	{
		// CvInvoke.Circle(image, corner, 1, new MCvScalar(0, 255, 0), -1);

		var near = Outline.GetContour()
			.Where(c => Distance(corner, c) < 30)
			.Select((c, i) => new
			{
				Point = c,
				Index = i,
				DistanceFromCorner = Distance(corner, c),
			})
			.ToArray();

		foreach (var point in near)
		{
			//image[point.Point] = new Bgr(Color.LightGreen);
		}

		var minPoint = new Point(near.Min(p => p.Point.X) - 10, near.Min(p => p.Point.Y) - 10);
		var maxPoint = new Point(near.Max(p => p.Point.X) + 10, near.Max(p => p.Point.Y) + 10);

		var orderedNear = near.OrderByDescending(x => x.DistanceFromCorner).ToArray();
		Point farPoint1 = orderedNear.First().Point;
		Point farPoint2 = orderedNear
			.Where(x => Distance(x.Point, farPoint1) > 20)
			.First()
			.Point;

		var line1 = MedianLine(farPoint1);
		//DrawLine(line1);
		var line2 = MedianLine(farPoint2);
		//DrawLine(line2);

		//CvInvoke.Circle(image, farPoint1, 2, new MCvScalar(255, 0, 0), -1);
		//CvInvoke.Circle(image, farPoint2, 2, new MCvScalar(255, 255, 0), -1);

		var intersection = CalculateIntersection(line1, line2);
		//CvInvoke.Circle(image, intersection.ToPoint(), 1, new MCvScalar(255, 0, 255), -1);

		return intersection.ToPoint();

		void DrawLine(LineData line)
		{
			if (line.horizotal)
			{
				var p1 = new Point((int)line.xValue, minPoint.Y);
				var p2 = new Point((int)line.xValue, maxPoint.Y);
				//CvInvoke.Line(image, p1, p2, new MCvScalar(0, 255, 255), 1);
			}
			else
			{
				var p1 = new Point(minPoint.X, (int)(line.angle * minPoint.X + line.yValue));
				var p2 = new Point(maxPoint.X, (int)(line.angle * maxPoint.X + line.yValue));
				//CvInvoke.Line(image, p1, p2, new MCvScalar(0, 255, 255), 1);
			}
		}

		LineData MedianLine(Point farPoint)
		{
			var fromMe = near
				.OrderBy(x => Distance(farPoint, x.Point))
				.Where(x => Distance(farPoint, x.Point) > 10)
				.ToList();
			//.SkipWhile(x => Distance(farPoint1, x.Point) < 10)
			//.TakeWhile(x => x.DistanceFromCorner > 2)
			//.ToArray();
			var firstTarget = fromMe.First();
			var targets = new[] { firstTarget }.ToList();
			while (fromMe.Any())
			{
				var last = targets.Last();
				var closeFromLast = fromMe
					.OrderBy(x => Distance(last.Point, x.Point))
					.First();

				if (closeFromLast.DistanceFromCorner < 4)
					break;
				targets.Add(closeFromLast);
				fromMe.Remove(closeFromLast);
			}

			foreach (var point in targets)
			{
				//image[point.Point] = new Bgr(Color.Red);
			}
			var lines = targets
				.Select(target => MakeLine(farPoint, target.Point))
				.ToArray();

			// v1 median
			var medianLine = lines
				.OrderBy(line => line.angle)
				.ToArray()
				[lines.Count() / 2];
			return medianLine;

			// v2 average
			//var avgAngle = lines.Select(line => line.angle).Average();
			//
			//if (avgAngle > 99999)
			//{
			//	var xValue = lines.Where(line => line.horizotal).Select(line => line.xValue).Average();
			//	return new LineData(999999, 0, true, xValue);
			//}
			//else
			//{
			//	var avgYValue = lines.Select(line => line.yValue).Average();
			//	return new LineData(avgAngle, avgYValue, false, 0);
			//}
		}

		LineData MakeLine(PointF p1, PointF p2)
		{
			if (p1.X == p2.X)
			{
				return new LineData(999999, 0, true, p1.X);
			}
			else
			{
				double m = (p2.Y - p1.Y) / (p2.X - p1.X);
				double b = p1.Y - m * p1.X;
				return new LineData(m, b, false, 0);
			}
		}
	}

	public PointF[] GetCorners()
	{
		return Corners;
	}

	public void WriteTo(Image<Bgr, byte> image, int radius)
	{
		foreach (var corner in Corners)
		{
			CvInvoke.Circle(image, corner.ToPoint(), radius, new MCvScalar(255, 0, 255), 1);
		}
	}

	record LineData(double angle, double yValue, bool horizotal, double xValue);

	private PointF CalculateIntersection(LineData line1, LineData line2)
	{
		if (line1.horizotal)
		{
			var x = line1.xValue;
			var y = line2.angle * x + line2.yValue;
			return new PointF((float)x, (float)y);
		}
		else if (line2.horizotal)
		{
			var x = line2.xValue;
			double y = line1.angle * x + line1.yValue;

			return new PointF((float)x, (float)y);
		}
		else
		{
			double x = (line2.yValue - line1.yValue) / (line1.angle - line2.angle);
			double y = line1.angle * x + line1.yValue;

			return new PointF((float)x, (float)y);
		}
	}
}

public class Outline
{
	public string Name { get; init; }

	private Point[] Contour { get; set; }
	private Image<Bgr, byte> _raw { get; init; }
	private Image<Bgr, byte> _outlineImage { get; set; }

	public Outline(string imagePath)
	{
		Name = Path.GetFileNameWithoutExtension(imagePath);
		Mat image = CvInvoke.Imread(imagePath);
		Image<Bgr, byte> rawImage = new Image<Bgr, byte>(image.Width, image.Height);
		image.CopyTo(rawImage);

		_raw = rawImage;
	}

	public Task ProcessAsync()
	{
		return Task.Run(() =>
		{
			var image = _raw;
			Image<Gray, byte> gray = new Image<Gray, byte>(image.Width, image.Height);
			CvInvoke.CvtColor(image, gray, ColorConversion.Bgr2Gray);
			Image<Gray, byte> gaussian = new Image<Gray, byte>(image.Width, image.Height);
			CvInvoke.GaussianBlur(gray, gaussian, new Size(5, 5), 0);
			Image<Gray, byte> blackwhite = new Image<Gray, byte>(image.Width, image.Height);
			CvInvoke.Threshold(gaussian, blackwhite, 127, 255, ThresholdType.Binary);

			Image<Gray, byte> cannyResult = new Image<Gray, byte>(image.Width, image.Height);
			CvInvoke.Canny(blackwhite, cannyResult, 50, 255);

			VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
			Mat hierarchy = new Mat();
			CvInvoke.FindContours(cannyResult, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxNone);

			VectorOfPoint puzzleContours = contours[0];
			int puzzleContoursIndex = 0;
			for (var i = 0; i < contours.Size; i++)
			{
				if (puzzleContours.Size < contours[i].Size)
				{
					puzzleContours = contours[i];
					puzzleContoursIndex = i;
				}
			}
			// image에 outline을 그렸음
			// CvInvoke.DrawContours(image, contours, puzzle_contours, new MCvScalar(0, 255, 0), 2);

			var outline = new Image<Bgr, byte>(image.Width, image.Height, new Bgr(0, 0, 0));
			CvInvoke.DrawContours(outline, contours, puzzleContoursIndex, new MCvScalar(0, 255, 0), 1);


			var contour = new List<Point>();
			for (var i = 0; i < puzzleContours.Size; i++)
			{
				if (contour.Contains(puzzleContours[i]))
				{
				}
				else
				{
					contour.Add(puzzleContours[i]);
				}
			}
			Contour = contour.ToArray();
			_outlineImage = outline;
		});
	}
	
	public Image<Bgr, byte> GetImage()
	{
		if (_outlineImage == null)
		{
			throw new Exception("run process");
		}

		return _outlineImage;
	}
	
	public Point[] GetContour()
	{
		if (Contour == null)
		{
			throw new Exception("run process");
		}
		
		return Contour;
	}
}

static double CalculateAngleBetweenPoints(PointF point1, PointF point2)
{
	double deltaX = point2.X - point1.X;
	double deltaY = point2.Y - point1.Y;

	// 아크탄젠트 함수를 사용하여 각도를 계산합니다.
	double angleInRadians = Math.Atan2(deltaY, deltaX);
	
	return angleInRadians;
}


static double CalculateDistance(PointF p1, PointF p2, PointF p3) {
	double px = p3.X - p2.X;
	double py = p3.Y - p2.Y;
	double something = px * px + py * py;
	double u = ((p1.X - p2.X) * px + (p1.Y - p2.Y) * py) / something;

	if (u > 1)
	{
		u = 1;
	}
	else if (u < 0)
	{
		u = 0;
	}

	double x = p2.X + u * px;
	double y = p2.Y + u * py;
	double dx = x - p1.X;
	double dy = y - p1.Y;
	double dist = Math.Sqrt(dx * dx + dy * dy);

	return dist;
}

static PointF RotatePointAroundOrigin(PointF point, double angleRad)
{
	var x = point.X;
	var y = point.Y;
	
	var rotatedX = x * Math.Cos(angleRad) - y * Math.Sin(angleRad);
	var rotatedY = x * Math.Sin(angleRad) + y * Math.Cos(angleRad);
	
	return new PointF((float)rotatedX, (float)rotatedY);
}

static double Distance(PointF p1, PointF p2)
{
	double dx = p2.X - p1.X;
	double dy = p2.Y - p1.Y;

	return Math.Sqrt(dx * dx + dy * dy);
}

public static class Ex
{
	public static Point ToPoint(this PointF point)
	{
		return new Point((int)point.X, (int)point.Y);
	}
	
	public static MCvScalar ToScalar(this Bgr bgr)
	{
		return new MCvScalar(bgr.Blue, bgr.Green, bgr.Red, 0); // The fourth value is alpha, typically 0
	}
	
	public static Bgr ToBgr(this MCvScalar scalar)
	{
		return new Bgr(scalar.V2, scalar.V1, scalar.V0);
	}
}

























