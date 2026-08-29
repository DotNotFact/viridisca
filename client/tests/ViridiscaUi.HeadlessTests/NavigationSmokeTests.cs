using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ViridiscaUi.Base;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Navigations;
using ViridiscaUi.ViewModels;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.ViewModels.Education;
using Xunit;

namespace ViridiscaUi.HeadlessTests;

/// <summary>
/// Boots the real app headlessly (no visible window - see Avalonia.Headless), logs in with
/// the demo account and walks every registered route, asserting the router actually lands on
/// the expected ViewModel. Catches DI resolution failures, XAML binding crashes, and
/// silently-failed navigation without requiring a visible desktop session.
/// </summary>
public class NavigationSmokeTests
{
    private const string DemoEmail = "demo@viridisca.local";
    private const string DemoPassword = "Demo12345!";

    /// <summary>
    /// Blocks the calling thread until the task completes, while repeatedly pumping
    /// Dispatcher.UIThread so posted continuations (Rx's AvaloniaScheduler, async
    /// service calls resuming on the UI SynchronizationContext) actually get to run.
    /// SetupWithLifetime (unlike a real app's StartWithClassicDesktopLifetime) does not
    /// start a background dispatcher loop - nothing pumps the queue unless we do it
    /// ourselves, so a plain `await` on this same thread deadlocks forever waiting on a
    /// continuation that only this thread could ever run.
    /// </summary>
    private static T RunSync<T>(Func<Task<T>> taskFactory)
    {
        var task = taskFactory();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (!task.IsCompleted)
        {
            Dispatcher.UIThread.RunJobs();
            if (sw.Elapsed > TimeSpan.FromSeconds(30))
            {
                throw new TimeoutException("RunSync timed out after 30s waiting for task completion.");
            }
            Thread.Sleep(1);
        }
        return task.GetAwaiter().GetResult();
    }

    private static void RunSync(Func<Task> taskFactory)
    {
        RunSync(async () => { await taskFactory(); return true; });
    }

    [Fact]
    public async Task AllRoutes_NavigateSuccessfully()
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime();
        var appBuilder = TestAppBuilder.BuildAvaloniaApp();
        appBuilder.SetupWithLifetime(lifetime);

        Dispatcher.UIThread.RunJobs();

        var services = ApplicationBootstrapper.Services;
        var authService = services.GetRequiredService<IAuthService>();
        var personSessionService = services.GetRequiredService<IPersonSessionService>();
        var navigationService = services.GetRequiredService<IUnifiedNavigationService>();

        var screenshotDir = Path.Combine(AppContext.BaseDirectory, "screenshots");
        Directory.CreateDirectory(screenshotDir);

        var window = lifetime.MainWindow!;
        window.Width = 1440;
        window.Height = 900;
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        // Before logging in: MainViewModel.InitializeNavigation routes to "auth" on
        // construction whenever there's no current session, and the sidebar (student/course
        // counts, page navigation) is gated on MainViewModel.IsLoggedIn, which defaults to
        // false and only flips true on a successful login. This is the actual first screen a
        // real, never-authenticated visitor sees. Every other screenshot in this test -
        // including whatever it captures for the "auth"/"login"/"register" routes themselves,
        // since those stay reachable and get walked like any other route below - happens
        // after the demo login a few lines down, so none of those can stand in for what an
        // anonymous visitor actually sees (they'd all show the sidebar).
        var mainViewModelBeforeLogin = Assert.IsType<MainViewModel>(window.DataContext);
        Assert.False(mainViewModelBeforeLogin.IsLoggedIn, "A never-authenticated session must not report IsLoggedIn=true (it would show the sidebar).");
        Assert.IsType<AuthenticationViewModel>(mainViewModelBeforeLogin.Router.GetCurrentViewModel());
        using (var preLoginFrame = window.CaptureRenderedFrame())
        {
            preLoginFrame?.Save(Path.Combine(screenshotDir, "auth-unauthenticated.png"));
        }

        var loginResult = RunSync(() => authService.AuthenticateAsync(DemoEmail, DemoPassword));
        Assert.True(loginResult.Success, $"Demo login failed: {loginResult.ErrorMessage}");
        Assert.NotNull(loginResult.Person);
        personSessionService.SetCurrentPerson(loginResult.Person!);

        var homeOk = RunSync(() => navigationService.NavigateToAsync("home"));
        Dispatcher.UIThread.RunJobs();
        Assert.True(homeOk, "Navigation to 'home' after login failed");

        var failures = new List<string>();

        foreach (var route in navigationService.Routes.OrderBy(r => r.Order))
        {
            bool ok;
            Exception? thrown = null;
            try
            {
                ok = RunSync(() => navigationService.NavigateToAsync(route.Path));
                Dispatcher.UIThread.RunJobs();
                // Give any first-time-load async work (EF queries, HTTP calls) a chance to
                // run and marshal back, and the layout pass a chance to settle before capture.
                RunSync(() => Task.Delay(300));
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
            }
            catch (Exception ex)
            {
                ok = false;
                thrown = ex;
            }

            var currentViewModel = navigationService.CurrentScreen?.Router.GetCurrentViewModel();
            var landedOnExpectedType = currentViewModel?.GetType() == route.ViewModelType;

            if (!ok || !landedOnExpectedType)
            {
                var actualType = currentViewModel?.GetType().Name ?? "null";
                var detail = thrown != null ? $" Exception: {thrown}" : string.Empty;
                failures.Add($"Route '{route.Path}' ({route.ViewModelType.Name}): expected to land on {route.ViewModelType.Name}, got {actualType}.{detail}");
                continue;
            }

            try
            {
                using var frame = window.CaptureRenderedFrame();
                frame?.Save(Path.Combine(screenshotDir, $"{route.Path.Replace('/', '_')}.png"));
            }
            catch
            {
                // Screenshot capture is best-effort; a failure here shouldn't fail the route check.
            }
        }

        Assert.True(failures.Count == 0, $"{failures.Count} route(s) failed to navigate correctly:\n" + string.Join("\n", failures));

        // Dialogs are shown via IDialogService, not through the router, so the route-walking
        // loop above never exercises them. DialogService constructs dialog ViewModels through
        // ActivatorUtilities.CreateInstance(serviceProvider, screen) rather than a plain
        // GetRequiredService<T>, since IScreen is only ever supplied this way and is never
        // registered in the container itself. Reproduce exactly that construction call here
        // for every real (non-stub) dialog ViewModel and then run its LoadXAsync, without going
        // through an actual modal Window/ShowDialog - that part is just UI chrome and, in this
        // headless host, isn't reliably observable (IClassicDesktopStyleApplicationLifetime.
        // Windows never gets populated by a manually-constructed lifetime the way a real
        // StartWithClassicDesktopLifetime app does, so a window-detection loop times out even
        // though everything underneath works) - this targets the actual risk (DI/IScreen
        // wiring), not the chrome around it.
        var screen = navigationService.CurrentScreen!;
        var dialogFailures = new List<string>();

        void VerifyDialogConstructs<T>(string name, Func<T, Task> load) where T : class
        {
            try
            {
                var viewModel = ActivatorUtilities.CreateInstance<T>(services, screen);
                RunSync(() => load(viewModel));
            }
            catch (Exception ex)
            {
                dialogFailures.Add($"Dialog ViewModel '{name}' failed to construct/load: {ex}");
            }
        }

        VerifyDialogConstructs<StudentDialogViewModel>("Student", vm => vm.LoadStudentAsync(new Student()));
        VerifyDialogConstructs<TeacherDialogViewModel>("Teacher", vm => vm.LoadTeacherAsync(new Teacher()));
        VerifyDialogConstructs<GroupDialogViewModel>("Group", vm => vm.LoadGroupAsync(new Group()));
        VerifyDialogConstructs<SubjectDialogViewModel>("Subject", vm => vm.LoadSubjectAsync(new Subject()));
        VerifyDialogConstructs<CourseEditorViewModel>("Course", vm => vm.LoadCourseAsync(new Course()));
        VerifyDialogConstructs<GradeDialogViewModel>("Grade", vm => { vm.LoadGradeAsync(new Grade()); return Task.CompletedTask; });

        Assert.True(dialogFailures.Count == 0, $"{dialogFailures.Count} dialog ViewModel(s) failed:\n" + string.Join("\n", dialogFailures));
    }
}
