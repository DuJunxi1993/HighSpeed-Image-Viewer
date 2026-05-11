using System.Windows.Input;

namespace HighSpeedImageViewer.Controls;

public static class AppCommands
{
    public static readonly RoutedUICommand Prev = new("上一张", "Prev", typeof(AppCommands));
    public static readonly RoutedUICommand Next = new("下一张", "Next", typeof(AppCommands));
    public static readonly RoutedUICommand ToggleSlideshow = new("幻灯片", "ToggleSlideshow", typeof(AppCommands));
    public static readonly RoutedUICommand ToggleFullscreen = new("全屏", "ToggleFullscreen", typeof(AppCommands));
    public static readonly RoutedUICommand FitToScreen = new("适应", "FitToScreen", typeof(AppCommands));
    public static readonly RoutedUICommand ZoomIn = new("放大", "ZoomIn", typeof(AppCommands));
    public static readonly RoutedUICommand ZoomOut = new("缩小", "ZoomOut", typeof(AppCommands));
    public static readonly RoutedUICommand Open = new("打开", "Open", typeof(AppCommands));
    public static readonly RoutedUICommand ToggleSidePanel = new("侧栏", "ToggleSidePanel", typeof(AppCommands));
    public static readonly RoutedUICommand Escape = new("退出", "Escape", typeof(AppCommands));
}