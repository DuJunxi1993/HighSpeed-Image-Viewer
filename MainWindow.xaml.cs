using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using HighSpeedImageViewer.Controls;
using HighSpeedImageViewer.Helpers;
using HighSpeedImageViewer.Models;
using HighSpeedImageViewer.Services;
using Wpf.Ui.Controls;

namespace HighSpeedImageViewer;

public partial class MainWindow : FluentWindow
{
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    private readonly NavigationService _navigation = new();
    private readonly SlideshowService _slideshow = new();
    private readonly ThumbnailCache _thumbnailCache;
    private bool _showSidePanel;
    private bool _isFullscreen;
    private WindowState _prevWindowState;
    private IntPtr _hwnd;

    private DispatcherTimer? _toolbarHideTimer;
    private bool _isToolbarVisible = true;
    private Point _lastMousePosition;

    public MainWindow()
    {
        InitializeComponent();

        var cacheDir = Path.Combine(Path.GetTempPath(), "ImageViewerNeo", "thumbs");
        Directory.CreateDirectory(cacheDir);
        _thumbnailCache = new ThumbnailCache(Path.Combine(cacheDir, "cache.db"));

        _navigation.CollectionChanged += OnCollectionChanged;
        _navigation.CurrentImageChanged += OnCurrentImageChanged;
        _slideshow.NextRequested += () => Dispatcher.Invoke(() => _navigation.MoveNext());

        ImageViewer.ZoomChanged += zoom =>
            Dispatcher.Invoke(() =>
            {
                ZoomTextBlock.Text = $"{zoom * 100:F0}%";
            });

        ImageViewer.StatusChanged += msg =>
            Dispatcher.Invoke(() => StatusText.Text = msg);

        SetupSidePanel();

        _toolbarHideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
        _toolbarHideTimer.Tick += (s, e) =>
        {
            if (_isFullscreen && _isToolbarVisible)
            {
                HideToolbar();
            }
            _toolbarHideTimer?.Stop();
        };

        Loaded += (_, _) =>
        {
            _hwnd = new WindowInteropHelper(this).Handle;
            ComponentDispatcher.ThreadFilterMessage += OnThreadFilterMessage;
            Focus();
        };

        Closed += (_, _) =>
        {
            ComponentDispatcher.ThreadFilterMessage -= OnThreadFilterMessage;
            _slideshow.Dispose();
            _thumbnailCache.Dispose();
            _navigation.Dispose();
            _toolbarHideTimer?.Stop();
        };

        MouseMove += OnWindowMouseMove;
        Drop += OnWindowDrop;
    }

    public MainWindow(string filePath) : this()
    {
        if (File.Exists(filePath))
        {
            var folder = Path.GetDirectoryName(filePath);
            if (folder != null)
            {
                _navigation.LoadFolder(folder);
                _navigation.NavigateTo(filePath);
            }
        }
    }

    private void OnWindowMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isFullscreen) return;

        var pos = e.GetPosition(this);
        if (Math.Abs(pos.X - _lastMousePosition.X) > 5 || Math.Abs(pos.Y - _lastMousePosition.Y) > 5)
        {
            _lastMousePosition = pos;
            if (!_isToolbarVisible)
            {
                ShowToolbar();
            }
            ResetToolbarHideTimer();
        }
    }

    private void OnWindowDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                var file = files[0];
                if (File.Exists(file) && FormatHelper.IsSupported(file))
                {
                    var folder = Path.GetDirectoryName(file);
                    if (folder != null)
                    {
                        _navigation.LoadFolder(folder);
                        _navigation.NavigateTo(file);
                    }
                }
            }
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
        }
        else
        {
            if (WindowState == WindowState.Maximized)
            {
                var point = e.GetPosition(this);
                var screenPoint = PointToScreen(point);

                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Normal;
                MaximizeIcon.Text = "□";

                Left = screenPoint.X - point.X;
                Top = screenPoint.Y - point.Y;
                Width = RestoreBounds.Width;
                Height = RestoreBounds.Height;

                ResizeMode = ResizeMode.CanResize;
            }

            if (WindowState == WindowState.Normal)
            {
                DragMove();
            }
        }
    }

    private void ShowToolbar()
    {
        _isToolbarVisible = true;
        if (_isFullscreen)
        {
            TitleBarRow.Height = new GridLength(48);
        }
        TitleBarArea.Visibility = Visibility.Visible;
        BottomBar.Visibility = Visibility.Visible;
    }

    private void HideToolbar()
    {
        _isToolbarVisible = false;
        if (_isFullscreen)
        {
            TitleBarRow.Height = new GridLength(0);
        }
        TitleBarArea.Visibility = Visibility.Collapsed;
        BottomBar.Visibility = Visibility.Collapsed;
    }

    private void ResetToolbarHideTimer()
    {
        _toolbarHideTimer?.Stop();
        _toolbarHideTimer?.Start();
    }

    private void OnThreadFilterMessage(ref MSG msg, ref bool handled)
    {
        if (handled) return;
        if (msg.message != WM_KEYDOWN && msg.message != WM_SYSKEYDOWN) return;
        if (msg.hwnd != _hwnd) return;

        var key = KeyInterop.KeyFromVirtualKey((int)msg.wParam);
        if (HandleKey(key))
            handled = true;
    }

    private bool HandleKey(Key key)
    {
        var ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        var ctrlShift = ctrl && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

        if (ctrlShift)
        {
            switch (key)
            {
                case Key.P:
                    ToggleSidePanel(); return true;
                default:
                    break;
            }
        }

        if (ctrl)
        {
            switch (key)
            {
                case Key.F:
                    ToggleFullscreen(); return true;
                case Key.O:
                    BtnOpen_Click(this, new RoutedEventArgs()); return true;
                case Key.OemPlus:
                case Key.Add:
                    ImageViewer.ZoomIn(); return true;
                case Key.OemMinus:
                case Key.Subtract:
                    ImageViewer.ZoomOut(); return true;
                case Key.D0:
                    ImageViewer.FitToScreen(); return true;
                default:
                    break;
            }
        }

        switch (key)
        {
            case Key.Left:
            case Key.Up:
                _navigation.MovePrevious(); return true;
            case Key.Right:
            case Key.Down:
                _navigation.MoveNext(); return true;
            case Key.Escape:
                if (_slideshow.IsRunning) { _slideshow.Stop(); UpdateSlideshowButton(); }
                if (_isFullscreen) ToggleFullscreen();
                return true;
            case Key.F5:
                ToggleSlideshow(); return true;
            default:
                return false;
        }
    }

    private void SetupSidePanel()
    {
        SidePanel.ItemClicked += idx => _navigation.MoveTo(idx);
    }

    private void OnCollectionChanged()
    {
        SidePanel.ItemsSource = _navigation.Items;
    }

    private void OnCurrentImageChanged(ImageItem item)
    {
        Title = $"HighSpeed Image Viewer - {item.FileName} ({_navigation.CurrentIndex + 1}/{_navigation.Count})";
        ImageViewer.LoadImage(item.FilePath);
        StatusText.Text = item.FileName;
        ImageInfo.Text = $"{item.Width}×{item.Height}  |  {item.FileSizeKB} KB";
        ImageIndexInfo.Text = $"{_navigation.CurrentIndex + 1}/{_navigation.Count}";
        SidePanel.SelectedIndex = _navigation.CurrentIndex;
        UpdateSidebarIndicator();
        Dispatcher.BeginInvoke(() => SidePanel.ScrollIntoView(_navigation.CurrentIndex), System.Windows.Threading.DispatcherPriority.Loaded);
        if (ImageViewer.ContextMenu != null)
            ImageViewer.ContextMenu.IsOpen = false;
    }

    private void UpdateSidebarIndicator()
    {
        SidePanel.UpdateSelectionIndicator();
    }

    private void BtnOpen_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = Helpers.FormatHelper.Filter,
            Multiselect = false,
            RestoreDirectory = true
        };

        if (dialog.ShowDialog() == true)
        {
            var folder = Path.GetDirectoryName(dialog.FileName)!;
            _navigation.LoadFolder(folder);
            _navigation.NavigateTo(dialog.FileName);
        }
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void BtnMaximize_Click(object sender, RoutedEventArgs e)
    {
        ToggleMaximize();
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ToggleMaximize()
    {
        if (WindowState == WindowState.Maximized)
        {
            WindowState = WindowState.Normal;
            MaximizeIcon.Text = "□";
        }
        else
        {
            WindowState = WindowState.Maximized;
            MaximizeIcon.Text = "❐";
        }
    }

    private void BtnPrev_Click(object sender, RoutedEventArgs e) => _navigation.MovePrevious();
    private void BtnNext_Click(object sender, RoutedEventArgs e) => _navigation.MoveNext();
    private void BtnFit_Click(object sender, RoutedEventArgs e) => ImageViewer.FitToScreen();
    private void BtnSlideshow_Click(object sender, RoutedEventArgs e) => ToggleSlideshow();
    private void BtnFullscreen_Click(object sender, RoutedEventArgs e) => ToggleFullscreen();
    private void BtnSidePanel_Click(object sender, RoutedEventArgs e) => ToggleSidePanel();

    private void ToggleSlideshow()
    {
        _slideshow.Toggle();
        UpdateSlideshowButton();
    }

    private void UpdateSlideshowButton()
    {
        if (_slideshow.IsRunning)
        {
            SlideshowIcon.Text = "⏸";
            SlideshowText.Text = "暂停";
        }
        else
        {
            SlideshowIcon.Text = "▶";
            SlideshowText.Text = "幻灯片";
        }
    }

    private void ToggleFullscreen()
    {
        _isFullscreen = !_isFullscreen;

        if (_isFullscreen)
        {
            _prevWindowState = WindowState;
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            HideToolbar();
            TitleBarRow.Height = new GridLength(0);
            ResetToolbarHideTimer();
        }
        else
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = _prevWindowState;
            TitleBarRow.Height = new GridLength(48);
            ShowToolbar();
            _toolbarHideTimer?.Stop();
        }

        Dispatcher.BeginInvoke(() =>
        {
            UpdateLayout();
            ImageViewer.FitToScreen();
            Focus();
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }
    private void ZoomTextBlock_Click(object sender, MouseButtonEventArgs e)
    {
        ImageViewer.ZoomToOriginal();
    }

    private void ToggleSidePanel()
    {
        _showSidePanel = !_showSidePanel;
        if (_showSidePanel)
        {
            SideColumn.Width = new GridLength(280);
            SidePanelBorder.Visibility = Visibility.Visible;
        }
        else
        {
            SideColumn.Width = new GridLength(0);
            SidePanelBorder.Visibility = Visibility.Collapsed;
        }
    }

    private void CtxCopyPath_Click(object sender, RoutedEventArgs e)
    {
        if (_navigation.Current == null) return;
        Clipboard.SetText(_navigation.Current.FilePath);
    }

    private void CtxOpenInExplorer_Click(object sender, RoutedEventArgs e)
    {
        if (_navigation.Current == null) return;
        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{_navigation.Current.FilePath}\"");
    }

    private void CtxPrint_Click(object sender, RoutedEventArgs e)
    {
        if (_navigation.Current == null) return;
        var dialog = new System.Windows.Controls.PrintDialog();
        if (dialog.ShowDialog() == true)
        {
            dialog.PrintVisual(ImageViewer, _navigation.Current.FileName);
        }
    }

    private void CtxSetWallpaper_Click(object sender, RoutedEventArgs e)
    {
        if (_navigation.Current == null) return;
        SetWallpaper(_navigation.Current.FilePath);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

    private const int SPI_SETDESKWALLPAPER = 0x0014;
    private const int SPIF_UPDATEINIFILE = 0x01;
    private const int SPIF_SENDCHANGE = 0x02;

    private void SetWallpaper(string path)
    {
        SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
    }
}