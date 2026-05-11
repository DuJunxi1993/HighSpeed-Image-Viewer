using System;
using System.IO;

namespace HighSpeedImageViewer.Models;

public class ImageItem
{
    public string FilePath { get; }
    public string FileName => Path.GetFileName(FilePath);
    public string Extension => Path.GetExtension(FilePath).ToUpperInvariant();
    public long FileSize { get; }
    public DateTime LastWriteTime { get; }

    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? FileSizeKB => Math.Round(FileSize / 1024.0, 1);

    public ImageItem(string filePath)
    {
        FilePath = filePath;
        var info = new FileInfo(filePath);
        FileSize = info.Length;
        LastWriteTime = info.LastWriteTime;
    }
}