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
</Query>

void Main()
{
	var init = CvInvoke.Init();

	var fileName1 = "resize_20230804_225655.jpg";
	var fileName2 = "resize_20230804_225715.jpg";
	var fileName3 = "resize_20230804_225725.jpg";

	var piece1 = new Piece(fileName1);

	Image<Bgr, byte> board = new Image<Emgu.CV.Structure.Bgr, byte>(1000, 1000);

	var hole = piece1.Edges.First(e => e.IsHole);
	hole.PrintTo(board, new Point(300, 200), new MCvScalar(255, 255, 255));

	CvInvoke.Imshow("board", board);
	CvInvoke.WaitKey();
	CvInvoke.DestroyAllWindows();
}

public class Piece
{
	public string Name;
	public List<Edge> Edges = new();
	
	public Piece(string fileName)
	{
		Name = fileName;
		
		var imagePath = Path.Join(Directory.GetParent(Util.CurrentQueryPath).FullName, fileName);
		Mat image = CvInvoke.Imread(imagePath);
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
		CvInvoke.FindContours(cannyResult, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxSimple);

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

		Image<Bgr, byte> outline = new Image<Bgr, byte>(image.Width, image.Height, new Bgr(0, 0, 0));
		CvInvoke.DrawContours(outline, contours, puzzleContoursIndex, new MCvScalar(0, 255, 0), 1);

		var d = new Emgu.CV.Features2D.GFTTDetector(4, 0.01, 200, 5);
		var corners = d.Detect(outline);

		// 점을 시계방향으로 정렬
		var baseCorner = corners.OrderBy(c => c.Point.Y).First();
		var otherCorners = corners.OrderBy(c => c.Point.Y).Skip(1).ToArray();
		otherCorners = otherCorners
			.Select(c => new { Corner = c, Angle = CalculateAngleBetweenPoints(baseCorner.Point, c.Point) })
			.OrderBy(c => c.Angle)
			.Select(x => x.Corner)
			.ToArray();
		corners = new[] { baseCorner }.Concat(otherCorners).ToArray();

		foreach (var corner in corners)
		{
			CvInvoke.Circle(outline, corner.Point.ToPoint(), 5, new MCvScalar(255, 0, 255), -1);
		}

		var edges = new List<List<PointF>>
		{
			new List<PointF>(),
			new List<PointF>(),
			new List<PointF>(),
			new List<PointF>(),
		};

		var colors = new MCvScalar[]
		{
			new MCvScalar(0, 0, 255),
			new MCvScalar(0, 255, 0),
			new MCvScalar(255, 0, 255),
			new MCvScalar(255, 255, 0),
		};

		int prevIndex = 0;
		for (var i = 0; i < puzzleContours.Size; i++)
		{
			var point = puzzleContours[i];

			//CvInvoke.Circle(outline, point, 3, new MCvScalar(255, 255, 255), -1);
			//CvInvoke.Imshow("Puzzle Area", outline);
			//CvInvoke.WaitKey();

			var d01 = CalculateDistance(point, corners[0].Point, corners[1].Point);
			var d12 = CalculateDistance(point, corners[1].Point, corners[2].Point);
			var d23 = CalculateDistance(point, corners[2].Point, corners[3].Point);
			var d30 = CalculateDistance(point, corners[3].Point, corners[0].Point);
			var dddd = new[] { d01, d12, d23, d30 };

			var minDistance = dddd.Min();
			var minIndex = dddd.Select((d, i) => new { D = d, I = i })
				.First(x => x.D == minDistance)
				.I;

			if (minDistance < 10)
			{
				edges[minIndex].Add(point);
				prevIndex = minIndex;
			}
			else
			{
				edges[prevIndex].Add(point);
			}
		}
		var edgeCornerSet = new Tuple<PointF, PointF>[]
		{
			new Tuple<PointF, PointF>(corners[0].Point, corners[1].Point),
			new Tuple<PointF, PointF>(corners[1].Point, corners[2].Point),
			new Tuple<PointF, PointF>(corners[2].Point, corners[3].Point),
			new Tuple<PointF, PointF>(corners[3].Point, corners[0].Point),
		};

		// normalize
		for (var i = 0; i < edges.Count; i++)
		{
			var edge = edges[i];
			var color = colors[i];
			var cornerSet = edgeCornerSet[i];

			var angle = CalculateAngleBetweenPoints(cornerSet.Item1, cornerSet.Item2);
			
			var points = new List<PointF>();
			var newEdges = new[] { cornerSet.Item1 }
				.Concat(edge)
				.Concat(new[] { cornerSet.Item2 });
			foreach (var point in edge)
			{
				float x = point.X;
				float y = point.Y;
				x -= cornerSet.Item1.X;
				y -= cornerSet.Item1.Y;
				var rotatedPoint = RotatePointAroundOrigin(new PointF(x, y), -angle);
				points.Add(new Point((int)rotatedPoint.X, (int)rotatedPoint.Y));
			}

			Edges.Add(new Edge(points));
		}

		// CvInvoke.Imshow(fileName, outline);
	}

	public bool Test(Piece other)
	{
		foreach (var edge in Edges)
		{
			if (edge.Type == EdgeType.Hole)
			{
				var otherHeads = other.Edges.Where(e => e.Type == EdgeType.Head);
				foreach (var otherHead in otherHeads)
				{
					// edge.Test(otherHead).Dump($"{Name} Hole,{other.Name} Head");
				}
			}

			if (edge.Type == EdgeType.Head)
			{
				var otherHoles = other.Edges.Where(e => e.Type == EdgeType.Hole);
				foreach (var otherHole in otherHoles)
				{
					// edge.Test(otherHole).Dump($"{Name} Head,{other.Name} Hole");
				}
			}
		}
		return true;
	}
}

public enum EdgeType
{
	Hole,
	Head,
	Line,
}

public class Edge
{
	public List<PointF> Points;
	public EdgeType Type;
	public Edge(List<PointF> normalizedPoint)
	{
		Type = GetType(normalizedPoint);
		var interpolated = Interpolate(normalizedPoint);
		if (IsHole)
		{
			Points = Reverse(interpolated);
		}
		else
		{
			Points = interpolated;
		}
	}
	
	public bool IsHead => Type == EdgeType.Head;
	public bool IsHole => Type == EdgeType.Hole;
	public bool IsLine => Type == EdgeType.Line;
	
	private static EdgeType GetType(List<PointF> normalizedPoint)
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
		double distanceSum = Points
			.Sum(thisPoint => other.Points.Min(otherPoint => Distance(thisPoint, otherPoint)));
		
		return distanceSum;
	}
	
	public static List<PointF> Reverse(List<PointF> points)
	{
		PointF first = points.OrderBy(p => p.X).First();
		PointF last = points.OrderBy(p => p.X).Last();

		var angle = CalculateAngleBetweenPoints(last, first);
		var reversed = points
			.Select(p => new PointF(p.X - last.X, p.Y - last.Y))
			.Select(p => RotatePointAroundOrigin(p, -angle))
			.ToList();
		return reversed;
	}
	
	public static List<PointF> Interpolate(List<PointF> points)
	{
		//var diffs = points
		//	.Take(points.Count() - 1)
		//	.Select((p, i) => new
		//	{
		//		Curr = p,
		//		Next = points[i + 1],
		//		Distance = Distance(p, points[i + 1]),
		//	})
		//	.Select(x => new
		//	{
		//		x.Curr,
		//		x.Next,
		//		x.Distance,
		//		Interpolated = x.Distance > 5 ? Interpolate(x.Curr, x.Next, 1.8) : null,
		//	})
		//	.Dump();
		var result = new List<PointF>();
		for (var i = 0; i < points.Count() - 1; i++)
		{
			var curr = points[i];
			var next = points[i + 1];
			var distance = Distance(curr, next);
			if (distance < 5)
			{
				result.Add(curr);
			}
			else
			{
				result.AddRange(Interpolate(curr, next, 1.8));
			}
		}
		result.Add(points.Last());
		return result;
	}

	static List<PointF> Interpolate(PointF start, PointF end, double interval)
	{
		List<PointF> interpolatedPoints = new List<PointF>();

		// 두 점 사이의 거리와 방향 벡터 계산
		double distance = Math.Sqrt(Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2));
		double directionX = (end.X - start.X) / distance;
		double directionY = (end.Y - start.Y) / distance;

		// 보간점 계산 및 추가
		for (double t = 0; t < 1; t += interval / distance)
		{
			float interpolatedX = (float)(start.X + directionX * distance * t);
			float interpolatedY = (float)(start.Y + directionY * distance * t);
			interpolatedPoints.Add(new PointF(interpolatedX, interpolatedY));
		}

		return interpolatedPoints;
	}

	public void PrintTo(Image<Bgr, byte> board, PointF basePoint, MCvScalar color)
	{
		foreach (var point in Points)
		{
			var x = point.X + basePoint.X;
			var y = point.Y + basePoint.Y;
			
			CvInvoke.Circle(board, new Point((int)x, (int)y), 1, color, -1);
		}
	}
}

static void Test(string fileName)
{

	var imagePath = Path.Join(Directory.GetParent(Util.CurrentQueryPath).FullName, fileName);
	Mat image = CvInvoke.Imread(imagePath);
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
	CvInvoke.FindContours(cannyResult, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxSimple);

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

	Image<Bgr, byte> outline = new Image<Bgr, byte>(image.Width, image.Height, new Bgr(0, 0, 0));
	CvInvoke.DrawContours(outline, contours, puzzleContoursIndex, new MCvScalar(0, 255, 0), 1);

	var d = new Emgu.CV.Features2D.GFTTDetector(4, 0.01, 100, 5);
	var corners = d.Detect(outline);

	// 점을 시계방향으로 정렬
	var baseCorner = corners.OrderBy(c => c.Point.Y).First();
	var otherCorners = corners.OrderBy(c => c.Point.Y).Skip(1).ToArray();
	otherCorners = otherCorners
		.Select(c => new { Corner = c, Angle = CalculateAngleBetweenPoints(baseCorner.Point, c.Point) })
		.OrderBy(c => c.Angle)
		.Select(x => x.Corner)
		.ToArray();
	corners = new[] { baseCorner }.Concat(otherCorners).ToArray();

	foreach (var corner in corners)
	{
		CvInvoke.Circle(outline, corner.Point.ToPoint(), 5, new MCvScalar(255, 0, 255), -1);
	}

	var edges = new List<List<PointF>>
	{
		new List<PointF>(),
		new List<PointF>(),
		new List<PointF>(),
		new List<PointF>(),
	};

	var colors = new MCvScalar[]
	{
		new MCvScalar(0, 0, 255),
		new MCvScalar(0, 255, 0),
		new MCvScalar(255, 0, 255),
		new MCvScalar(255, 255, 0),
	};

	int prevIndex = 0;
	for (var i = 0; i < puzzleContours.Size; i++)
	{
		var point = puzzleContours[i];

		CvInvoke.Circle(outline, point, 3, new MCvScalar(255, 255, 255), -1);
		//CvInvoke.Imshow("Puzzle Area", outline);
		//CvInvoke.WaitKey();

		var d01 = CalculateDistance(point, corners[0].Point, corners[1].Point);
		var d12 = CalculateDistance(point, corners[1].Point, corners[2].Point);
		var d23 = CalculateDistance(point, corners[2].Point, corners[3].Point);
		var d30 = CalculateDistance(point, corners[3].Point, corners[0].Point);
		var dddd = new[] { d01, d12, d23, d30 };

		var minDistance = dddd.Min();
		var minIndex = dddd.Select((d, i) => new { D = d, I = i })
			.First(x => x.D == minDistance)
			.I;

		if (minDistance < 10)
		{
			edges[minIndex].Add(point);
			prevIndex = minIndex;
		}
		else
		{
			edges[prevIndex].Add(point);
		}
	}
	var edgeCornerSet = new Tuple<MKeyPoint, MKeyPoint>[]
	{
		new Tuple<MKeyPoint, MKeyPoint>(corners[0], corners[1]),
		new Tuple<MKeyPoint, MKeyPoint>(corners[1], corners[2]),
		new Tuple<MKeyPoint, MKeyPoint>(corners[2], corners[3]),
		new Tuple<MKeyPoint, MKeyPoint>(corners[3], corners[0]),
	};
	var yyyy = new int[] { 100, 200, 300, 400 };

	foreach (var x in edges.Zip(colors))
	{
		var edge = x.First;
		var color = x.Second;
		foreach (var point in edge)
		{
			CvInvoke.Circle(outline, point.ToPoint(), 3, color, -1);
			//CvInvoke.Imshow("Puzzle Area", outline);
			//CvInvoke.WaitKey();
		}
	}

	// normalize
	for (var i = 0; i < edges.Count; i++)
	{
		var edge = edges[i];
		var color = colors[i];
		var cornerSet = edgeCornerSet[i];
		var baseX = 50;
		var baseY = yyyy[i];

		CvInvoke.Line(outline, new Point(50, baseY), new Point(250, baseY), new MCvScalar(255, 255, 255), 1);
		var angle = CalculateAngleBetweenPoints(cornerSet.Item1.Point, cornerSet.Item2.Point);
		foreach (var point in edge)
		{
			float x = point.X;
			float y = point.Y;
			x -= cornerSet.Item1.Point.X;
			y -= cornerSet.Item1.Point.Y;
			var rotatedPoint = RotatePointAroundOrigin(new PointF(x, y), -angle + Math.PI * 2);
			var circlePoint = new Point(
				x: (int)rotatedPoint.X + baseX,
				y: (int)rotatedPoint.Y + baseY
			);
			CvInvoke.Circle(outline, circlePoint, 3, color, -1);
		}
	}

	// CvInvoke.Imshow(fileName, outline);
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
}

























