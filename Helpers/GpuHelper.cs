using System;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace HighSpeedImageViewer.Helpers;

public static class GpuHelper
{
    private static GRContext? _context;
    private static readonly object _lock = new();

    public static GRContext GetOrCreateContext()
    {
        if (_context is { IsAbandoned: false })
            return _context;

        lock (_lock)
        {
            if (_context is { IsAbandoned: false })
                return _context;

            _context?.Dispose();
            _context = GRContext.CreateGl();
            return _context;
        }
    }

    public static SKSurface CreateSurface(int width, int height)
    {
        var ctx = GetOrCreateContext();
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        return SKSurface.Create(ctx, false, info);
    }

    public static void DisposeContext()
    {
        lock (_lock)
        {
            _context?.Dispose();
            _context = null;
        }
    }
}