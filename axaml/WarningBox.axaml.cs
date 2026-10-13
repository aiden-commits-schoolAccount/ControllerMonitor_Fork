using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ControllerMonitor;

public partial class WarningBox : Avalonia.Controls.Window
{
    private readonly Window MainWindow;
    public bool Skipped = false;
    public WarningBox(Window mainWindow)
    {
        InitializeComponent();
        this.MainWindow = mainWindow;
    }

    private void Exit_Click(object? sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

    private void Retry_Click(object? sender, RoutedEventArgs e)
    {
        this.Hide();
    }

    private void Skip_Click(object? sender, RoutedEventArgs e)
    {
        this.Hide();
        this.Skipped = true;
    }

    private void SettingsWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}