# Agents

## Build

```bash
dotnet build
dotnet run
```

- Single SDK-style project, no solution file needed.
- Output: `bin/Debug/net10.0-windows/ImageViewerNeo.exe`
- Assembly name (`ImageViewerNeo`) differs from project file (`HighSpeedImageViewer`).

## Dependencies

- **WPF-UI 3.0.5** — FluentWindow, dark Mica backdrop (set in App.xaml.cs)
- **SkiaSharp 3.116.1** — image decoding and rendering
- **SQLitePCLRaw** — thumbnail cache DB (stored in `%TEMP%\ImageViewerNeo\thumbs\cache.db`)

## Architecture

| Folder | Purpose |
|--------|---------|
| `Controls/` | SkiaImageViewer (SkiaSharp rendering), ThumbnailPanel (file list), AppCommands |
| `Services/` | ImageLoader (async decode with max-dimension clamp), NavigationService (folder+FSW), SlideshowService, ThumbnailCache |
| `Models/` | ImageItem, ImageLoadResult, ThumbnailEntry |
| `Helpers/` | FormatHelper (supported extensions), GpuHelper (GRContext singleton) |

Entry point: `MainWindow.xaml` / `MainWindow.xaml.cs`. Opens folder via `OpenFileDialog`, navigates to selected file.

## Key Behaviors

- **Image decoding**: ImageLoader clamps largest dimension to `_maxDecodeDimension` (default 7680). Images above limit are downscaled.
- **Navigation**: wraps around (MoveNext/MovePrevious modulo count). FileSystemWatcher reloads folder on create/delete/rename.
- **Zoom**: smooth animated zoom (0.05x–20x) with scroll-wheel zoom-to-cursor and double-click fit/original toggle.
- **Fullscreen**: hides title bar and bottom bar, toolbar auto-hides after 2.5s idle.
- **SkiaImageViewer**: uses `SKSurface` + `WriteableBitmap` for WPF interop; fades in new images over 0.2s.

## No Test Suite

No test project or test framework present. Do not attempt to run tests.
