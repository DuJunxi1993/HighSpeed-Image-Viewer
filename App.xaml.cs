using System;
using System.IO;
using System.Linq;
using System.Windows;
using HighSpeedImageViewer.Helpers;
using Wpf.Ui;

namespace HighSpeedImageViewer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
            Wpf.Ui.Appearance.ApplicationTheme.Dark,
            Wpf.Ui.Controls.WindowBackdropType.Mica,
            true);

        if (e.Args.Length > 0)
        {
            var filePath = e.Args[0];
            if (File.Exists(filePath) && FormatHelper.IsSupported(filePath))
            {
                var mainWindow = new MainWindow(filePath);
                mainWindow.Show();
                return;
            }
        }

        var mainWindowDefault = new MainWindow();
        mainWindowDefault.Show();
    }
}