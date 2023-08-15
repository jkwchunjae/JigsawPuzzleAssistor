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

	var ppp = new List<Piece>();
	foreach (var pair in pairs) //.Where(p => p.SourcePath.Contains("00009")))
	{
		var outline = new Outline(pair.SourcePath);
		await outline.ProcessAsync();

		var outlineImage = outline.GetImage();
		var corner = new Corner(outline);
		var corners = corner.GetCorners();
		// corner.WriteTo(outlineImage, radius: 10);
		var piece = new Piece(corner);

		var edges = piece.Edges;
		if (edges.Count() == 4)
		{
			ppp.Add(piece);
			edges[0].OriginPointPrintTo(outlineImage, new Bgr(0, 0, 255));
			edges[1].OriginPointPrintTo(outlineImage, new Bgr(0, 255, 255));
			edges[2].OriginPointPrintTo(outlineImage, new Bgr(255, 0, 255));
			edges[3].OriginPointPrintTo(outlineImage, new Bgr(255, 255, 0));
		}
		//CvInvoke.Imwrite(pair.TargetPath, outlineImage);
	}
	
	var diffs = ppp
		.AsParallel()
		.SelectMany(p1 =>
		{
			return ppp
				.Where(p2 => p2 != p1)
				.AsParallel()
				.Select(p2 => new { p2 = p2, test = p1.Test(p2, 450) })
				.Where(x => x.test.Item1)
				.Select(x => new
				{
					Name1 = p1.Name,
					Type1 = x.test.Item3.Type,
					Name2 = x.p2.Name,
					Type2 = x.test.Item4.Type,
					Value = x.test.Item2,
					P1 = p1,
					P2 = x.p2,
					Edge1 = x.test.Item3,
					Edge2 = x.test.Item4,
				})
				.ToArray();
		})
		.OrderBy(x => x.Value)
		.ToList()
		.Dump();
	
	foreach (var diff in diffs)
	{
		//	WriteEdge(diff.P1, diff.Edge1, diff.P2, diff.Edge2, (int)diff.Value);
	}

//	var p64 = ppp.First(p => p.Name.Contains("00064"));
//	var p65 = ppp.First(p => p.Name.Contains("00065"));
//	var p64Head = p64.Edges.First(e => e.IsHead);
//	var p65Hole1 = p65.Edges.First(e => e.IsHole);
//	var p65Hole2 = p65.Edges.Last(e => e.IsHole);
//	
//	var test1 = p64Head.Test(p65Hole1);
//	var test2 = p64Head.Test(p65Hole2);
//
//	WriteEdge(p64, p64Head, p65, p65Hole1, (int)test1, "1");
//	WriteEdge(p64, p64Head, p65, p65Hole2, (int)test2, "2");
}

public void WriteEdge(Piece p1, Edge e1, Piece p2, Edge e2, int value, string suffix = "")
{
	var filename = @$"D:\puzzle\5_edge_diff\{p1.Name}_{e1.Type}---{p2.Name}_{e2.Type}_{suffix}--({value}).jpg";
	Image<Bgr, byte> image = new Image<Bgr, byte>(400, 150, new Bgr(Color.White));
	
	e1.NormalizedPointPrintTo(image, new Point(50, 120), new Bgr(Color.Blue));
	e2.NormalizedPointPrintTo(image, new Point(50, 120), new Bgr(Color.Green));
	
	CvInvoke.Imwrite(filename, image);
}

public class Piece
{
	public string Name => Corner.Name;
	public Corner Corner { get; init; }
	public List<Edge> Edges { get; init; }
	public Point[] FirstContour { get; init; }
	public Point[] OrderContour1 { get; init; }
	
	public Piece(Corner corner)
	{
		Corner = corner;

		if (corner.GetCorners().Count() != 4)
		{
			Edges = new();
			return;
		}
		var contour = Corner.Outline.GetContour();
		var corners = Corner.GetCorners();
		
		// 1. 첫 코너에서 가장 가까운 점을 고른다.
		// 2. 그 점에서 가장 가까운 점을 고른다. 계속 반복한다. (외곽점)
		// 3. 외곽점의 순서를 코너의 순서와 맞춘다.
		// 4. 외곽점을 코너에 맞춰서 나눈다. Edge를 만든다.
		
		// 1. 첫 코너에서 가장 가까운 점을 고른다.
		var nearests = corners
			.Select(corner => FindNearestPoint(contour, corner))
			.ToArray();
		// 2. 그 점에서 가장 가까운 점을 고른다. 계속 반복한다. (외곽점)
		var orderedContour = ReorderContour(contour, nearests[0]);
		// 3. 외곽점의 순서를 코너의 순서와 맞춘다.
		var contourIndex1 = orderedContour.FindIndex(p => p == nearests[1]);
		var contourIndex2 = orderedContour.FindIndex(p => p == nearests[2]);
		if (contourIndex1 > contourIndex2)
		{
			// contour의 순서와 corner의 순서가 반대다.
			// corner의 순서는 바뀌면 안된다. contour를 뒤집어서 사용한다.
			orderedContour = new[] { orderedContour.First() }
				.Concat(orderedContour.Skip(1).Reverse())
				.ToArray();
			nearests = corners
				.Select(corner => FindNearestPoint(orderedContour, corner))
				.ToArray();
		}
		// 4. 외곽점을 코너에 맞춰서 나눈다. Edge를 만든다.
		Edges = corners
			.Select((corner, cornerIndex) =>
			{
				var nextCorner = corners[(cornerIndex + 1) % 4];
				var beginContourPoint = nearests[cornerIndex];
				var endContourPoint = nearests[(cornerIndex + 1) % 4];
				var beginIndex = orderedContour.FindIndex(p => p == beginContourPoint);
				var endIndex = orderedContour.FindIndex(p => p == endContourPoint);
				
				var points = orderedContour
					.Skip(beginIndex == 0 ? 0 : beginIndex - 1)
					.Take(endIndex == 0 ? orderedContour.Count() - beginIndex : endIndex - beginIndex + 1)
					.ToList();
				if (endIndex == 0)
				{
					points.Add(orderedContour.First());
				}
				
				return new Edge(points.Select(p => (PointF)p), corner, nextCorner);
			})
			.ToList();
	}

	// 1. 첫 코너에서 가장 가까운 점을 고른다.
	private static Point FindNearestPoint(Point[] contour, PointF corner)
	{
		return contour
			.OrderBy(p => Distance(p, corner))
			.First();
	}
	
	// 2. 그 점에서 가장 가까운 점을 고른다. 계속 반복한다. (외곽점)
	private static Point[] ReorderContour(Point[] contour, Point first)
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
			result.Add(nearest.Point);
		}
		return result.ToArray();
	}

	public (bool, double, Edge, Edge) Test(Piece other, double threshold)
	{
		var tests = new List<(int, Edge, Edge)>();
		foreach (var hole in Edges.Where(edge => edge.IsHole))
		{
			foreach (var head in other.Edges.Where(edge => edge.IsHead))
			{
				var value = hole.Test(head);
				tests.Add(((int)value, hole, head));
			}
		}
		foreach (var head in Edges.Where(edge => edge.IsHead))
		{
			foreach (var hole in other.Edges.Where(edge => edge.IsHole))
			{
				var value = head.Test(hole);
				tests.Add(((int)value, head, hole));
			}
		}
		var min = tests.Select(x => x.Item1).Min();
		var edge1 = tests.First(x => x.Item1 == min).Item2;
		var edge2 = tests.First(x => x.Item1 == min).Item3;
		var success = min < threshold;
		return (success, min, edge1, edge2);
	}
}

public enum EdgeType
{
	Hole, Head, Line,
}

public class Edge
{
	public PointF[] OriginPoints;
	public PointF OriginCorner1;
	public PointF OriginCorner2;
	
	public PointF[] NormalizedPoints;
	public PointF NormalizedCorner1;
	public PointF NormalizedCorner2;
	
	public EdgeType Type;
	public Edge(IEnumerable<PointF> points, PointF corner1, PointF corner2)
	{
		OriginPoints = points.ToArray();
		
		var normalized = Normalize(OriginPoints, corner1, corner2);
		NormalizedPoints = normalized.Points;
		NormalizedCorner1 = normalized.Corner1;
		NormalizedCorner2 = normalized.Corner2;
		
		Type = GetType(NormalizedPoints);
		if (IsHole)
		{
			NormalizedPoints = Reverse(NormalizedPoints, NormalizedCorner1, NormalizedCorner2);
		}
	}
	public Edge(IEnumerable<Point> points, Point corner1, Point corner2)
		:this (points.Select(p => (PointF)p), corner1, corner2)
	{
	}

	public bool IsHead => Type == EdgeType.Head;
	public bool IsHole => Type == EdgeType.Hole;
	public bool IsLine => Type == EdgeType.Line;

	private static EdgeType GetType(PointF[] normalizedPoint)
	{
		var maxY = Math.Abs(normalizedPoint.Max(p => p.Y));
		var minY = Math.Abs(normalizedPoint.Min(p => p.Y));

		if (maxY < 30 && minY < 30)
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
		//if (Math.Abs(NormalizedCorner2.X - other.NormalizedCorner2.X) > 5)
		//	return 9999;
			
		double distanceSum = NormalizedPoints
			.Sum(thisPoint => other.NormalizedPoints.Min(otherPoint => Distance(thisPoint, otherPoint)));

		return distanceSum;
	}
	
	public static (PointF[] Points, PointF Corner1, PointF Corner2) Normalize(IEnumerable<PointF> points, PointF corner1, PointF corner2)
	{
		var angle = CalculateAngleBetweenPoints(corner1, corner2);
		var result = points
			.Select(point => new PointF(point.X - corner1.X, point.Y - corner1.Y))
			.Select(point => RotatePointAroundOrigin(point, -angle))
			.ToArray();
		var newCorner2 = RotatePointAroundOrigin(new PointF(corner2.X - corner1.X, corner2.Y - corner1.Y), -angle);
		return (result, new PointF(0, 0), newCorner2);
	}

	public static PointF[] Reverse(PointF[] points, PointF corner1, PointF corner2)
	{
		var angle = CalculateAngleBetweenPoints(corner2, corner1);
		var reversed = points
			.Select(p => new PointF(p.X - corner2.X, p.Y - corner2.Y))
			.Select(p => RotatePointAroundOrigin(p, -angle))
			.ToArray();
		return reversed;
	}

	public void NormalizedPointPrintTo(Image<Bgr, byte> board, Point basePoint, Bgr color)
	{
		foreach (var point in NormalizedPoints)
		{
			var x = point.X + basePoint.X;
			var y = point.Y + basePoint.Y;
			var newPoint = new Point((int)x, (int)y);

			// CvInvoke.Circle(board, , 1, color, -1);
			board[newPoint] = color; 
		}
	}
	
	public void OriginPointPrintTo(Image<Bgr, byte> image, Bgr color)
	{
		foreach (var point in OriginPoints)
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

		var d = new Emgu.CV.Features2D.GFTTDetector(4, 0.01, 200, 9);
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
	
	//public static int IndexOf<T>(this T[] source, T value)
	//{
	//	for (var i = 0; i < source.Length; i++)
	//	{
	//		if (source.Equals(value))
	//			return i;
	//	}
	//	return -1;
	//}
	
	public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
	{
		var found = source.Select((x, i) => new { x, i })
			.FirstOrDefault(x => predicate(x.x));
		return found?.i ?? -1;
	}
}

























