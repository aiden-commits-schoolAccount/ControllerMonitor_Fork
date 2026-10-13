using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ControllerMonitor;

public partial class SettingsWindow : Avalonia.Controls.Window
{
    private readonly ChangeLanguageWindow ChangeLanguageWindow;
    private readonly Window MainWindow;

    public SettingsWindow(Window mainWindow)
    {
        InitializeComponent();
        Closing += SettingsWindow_Closing;
        this.MainWindow = mainWindow;
        ChangeLanguageWindow = new ChangeLanguageWindow(mainWindow, this);
        ChangeLanguageWindow.Activate();
    }

    private void SettingsWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    private void LanguageButton_Click(object? sender, RoutedEventArgs e)
    {
        if (ChangeLanguageWindow.IsVisible)
        {
            ChangeLanguageWindow.Hide();
            return;
        }

        ChangeLanguageWindow.Show();
        ChangeLanguageWindow.Activate();
    }

    public void ReloadLanguage()
    {
        this.LanguageButton.Content = this.MainWindow.Translations.GetTranslation("app.lebal.settings.change_language");
    }
}
