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
</Query>

void Main()
{
	var init = CvInvoke.Init();

	var fileName1 = "20230804_225655_3.jpg";
	Test(fileName1);
	var fileName2 = "20230806_161402_3.jpg";
	Test(fileName2);
	CvInvoke.WaitKey();
	CvInvoke.DestroyAllWindows();
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
		var point = new Point((int)corner.Point.X, (int)corner.Point.Y);
		CvInvoke.Circle(outline, point, 5, new MCvScalar(255, 0, 255), -1);
	}

	var edges = new List<List<Point>>
	{
		new List<Point>(),
		new List<Point>(),
		new List<Point>(),
		new List<Point>(),
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
			CvInvoke.Circle(outline, point, 3, color, -1);
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

	CvInvoke.Imshow(fileName, outline);
}

static double CalculateAngleBetweenPoints(PointF point1, PointF point2)
{
	double deltaX = point2.X - point1.X;
	double deltaY = point2.Y - point1.Y;

	// 아크탄젠트 함수를 사용하여 각도를 계산합니다.
	double angleInRadians = Math.Atan2(deltaY, deltaX);
	
	return angleInRadians;
}


static double CalculateDistance(Point p1, PointF p2, PointF p3) {
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



























