using Avalonia;
using Avalonia.Headless;
using Avalonia.ReactiveUI;
using ViridiscaUi;

namespace ViridiscaUi.HeadlessTests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .UseReactiveUI();
}
