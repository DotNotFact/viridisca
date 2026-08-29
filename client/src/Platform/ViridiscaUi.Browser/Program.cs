using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Avalonia.Browser;
using ViridiscaUi;
using Avalonia;

internal sealed partial class Program
{
    private static Task Main(string[] _)
        => BuildAvaloniaApp().WithInterFont().UseReactiveUI().StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
