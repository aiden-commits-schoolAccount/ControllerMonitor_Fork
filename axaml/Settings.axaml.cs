using System.Text.Json.Nodes;
using Avalonia.Controls;
using Avalonia.Input;
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
        File.AppendAllText((string)Data.Json["outputFile"], "LanguageButton clicked");
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
        this.IncludeKeyboardInputsBool.Content = this.MainWindow.Translations.GetTranslation("app.label.settings.include_keyboard_inputs");
        this.IncludeKeyboardInputsBool.IsChecked = (bool)Data.Json["includeKeyboard"];
    }

    private void IncludeKeyboardInputs_UpdateJson(object sender, RoutedEventArgs e)
    {
        File.AppendAllText((string)Data.Json["outputFile"], "Checkmark IncludeKeyboardInputsBool changed\n");
        Data.Json["includeKeyboard"] = this.IncludeKeyboardInputsBool.IsChecked == true;
        Data.Save();
    }
}
