using JkwExtensions;
using PictureToData;

namespace MainCli;

internal class DebugCorner
{
    public async Task Run()
    {
        var imageDir = @"../../../../../puzzle-test/1_resize";
        var targetDir = @"../../../../output/debug-corner";

        var files = Directory.GetFiles(imageDir);

        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        var processor = new SinglePieceImageProcessor();

        var infos = await files
            .Select(async file =>
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var outputPath = Path.Join(targetDir, $"{fileName}.png");

                    await processor.DebugAsync(file, outputPath);
                    return true;
                }
                catch (Exception ex)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    Console.WriteLine(ex.Message + " " + fileName);
                    return false;
                }
            })
            .WhenAll();

    }
}
