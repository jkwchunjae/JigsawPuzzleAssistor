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

	var sourceDir = @"D:\puzzle\0_source";
	
	var pairs = Directory.GetFiles(sourceDir)
		.Select(source => new 
		{
			SourcePath = source,
			TargetPath = source.Replace("0_source", "1_resize"),
		})
		.ToList();
	await Parallel.ForEachAsync(pairs, async (pair, token) =>
	{
		await ResizeAsync(pair.SourcePath, pair.TargetPath);
	});
}

static Task ResizeAsync(string sourcePath, string targetPath)
{
	return Task.Run(() =>
	{
		Mat image = CvInvoke.Imread(sourcePath);
		var ratio = 1.0;
		Image<Gray, byte> resized = new Image<Gray, byte>(image.Width, image.Height);
		CvInvoke.Resize(image, resized, new Size(0, 0), ratio, ratio, Inter.Area);
		var roiRectangle = new Rectangle(200, 400, 700, 700);
		var cropped = new Mat(resized.Mat, roiRectangle);

		CvInvoke.Imwrite(targetPath, cropped);
	});
}





















