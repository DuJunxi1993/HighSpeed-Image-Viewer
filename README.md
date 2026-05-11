# HighSpeed Image Viewer

A fast, lightweight WPF image viewer with SkiaSharp rendering, thumbnail sidebar, slideshow, and fullscreen support.

## Features

- SkiaSharp-powered image decoding and rendering
- Smooth zoom (scroll wheel, keyboard shortcuts, double-click fit/original toggle)
- Thumbnail sidebar with scroll-into-view and selection highlighting
- Slideshow mode with configurable interval
- Fullscreen mode with auto-hiding toolbar
- Right-click context menu (copy path, open in explorer, print, set wallpaper)
- FileSystemWatcher for live folder updates
- Drag-and-drop support
- Command-line argument support
- Thumbnail caching with SQLite

## Supported Formats

jpg, jpeg, png, bmp, gif, tiff, tif, webp, heic, heif, avif, ico

## Controls

| Action | Mouse | Keyboard |
|---|---|---|
| Zoom in/out | Scroll wheel | `+` / `-` |
| Reset zoom | Click zoom % | `*` |
| Fit to screen | Double-click image | `0` |
| Original size | Double-click image | `1` |
| Previous image | Click prev button | `←` / `PgUp` |
| Next image | Click next button | `→` / `PgDn` |
| First image | | `Home` |
| Last image | | `End` |
| Slideshow | | `Space` |
| Fullscreen | | `F11` |
| Toggle sidebar | | `S` |

## Build

```powershell
dotnet build
dotnet run
```

Output: `bin/Debug/net10.0-windows/HighSpeedImageViewer.exe`

## Release Build

```powershell
dotnet publish -c Release
```

## Tech Stack

- WPF-UI 3.0.5 (FluentWindow, dark theme)
- SkiaSharp 3.116.1 (image rendering)
- SQLitePCLRaw (thumbnail cache)
- .NET 10.0-windows
