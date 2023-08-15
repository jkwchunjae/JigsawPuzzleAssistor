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
			TargetPath = source.Replace("1_resize", "2_outline"),
		})
		.ToList();

	await Parallel.ForEachAsync(pairs, async (pair, token) =>
	{
		var outline = new Outline(pair.SourcePath);
		var outlineImage = await outline.GetOutline();
		CvInvoke.Imwrite(pair.TargetPath, outlineImage);
	});
}

public class Outline
{
	public string Name { get; init; }

	public VectorOfPoint Contour { get; private set; }
	private Image<Bgr, byte> _raw { get; init; }
	
	public Outline(string imagePath)
	{
		Name = Path.GetFileNameWithoutExtension(imagePath);
		Mat image = CvInvoke.Imread(imagePath);
		Image<Bgr, byte> rawImage = new Image<Bgr, byte>(image.Width, image.Height);
		image.CopyTo(rawImage);
		
		_raw = rawImage;
	}
	
	public Task<Image<Bgr, byte>> GetOutline()
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

			var outline = new Image<Bgr, byte>(image.Width, image.Height, new Bgr(0, 0, 0));
			CvInvoke.DrawContours(outline, contours, puzzleContoursIndex, new MCvScalar(0, 255, 0), 1);

			Contour = puzzleContours;
			return outline;
		});
	}
}

public static class Ex
{
	public static Point ToPoint(this PointF point)
	{
		return new Point((int)point.X, (int)point.Y);
	}
}

























