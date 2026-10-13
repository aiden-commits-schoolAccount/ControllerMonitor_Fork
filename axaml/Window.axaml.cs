using System.Text.Json.Nodes;
using Avalonia.Input;
using Avalonia.Threading;
using LunaLib.Text;

using SDL2;

namespace ControllerMonitor;

public partial class Window : Avalonia.Controls.Window
{
    public SettingsWindow SettingsWindow { get; private set; }
    public TranslationsKeeper Translations { get; private set; } = new((string)Data.Json["currentLanguage"], "lang/en_us.json");
    public bool MessageBeingShown { get; private set; } = false;
    public string NoCompatibleControllerMessage = "placeholder";
    public WarningBox? NoCompatibleControllerWarningBox;
    public bool WarningBoxRetrying = false;
    private nint controller = nint.Zero;

    private readonly DispatcherTimer Timer_ReadController =
        new() { Interval = TimeSpan.FromMilliseconds(10) };

    public Window()
    {
        InitializeComponent();
        File.WriteAllText((string)Data.Json["outputFile"], "Window Initialized\n");
        this.SettingsWindow = new(this);
        this.SettingsWindow.Activate();

        this.ReloadLanguage();

        // SDL
        if (SDL.SDL_Init(SDL.SDL_INIT_GAMECONTROLLER) < 0)
        {
            return;
        }

        // Enable controller hotplug events
        SDL.SDL_GameControllerEventState(SDL.SDL_ENABLE);

        // Try to find an initial controller
        this.FindController();

        this.Timer_ReadController.Tick += this.ReadController;
        this.Timer_ReadController.Start();

        this.Closed += this.Window_Closed;
    }

    private void FindController()
    {
        if (this.controller != nint.Zero)
            return;

        for (int i = 0; i < SDL.SDL_NumJoysticks(); i++)
        {
            if (SDL.SDL_IsGameController(i) != SDL.SDL_bool.SDL_TRUE)
                continue;

            nint Controller = SDL.SDL_GameControllerOpen(i);

            if (Controller != nint.Zero)
            {
                this.controller = Controller;
                return;
            }
        }
    }

    private async void ReadController(object? sender, EventArgs e)
    {
        bool doController = true;
        SDL.SDL_GameControllerUpdate();
        if (this.NoCompatibleControllerWarningBox?.Skipped != true)
        {
            this.NoCompatibleControllerWarningBox?.SkipButton.IsVisible = (bool)Data.Json["includeKeyboard"];
            if (this.controller != nint.Zero)
            {
                if (SDL.SDL_GameControllerGetAttached(this.controller) == SDL.SDL_bool.SDL_FALSE)
                {
                    SDL.SDL_GameControllerClose(this.controller);
                    this.controller = nint.Zero;
                }
            }

            if (this.controller == nint.Zero)
                this.FindController();

            if (this.controller == nint.Zero)
            {
                this.NoCompatibleControllerWarningBox ??= new WarningBox(this);

                if (this.NoCompatibleControllerWarningBox.Message.Text != this.NoCompatibleControllerMessage)
                    this.NoCompatibleControllerWarningBox.Message.Text = this.NoCompatibleControllerMessage;

                if (!this.NoCompatibleControllerWarningBox.IsVisible)
                    this.NoCompatibleControllerWarningBox.Show();

                doController = false;
            }
        }

        if (this.controller == nint.Zero)
            doController = false;

        var keyBinds = (Dictionary<string, Object?>)Data.Json["keyBinds"];

        string[] throttleLabelParts = (this.ThrottleLabel.Text ?? string.Empty).Split(" ");
        string[] brakeLabelParts = (this.BrakeLabel.Text ?? string.Empty).Split(" ");
        string[] steeringLabelParts = (this.SteeringLabel.Text ?? string.Empty).Split(" ");

        if (doController)
        {
            double brake = SDL.SDL_GameControllerGetAxis(
                this.controller,
                SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT
            ) / 32767.0 * 100;

            double throttle = SDL.SDL_GameControllerGetAxis(
                this.controller,
                SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT
            ) / 32767.0 * 100;

            this.BrakeLabel.Text = $"{brakeLabelParts[0]} {brake:F0}%";

            this.ThrottleLabel.Text = $"{throttleLabelParts[0]} {throttle:F0}%";

            this.BrakeBar.Value = (int)Math.Round(brake);
            this.ThrottleBar.Value = (int)Math.Round(throttle);

            int steer = (int)Math.Round(
                SDL.SDL_GameControllerGetAxis(
                    this.controller,
                    SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX
                ) / 32767.0 * 100
            );

            if (Math.Abs(steer) <= 5)
                steer = 0;

            this.SteeringLabel.Text = $"{steeringLabelParts[0]} {steer:F0}%";

            this.SteeringBar.Value = steer;

            this.NoCompatibleControllerWarningBox?.Hide();
        }

        

        if (!(bool)Data.Json["includeKeyboard"])
            return;

        if (keyBinds["throttle"] is string throttleKey && Input.IsKeyPressed(throttleKey))
        {
            this.ThrottleBar.Value = 100;
            this.ThrottleLabel.Text = $"{throttleLabelParts[0]} 100%";
        }

        if (keyBinds["brake"] is string brakeKey && Input.IsKeyPressed(brakeKey))
        {
            this.BrakeBar.Value = 100;
            this.BrakeLabel.Text = $"{brakeLabelParts[0]} 100%";
        }

        int keyboardSteer = 0;

        if (keyBinds["negSteer"] is string negSteerKey && Input.IsKeyPressed(negSteerKey))
        {
            keyboardSteer = -100;
        }

        if (keyBinds["posSteer"] is string posSteerKey && Input.IsKeyPressed(posSteerKey))
        {
            keyboardSteer = 100;
        }

        if (keyboardSteer > 0)
        {
            this.SteeringLabel.Text = $"{steeringLabelParts[0]} {keyboardSteer:F0}%";
            this.SteeringBar.Value = keyboardSteer;
        }
    }

    public void UpdateTranslations(string languagePath, string fallBackLanguagePath = "lang/en_us.json")
    {
        Data.Json["currentLanguage"] = languagePath;
        Data.Save();

        this.Translations.LanguagePath = languagePath;
        this.Translations.FallBackLanguagePath = fallBackLanguagePath;

        this.Translations.UpdateTranslations();
        this.Translations.UpdateFallBackTranslations();

        this.ReloadLanguage();
    }
    public void ReloadLanguage()
    {
        this.BrakeLabel.Text = this.Translations.GetTranslation("app.label.inputs.l2", ["0"]);
        this.ThrottleLabel.Text = this.Translations.GetTranslation("app.label.inputs.r2", ["0"]);
        this.SteeringLabel.Text = this.Translations.GetTranslation("app.label.inputs.l", ["0"]);
        this.NoCompatibleControllerMessage = this.Translations.GetTranslation("app.messagebox.no_compatible_controller");
        this.SettingsWindow.ReloadLanguage();
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        this.Timer_ReadController.Stop();

        if (this.controller != nint.Zero)
        {
            SDL.SDL_GameControllerClose(this.controller);
            this.controller = nint.Zero;
        }

        SDL.SDL_Quit();

        Environment.Exit(0); // closes all background threads aswell
    }


    private void SettingsButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        File.AppendAllText((string)Data.Json["outputFile"], "SettingsButton Clicked\n");

        if (this.SettingsWindow.IsVisible)
        {
            SettingsWindow.Hide();
            return;
        }

        this.SettingsWindow.Show();
        this.SettingsWindow.Activate();
    }
}
