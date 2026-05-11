using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HighSpeedImageViewer.Models;
using SkiaSharp;

namespace HighSpeedImageViewer.Services;

public class ThumbnailCache : IDisposable
{
    private const int DefaultCacheSize = 500;
    private readonly string _dbPath;
    private readonly ConcurrentDictionary<string, WeakReference<SKBitmap>> _memCache = new();
    private readonly int _maxMemCacheSize;
    private readonly SemaphoreSlim _dbLock = new(1, 1);
    private bool _disposed;

    public ThumbnailCache(string dbPath, int maxMemCacheSize = DefaultCacheSize)
    {
        _dbPath = dbPath;
        _maxMemCacheSize = maxMemCacheSize;
    }

    public async Task<SKBitmap?> GetOrCreateAsync(string filePath, int size = 256, CancellationToken ct = default)
    {
        if (_disposed) return null;

        if (_memCache.TryGetValue(filePath, out var weakRef) && weakRef.TryGetTarget(out var cached))
        {
            try
            {
                if (!cached.IsNull && cached.Width == size)
                    return cached;
            }
            catch
            {
                _memCache.TryRemove(filePath, out _);
            }
        }

        var result = await Task.Run(() => CreateThumbnail(filePath, size), ct);
        if (result != null && !ct.IsCancellationRequested)
        {
            if (_memCache.Count < _maxMemCacheSize)
                _memCache[filePath] = new WeakReference<SKBitmap>(result);
        }

        return result;
    }

    private static SKBitmap? CreateThumbnail(string filePath, int size)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.SequentialScan);
            using var codec = SKCodec.Create(stream);
            if (codec == null) return null;

            var info = codec.Info;
            var scale = Math.Max((float)info.Width / size, (float)info.Height / size);
            if (scale < 1) scale = 1;

            var sampleSize = (int)Math.Ceiling(scale);
            var decodeW = Math.Max(1, info.Width / sampleSize);
            var decodeH = Math.Max(1, info.Height / sampleSize);

            using var bitmap = SKBitmap.Decode(codec, new SKImageInfo(decodeW, decodeH, SKColorType.Rgba8888));
            if (bitmap == null) return null;

            if (bitmap.Width <= size && bitmap.Height <= size)
                return bitmap.Copy();

            using var resized = bitmap.Resize(new SKImageInfo(size, size), new SKSamplingOptions(SKFilterMode.Linear));
            return resized?.Copy();
        }
        catch
        {
            return null;
        }
    }

    public void Invalidate(string filePath)
    {
        _memCache.TryRemove(filePath, out _);
    }

    public void Clear()
    {
        _memCache.Clear();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _dbLock.Dispose();
        Clear();
        GC.SuppressFinalize(this);
    }
}