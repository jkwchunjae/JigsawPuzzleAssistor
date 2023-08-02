<Query Kind="Program">
  <NuGetReference>Emgu.CV</NuGetReference>
  <NuGetReference>Emgu.CV.runtime.windows</NuGetReference>
  <Namespace>System.Net</Namespace>
  <Namespace>Emgu.CV</Namespace>
  <Namespace>Emgu.CV.Structure</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>Emgu.CV.CvEnum</Namespace>
  <Namespace>Emgu.CV.Features2D</Namespace>
</Query>

void Main()
{
	var init = CvInvoke.Init();
	
	var currentDir = Directory.GetParent(Util.CurrentQueryPath).FullName;
	var sourcePath = Path.Combine(currentDir, @"Pieces_first.png");
	using (Image<Rgba, byte> sourceImg = new Image<Rgba, byte>(sourcePath))
	{
		CvInvoke.MedianBlur(sourceImg, sourceImg, 5);
		
		using (Image<Gray, byte> mask = GetMask(sourceImg))
		{
			var maskPath = Path.Combine(currentDir, "mask.png");
			mask.Save(maskPath);
			SimpleBlobDetector blobDetector = new SimpleBlobDetector();
			MKeyPoint[] keypoints = blobDetector.Detect(sourceImg, mask);
			foreach (MKeyPoint keyPoint in keypoints)
			{
				
			}
			keypoints.Dump();
		}
	}
}

public Image<Gray, byte> GetMask(Image<Rgba, byte> inputImage)
{
	// https://stackoverflow.com/questions/60861812/how-to-set-mask-to-grabcut-method-in-emgu-cv-c
	// https://docs.opencv.org/3.4/d8/d83/tutorial_py_grabcut.html
	Image<Rgb, byte> tmpInputImage = inputImage.Convert<Rgb, byte>();
	Rectangle roi = new Rectangle(1, 1, tmpInputImage.Width - 1, tmpInputImage.Height - 1);
	Image<Gray, byte> mask = new Image<Gray, byte>(tmpInputImage.Width, tmpInputImage.Height);
	mask.SetZero();
	
	CvInvoke.Rectangle(mask, roi, new MCvScalar(255), -1);

	Image<Bgr, byte> segmentedResult = new Image<Bgr, byte>(tmpInputImage.Width, tmpInputImage.Height);

	// Apply GrabCut algorithm
	Matrix<double> bg = new Matrix<double>(1, 65);
	bg.SetZero();
	Matrix<double> fg = new Matrix<double>(1, 65);
	fg.SetZero();
	CvInvoke.GrabCut(tmpInputImage, mask, roi, bg, fg, 2, GrabcutInitType.InitWithRect);

	for (int x = 0; x < mask.Cols; x++)
	{
		for (int y = 0; y < mask.Rows; y++)
		{
			if (mask[y, x].Intensity == new Gray(1).Intensity || mask[y, x].Intensity == new Gray(3).Intensity)
			{
				mask[y, x] = new Gray(1);
			}
			else
			{
				mask[y, x] = new Gray(0);
			}
		}

	}

	tmpInputImage = tmpInputImage.Mul(mask.Convert<Bgr, byte>());
	return mask;
//	using (var grabCut = new Emgu.CV.Image.gr.ImageGraphCut<byte>())
//	{
//		grabCut.MaxIterations = 10; // Number of iterations
//		grabCut.SegmentationModel = ImageGraphCut<byte>.GraphCutModel.GC_WITH_RECT;
//
//		grabCut.ProcessImage(inputImage, mask);
//		grabCut.PostProcess(inputImage, segmentedResult);
//	}
//
//
//	using Image<Rgba, byte> tmpInputImg = inputImage.LimitImageSize(10000, 10000);
//	mask = tmpInputImg.Convert<Rgb, byte>().GrabCut(new Rectangle(1, 1, tmpInputImg.Width - 1, tmpInputImg.Height - 1), 1);
//	// tmpInputImg.Dispose();
//	mask = mask.ThresholdBinary(new Gray(2), new Gray(255));            // Change the mask. All values bigger than 2 get mapped to 255. All values equal or smaller than 2 get mapped to 0.
//	return mask;
}

public static class Utils
{
	public static Image<TColor, TDepth> LimitImageSize<TColor, TDepth>(this Image<TColor, TDepth> img, int maxWidth, int maxHeight) where TColor : struct, IColor where TDepth : new()
	{
		if (img.Width > maxWidth || img.Height > maxHeight) { return img.Resize(maxWidth, maxHeight, Inter.Area, true); }
		else { return img; }
	}
}