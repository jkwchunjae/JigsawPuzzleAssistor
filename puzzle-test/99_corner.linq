<Query Kind="Program">
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
			TargetPath = source.Replace("1_resize", "3_corner"),
		})
		.ToList();

	foreach (var pair in pairs) //.Where(p => p.SourcePath.Contains("00010"))) //.Take(2))
	{
		var outline = new Outline(pair.SourcePath);
		await outline.ProcessAsync();

		var outlineImage = outline.GetImage();
		var corner = new Corner(outline);
		var corners = corner.GetCorners();
		var empty = new Image<Bgr, byte>(outlineImage.Width, outlineImage.Height, new Bgr(0, 0, 0));
		outline.WriteTo(empty, new Bgr(Color.Green));
		if (corners.Count() == 4)
		{
			corner.NewCorner3(corners[0], empty);
			corner.NewCorner3(corners[1], empty);
			corner.NewCorner3(corners[2], empty);
			corner.NewCorner3(corners[3], empty);
		}
		//corner.WriteTo(empty, 3, new Bgr(Color.AliceBlue));
		CvInvoke.Imwrite(pair.TargetPath, empty);
	}
}

public class Corner
{
	public string Name => Outline.Name;
	public Outline Outline { get; init; }
	private Point[] Corners { get; set; }
	
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
		
		Corners = orderedCorner.Select(c => c.Point.ToPoint()).ToArray();
	}

	public Point NewCorner1(Point corner, Image<Bgr, byte> image)
	{
		image[corner] = new Bgr(Color.Green);
		
		var near = Outline.GetContour()
			.Where(c => Distance(corner, c) < 40)
			.ToArray();
			
		foreach (var point in near)
		{
			image[point] = new Bgr(Color.White);
		}
		var minPoint = new Point(near.Min(p => p.X) - 10, near.Min(p => p.Y) - 10);
		var maxPoint = new Point(near.Max(p => p.X) + 10, near.Max(p => p.Y) + 10);
		var dic = new Dictionary<Point, int>
		{
			[minPoint] = 0,
		};
		var aaa = new List<(double Angle, Point P1, Point P2)>();
		foreach (PointF p1 in near)
		{
			foreach (PointF p2 in near.Where(p2 => p2 != p1))
			{
				if (p1.X == p2.X)
				{
					aaa.Add((99999, p1.ToPoint(), p2.ToPoint()));
					for (var y = minPoint.Y; y <= maxPoint.Y; y++)
					{
						var newP = new Point((int)p1.X, y);
						if (!near.Contains(newP))
						{
							image[newP] = new Bgr(0, 0, image[newP].Red + 1);
						}
						if (dic.ContainsKey(newP))
						{
							dic[newP] = dic[newP] + 1;
						}
						else
						{
							dic[newP] = 1;
						}
					}
				}
				else
				{
					double m = (p2.Y - p1.Y) / (p2.X - p1.X);  // 기울기 계산
					aaa.Add((m, p1.ToPoint(), p2.ToPoint()));
					double b = p1.Y - m * p1.X;            // 절편 계산
					for (var x = minPoint.X; x <= maxPoint.X; x++)
					{
						double y = m * x + b;
						var newP = new Point(x, (int)y);
						if (y >= minPoint.Y && y <= maxPoint.Y)
						{
							if (!near.Contains(newP))
							{
								image[newP] = new Bgr(0, 0, image[newP].Red + 1);
							}
							var ppp = newP;
							if (dic.ContainsKey(ppp))
							{
								dic[ppp] = dic[ppp] + 1;
							}
							else
							{
								dic[ppp] = 1;
							}
						}
					}
				}
			}
		}
		var newCorner = minPoint;
		for (var x = minPoint.X; x <= maxPoint.X; x++)
		{
			for (var y = minPoint.Y; y <= maxPoint.Y; y++)
			{
				var point = new Point(x, y);
				if (dic.ContainsKey(newCorner) && dic.ContainsKey(point))
				{
					if (dic[newCorner] < dic[point])
						newCorner = point;
				}
			}
		}
		dic[newCorner].Dump();
		image[newCorner].Red.Dump();
		//newCorner.Dump();
		// dic.OrderByDescending(x => x.Value).Take(10).Dump();
		image[newCorner] = new Bgr(Color.Yellow);
		aaa.Select(x => new { Angle = (int)(x.Angle * 100), x.P1, x.P2 })
			.GroupBy(x => x.Angle)
			.Select(x => new { A = x.Key / 100.0, Count = x.Count(), List = x.ToArray() })
			.OrderByDescending(x => x.Count)
			.Dump(1);
		return corner;
	}
	
	public Point NewCorner2(Point corner, Image<Bgr, byte> image)
	{
		image[corner] = new Bgr(Color.Green);

		var near = Outline.GetContour()
			.Where(c => Distance(corner, c) < 40)
			.Select((c, i) => new
			{
				Point = c,
				Index = i,
				DistanceFromCorner = Distance(corner, c),
			})
			.ToArray();

		foreach (var point in near)
		{
			image[point.Point] = new Bgr(Color.White);
		}
		var minPoint = new Point(near.Min(p => p.Point.X) - 10, near.Min(p => p.Point.Y) - 10);
		var maxPoint = new Point(near.Max(p => p.Point.X) + 10, near.Max(p => p.Point.Y) + 10);

		var aaa = new List<(double Angle, PointF P1, PointF P2, double Dist)>();
		foreach (var xxx1 in near.Where(x => x.DistanceFromCorner > 20))
		{
			var p1 = xxx1.Point;
			foreach (var xxx2 in near.Where(p2 => p2.Point != p1))
			{
				var p2 = xxx2.Point;
				var d = Distance(p1, p2);
				if (d >= 10 && d <= 40)
				{
					if (p1.X == p2.X)
					{
						aaa.Add((99999, p1, p2, Distance(p1, p2)));
					}
					else
					{
						double m = (p2.Y - p1.Y) / (p2.X - p1.X);  // 기울기 계산
						aaa.Add((m, p1, p2, Distance(p1, p2)));
					}
				}
			}
		}

		var aResult = aaa.Select(x => new { Angle = (int)(x.Angle * 100), x.P1, x.P2, x.Dist })
			.GroupBy(x => x.Angle)
			.Select(x => new { A = x.Key / 100.0, Weight = x.Sum(e => e.Dist), List = x.ToArray() })
			.OrderByDescending(x => x.Weight)
			//.Take(8)
			//.Dump(1)
			.ToArray();

		var dic = new Dictionary<Point, double>
		{
			[minPoint] = 0,
		};
		Action<Point, double> action = new Action<Point, double>((point, distance) =>
		{
			if (point.X >= minPoint.X && point.X <= maxPoint.X)
			if (point.Y >= minPoint.Y && point.Y <= maxPoint.Y)
			
			if (!near.Any(x => x.Point == point))
			{
				image[point] = new Bgr(0, 0, image[point].Red + 3);
			}
			if (dic.ContainsKey(point))
			{
				dic[point] = dic[point] + distance;
			}
			else
			{
				dic[point] = distance;
			}
		});
		foreach (var aaaa in aResult)
		{
			//var m = aaaa.A;
			foreach (var list in aaaa.List)
			{
				var p1 = list.P1;
				var p2 = list.P2;
				var distance = list.Dist;


				if (p1.X == p2.X)
				{
					for (var y = minPoint.Y; y <= maxPoint.Y; y++)
					{
						var newP = new Point((int)p1.X, y);
						action(newP, distance);
					}
				}
				else
				{
					double m = (p2.Y - p1.Y) / (p2.X - p1.X);  // 기울기 계산
					if (Math.Abs(m) > 1999999)
					{
						m = 1 / m;
						double b = p1.X - m * p1.Y; // x절편
						for (var y = minPoint.Y; y <= maxPoint.Y; y++)
						{
							double x = m * y + b;
							var newP = new Point((int)x, y);
							action(newP, distance);
						}
					}
					else
					{
						double b = p1.Y - m * p1.X;            // 절편 계산
						for (var x = minPoint.X; x <= maxPoint.X; x++)
						{
							double y = m * x + b;
							var newP = new Point(x, (int)y);
							action(newP, distance);
						}
					}
				}
			}
		}
		//dic.OrderByDescending(x => x.Value).Take(5).Dump();
		var newCorner = dic.OrderByDescending(x => x.Value).First().Key;
		CvInvoke.Circle(image, newCorner, 2, new MCvScalar(0, 255, 255), -1);
		image[newCorner] = new Bgr(Color.Yellow);
		return newCorner;
	}

	public Point NewCorner3(Point corner, Image<Bgr, byte> image)
	{
		CvInvoke.Circle(image, corner, 1, new MCvScalar(0, 255, 0), -1);

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

		CvInvoke.Circle(image, farPoint1, 2, new MCvScalar(255, 0, 0), -1);
		CvInvoke.Circle(image, farPoint2, 2, new MCvScalar(255, 255, 0), -1);
		
		var intersection = CalculateIntersection(line1, line2);
		CvInvoke.Circle(image, intersection.ToPoint(), 1, new MCvScalar(255, 0, 255), -1);
		
		return intersection.ToPoint();
		
		void DrawLine(LineData line)
		{
			if (line.horizotal)
			{
				var p1 = new Point((int)line.xValue, minPoint.Y);
				var p2 = new Point((int)line.xValue, maxPoint.Y);
				CvInvoke.Line(image, p1, p2, new MCvScalar(0, 255, 255), 1);
			}
			else
			{
				var p1 = new Point(minPoint.X, (int)(line.angle * minPoint.X + line.yValue));
				var p2 = new Point(maxPoint.X, (int)(line.angle * maxPoint.X + line.yValue));
				CvInvoke.Line(image, p1, p2, new MCvScalar(0, 255, 255), 1);
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
			var targets = new [] { firstTarget}.ToList();
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
				image[point.Point] = new Bgr(Color.Red);
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
	public Point[] GetCorners()
	{
		return Corners;
	}
	
	public void WriteTo(Image<Bgr, byte> image, int radius, Bgr color)
	{
		foreach (var corner in Corners)
		{
			//image[corner] = color;
			CvInvoke.Circle(image, corner, radius, new MCvScalar(color.Blue, color.Green, color.Red), -1);
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

			Contour = ContourWithoutItplt();
			_outlineImage = outline;

			Point[] ContourWithInterpolation()
			{
				var contour = new List<Point> { puzzleContours[0] };
				for (var i = 1; i < puzzleContours.Size; i++)
				{
					if (contour.Contains(puzzleContours[i]))
						continue;

					var dist = Distance(contour.Last(), puzzleContours[i]);
					if (dist > 3) // && dist < 70)
					{
						contour.AddRange(Interpolate(contour.Last(), puzzleContours[i], 2)
							.Select(p => p.ToPoint())
							.Distinct()
							);
					}
					else if (dist <= 3)
					{
						contour.Add(puzzleContours[i]);
					}
					else
					{
						$"({puzzleContours[i].X}, {puzzleContours[i].Y})".Dump();
					}
				}
				if (Distance(contour.First(), contour.Last()) > 5)
				{
					contour.AddRange(Interpolate(contour.Last(), contour.First(), 2)
						.Select(p => p.ToPoint())
						.Distinct());
				}
				return contour.ToArray();
			}
			Point[] ContourWithoutItplt()
			{
				var contour = new List<Point>();
				for (var i = 0; i < puzzleContours.Size; i++)
				{
					if (contour.Contains(puzzleContours[i]))
						continue;
						
					contour.Add(puzzleContours[i]);
				}
				return contour.ToArray();
			}
		});
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
	
	public void WriteTo(Image<Bgr, byte> image, Bgr color)
	{
		foreach (var point in Contour)
		{
			image[point] = color;
		}
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


static double CalculateDistance(PointF p1, PointF p2, PointF p3)
{
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

























