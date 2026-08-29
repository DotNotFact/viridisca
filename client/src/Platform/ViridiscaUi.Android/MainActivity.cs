using Avalonia.ReactiveUI;
using Android.Content.PM;
using Android.App;
using Avalonia;

namespace ViridiscaUi.Android;

[Activity(Label = "ViridiscaUi.Android", Theme = "@style/MyTheme.NoActionBar", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder).WithInterFont().UseReactiveUI();
    }
}
