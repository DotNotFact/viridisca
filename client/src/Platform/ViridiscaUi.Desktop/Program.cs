using Avalonia.ReactiveUI; 
using Avalonia;
using Avalonia.Logging;
using System; 

namespace ViridiscaUi.Desktop;

internal sealed class Program
{
    [STAThread]
    public static void Main(string[] args) 
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fatal error during application startup: {ex.Message}");
            throw;
        }
    }
     
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace(LogEventLevel.Information)
            .UseReactiveUI();
}
