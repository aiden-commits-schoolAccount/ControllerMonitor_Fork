using Avalonia;
using LunaLib.Json;

namespace ControllerMonitor;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        File.WriteAllText("logs/data-startup.txt", "Starting up...\n");
        try
        {
            File.AppendAllText("logs/data-startup.txt", "Loading data...\n");

            var data = Serializer.Load("data/data.json");
            Data.Json = data;

            File.AppendAllText("logs/data-startup.txt", "Data loaded successfully.\n");

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception Exception)
        {
            File.AppendAllText("logs/data-startup.txt", Exception.ToString());
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}