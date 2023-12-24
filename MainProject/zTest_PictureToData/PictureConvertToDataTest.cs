using PictureToData;

namespace zTest_PictureToData;

public class PictureConvertToDataTest
{
    //[Fact(Skip = "test only local")]
    [Fact]
    public async Task Test1()
    {
        var imagePath = @"../../../../../puzzle-test/1_resize/puzzle_00001.jpg";

        Assert.True(File.Exists(imagePath));
        //for (var i = 1; i < 10; i++)
        //{
        //    imagePath = "../" + imagePath;
        //    if (File.Exists(imagePath))
        //    {
        //        Assert.Equal(-1, i);
        //        Assert.Equal(string.Empty, imagePath);
        //    }
        //}

        var processor = new SinglePieceImageProcessor();
        var pieceInfo = await processor.MakePieceInfoAsync(imagePath);

        Assert.Equal(4, pieceInfo.Corners.Length);
    }
}