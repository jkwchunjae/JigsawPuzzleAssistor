using Common.PieceInfo;
using Emgu.CV;

namespace PictureToData;

public interface ISinglePieceImageProcessor
{
    Task<PieceInfo> MakePieceInfoAsync(string imagePath);
}

public class SinglePieceImageProcessor : ISinglePieceImageProcessor
{
    public async Task<PieceInfo> MakePieceInfoAsync(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException($"File not found: {imagePath}");
        }
        if (!CvInvoke.Init())
        {
            throw new Exception("Unable to initialize CvInvoke");
        }

        var outline = new Outline(imagePath);
        await outline.ProcessAsync();

        var cornerDetector = new CornerDetector(outline);
        var corners = cornerDetector.GetCorners();

        var piece = new Piece(outline, corners);

        return new PieceInfo
        {
            //Outline = outline.GetContour(),
            Corners = corners,
            Edges = piece.Edges.Select(edge => new EdgeInfo
            {
                Type = edge.Type,
                //OriginPoints = edge.OriginPoints,
                //OriginCorner1 = edge.OriginCorner1,
                //OriginCorner2 = edge.OriginCorner2,
                NormalizedPoints = edge.NormalizedPoints,
                //NormalizedCorner1 = edge.NormalizedCorner1,
                //NormalizedCorner2 = edge.NormalizedCorner2,
                Length = edge.NormalizedCorner2.X,
            }).ToList(),
        };
    }
}

