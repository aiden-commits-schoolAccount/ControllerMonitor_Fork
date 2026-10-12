using Avalonia.Controls;

namespace ControllerMonitor;

public partial class ChangeLanguageWindow : Avalonia.Controls.Window
{
    private readonly Window MainWindow;
    private readonly SettingsWindow SettingsWindow;
    public ChangeLanguageWindow(Window mainWindow, SettingsWindow settingsWindow)
    {
        InitializeComponent();
        this.MainWindow = mainWindow;
        this.SettingsWindow = settingsWindow;
        Closing += ChangeLanguageWindow_Closing;
        PopulateLanguageButtons();
    }

    private void PopulateLanguageButtons()
    {
        LanguageButtonsPanel.Children.Clear();

        if (!Directory.Exists("lang"))
            return;

        foreach (string LanguagePath in Directory.GetFiles("lang", "*.json"))
        {
            string LanguageName = Path.GetFileName(LanguagePath);
            Button LanguageButton = new() { Content = new TextBlock { Text = LanguageName }, FontSize = 18, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
            File.AppendAllText((string)Data.Json["outputFile"], $"Language button created for {LanguageName}\n");
            LanguageButton.Click += (_, _) =>
            {
                MainWindow.UpdateTranslations(LanguagePath.Replace("\\", "/"));
            };
            LanguageButtonsPanel.Children.Add(LanguageButton);
        }
    }

    private void ChangeLanguageWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}