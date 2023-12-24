using Common.PieceInfo;
using JkwExtensions;
using PictureToData;
using System.Text.Json;

namespace MainCli;

internal class PieceInfoJson
{
    public async Task Run()
    {
        var imageDir = @"../../../../../puzzle-test/1_resize";
        var targetDir = @"../../../../output/piece-info";

        var files = Directory.GetFiles(imageDir);

        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        var processor = new SinglePieceImageProcessor();

        var serializeOption = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
    {
        new PointFJsonConverter(),
        new PointArrayJsonConverter(),
        new PointFArrayJsonConverter(),
    },
        };

        var infos = await files
            .Select(async file =>
            {
                try
                {
                    return await processor.MakePieceInfoAsync(file);
                }
                catch (Exception ex)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    Console.WriteLine(ex.Message + " " + fileName);
                    return null;
                }
            })
            .WhenAll();

        foreach (var (file, info) in files.Zip(infos))
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var targetPath = Path.Join(targetDir, $"{fileName}.json");
            //Console.WriteLine(fileName);
            if (info != null)
            {
                await File.WriteAllTextAsync(targetPath, JsonSerializer.Serialize(info, serializeOption));
            }
        }

    }
}
