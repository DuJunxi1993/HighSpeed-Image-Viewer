namespace HighSpeedImageViewer.Models;

public class ThumbnailEntry
{
    public string FilePath { get; init; } = "";
    public byte[] Data { get; init; } = [];
    public int Width { get; init; }
    public int Height { get; init; }
    public long LastModifiedTicks { get; init; }
}