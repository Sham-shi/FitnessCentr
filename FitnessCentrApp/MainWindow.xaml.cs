using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Xceed.Wpf.AvalonDock.Themes;

namespace FitnessCentrApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void LightTheme_Click(object sender, RoutedEventArgs e)
    {
        ApplicationThemeManager.Apply(ApplicationTheme.Light);
    }

    private void DarkTheme_Click(object sender, RoutedEventArgs e)
    {
       ApplicationThemeManager.Apply(ApplicationTheme.Dark);
    }

    private void SystemTheme_Click(object sender, RoutedEventArgs e)
    {
        ApplicationThemeManager.Apply(ApplicationTheme.Unknown);
    }
}