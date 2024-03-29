﻿using Common.PieceInfo;
using Emgu.CV;
using System.Drawing;

namespace PictureToData;

public interface ISinglePieceImageProcessor
{
    Task<PieceInfo> MakePieceInfoAsync(string imagePath, CornerDetectArgument argument);
    Task<PieceInfo> MakePieceInfoWithPredefinedCornerAsync(string imagePath, PointF[] predefinedCorner);
    Task DebugAsync(string imagePath, string outputPath);
}

public class SinglePieceImageProcessor : ISinglePieceImageProcessor
{
    public async Task<PieceInfo> MakePieceInfoAsync(string imagePath, CornerDetectArgument argument)
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

        var cornerDetector = new CornerDetector(outline, argument);
        var corners = cornerDetector.Process();

        var piece = new Piece(outline, corners);

        return new PieceInfo
        {
            Name = Path.GetFileNameWithoutExtension(imagePath),
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

    public async Task<PieceInfo> MakePieceInfoWithPredefinedCornerAsync(string imagePath, PointF[] predefinedCorner)
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
        var corners = cornerDetector.ProcessWithPredefinedCorner(predefinedCorner);

        var piece = new Piece(outline, corners);

        return new PieceInfo
        {
            Name = Path.GetFileNameWithoutExtension(imagePath),
            //Outline = outline.GetContour(),
            Corners = corners,
            PredefinedCorners = predefinedCorner,
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

    public async Task DebugAsync(string imagePath, string outputPath)
    {
        var outline = new Outline(imagePath);
        await outline.ProcessAsync();

        var cornerDetector = new CornerDetector(outline);

        var outlineImage = outline.GetImage();
        cornerDetector.WriteTo(outlineImage, 10);

        CvInvoke.Imwrite(outputPath, outlineImage);
    }
}

