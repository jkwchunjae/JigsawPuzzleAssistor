using JkwExtensions;
using PuzzleCropper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainCli;

internal class PuzzleCrop : IMainRunner
{
    public async Task Run()
    {
        var inputDir = @"D:\puzzle\0_source";
        var outputDir = @"D:\puzzle\test_resize";

        var cropper = new Cropper();

        await Directory.GetFiles(inputDir)
            .Select(input =>
            {
                var fileName = Path.GetFileName(input);
                var outputPath = Path.Join(outputDir, fileName);
                var initRoi = new Rectangle(200, 400, 700, 700);
                return cropper.CropUsingOutline(input, outputPath, initRoi);
            })
            .WhenAll();
    }
}
