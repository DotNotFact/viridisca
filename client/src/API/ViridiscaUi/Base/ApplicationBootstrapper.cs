using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Splat;
using ViridiscaUi.Infrastructure.Extensions;
using ViridiscaUi.ViewModels;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.ViewModels.Education;
using ViridiscaUi.ViewModels.System;
using ViridiscaUi.Views.Auth;
using ViridiscaUi.Views.Common;
using ViridiscaUi.Views.Education;
using ViridiscaUi.Views.System;
using ViridiscaUi.Windows;

namespace ViridiscaUi.Base;

/// <summary>
/// Централизованный класс для инициализации приложения
/// Следует принципу единственной ответственности и обеспечивает чистую архитектуру
/// </summary>
public static class ApplicationBootstrapper
{
    private static IServiceProvider _services = null!;
    public static IServiceProvider Services => _services ?? throw new InvalidOperationException("Application not initialized");

    /// <summary>
    /// Инициализирует приложение для Desktop платформы
    /// </summary>
    public static void InitializeDesktop(IClassicDesktopStyleApplicationLifetime desktop)
    {
        try
        {
            Initialize();

            var mainWindow = _services.GetRequiredService<MainWindow>();
            var mainViewModel = _services.GetRequiredService<MainViewModel>();

            mainWindow.DataContext = mainViewModel;
            StatusLogger.LogInfo("Главная модель представления создана и привязана к окну", "ApplicationBootstrapper");

            desktop.MainWindow = mainWindow;
            StatusLogger.LogSuccess("Десктопное приложение инициализировано успешно", "ApplicationBootstrapper");
        }
        catch (Exception fallbackEx)
        {
            StatusLogger.LogError($"Критическая ошибка инициализации приложения: {fallbackEx.Message}", "ApplicationBootstrapper");
            throw;
        }
    }

    /// <summary>
    /// Инициализирует приложение для Single View платформы
    /// </summary>
    public static void InitializeSingleView(ISingleViewApplicationLifetime singleView)
    {
        try
        {
            Initialize();

            singleView.MainView = new MainView
            {
                DataContext = _services!.GetRequiredService<MainViewModel>()
            };

            StatusLogger.LogSuccess("Single view application initialized successfully", "ApplicationBootstrapper");
        }
        catch (Exception fallbackEx)
        {
            Console.WriteLine($"Fallback initialization also failed: {fallbackEx.Message}");
        }
    }

    /// <summary>
    /// Настраивает ReactiveUI для работы с Avalonia
    /// </summary>
    public static void ConfigureReactiveUI()
    {
        // Регистрация ViewLocator для ReactiveUI
        Locator.CurrentMutable.RegisterViewsForViewModels(typeof(App).Assembly);

        // Настройка ReactiveUI планировщика для Avalonia
        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
    }

    /// <summary>
    /// Основная инициализация приложения
    /// </summary>
    private static void Initialize()
    {
        try
        {
            // Определяем базовый путь более надежно
            var basePath = AppContext.BaseDirectory;

            Console.WriteLine("📋 Building configuration...");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Console.WriteLine("🔧 Creating service collection...");
            var services = new ServiceCollection();

            Console.WriteLine("📋 Adding configuration to services...");
            services.AddSingleton<IConfiguration>(configuration);
            
            Console.WriteLine("🏗️ Adding ViridiscaServices...");
            services.AddViridiscaServices(configuration);

            // UI-specific services
            Console.WriteLine("💬 Adding DialogService...");
            services.AddScoped<IDialogService, DialogService>();

            // Register UI components that are resolved in ApplicationBootstrapper
            Console.WriteLine("🏠 Registering MainWindow and MainViewModel...");
            services.AddTransient<MainWindow>();
            services.AddTransient<MainViewModel>();

            // Register Auth ViewModels
            Console.WriteLine("🔐 Registering Auth ViewModels...");
            services.AddTransient<AuthenticationViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();

            // Register Main ViewModels
            Console.WriteLine("🏠 Registering Main ViewModels...");
            services.AddTransient<HomeViewModel>();
            services.AddTransient<ProfileViewModel>();

            // Register Education ViewModels
            Console.WriteLine("🎓 Registering Education ViewModels...");
            services.AddTransient<CoursesViewModel>();
            services.AddTransient<AssignmentsViewModel>();
            services.AddTransient<GradesViewModel>();
            services.AddTransient<TeachersViewModel>();
            services.AddTransient<GroupsViewModel>();
            services.AddTransient<StudentsViewModel>();
            services.AddTransient<SubjectsViewModel>();
            services.AddTransient<ExamsViewModel>();
            services.AddTransient<ScheduleViewModel>();
            services.AddTransient<CurriculumViewModel>();

            // Register Editor ViewModels
            Console.WriteLine("✏️ Registering Editor ViewModels...");
            services.AddTransient<TeacherEditorViewModel>();
            services.AddTransient<StudentEditorViewModel>();
            services.AddTransient<CourseEditorViewModel>();
            services.AddTransient<GroupEditorViewModel>();
            services.AddTransient<SubjectEditorViewModel>();
            services.AddTransient<GradeEditorViewModel>();
            services.AddTransient<AssignmentEditorViewModel>();

            // Register System ViewModels
            Console.WriteLine("🏢 Registering System ViewModels...");
            services.AddTransient<LibraryViewModel>();
            services.AddTransient<DepartmentsViewModel>();
            services.AddTransient<DepartmentViewModel>();

            // Register ReactiveUI ViewLocator
            Console.WriteLine("👁️ Registering ViewLocator...");
            services.AddSingleton<IViewLocator>(_ => new ReactiveViewLocator());

            // Register Navigation Service
            Console.WriteLine("🧭 Registering NavigationService...");
            services.AddSingleton<IUnifiedNavigationService, UnifiedNavigationService>();

            Console.WriteLine("🏗️ Building service provider...");
            _services = services.BuildServiceProvider();

            Console.WriteLine("📊 Initializing StatusLogger...");
            StatusLogger.Initialize(_services!);
            
            StatusLogger.LogInfo("Application services configured successfully", "ApplicationBootstrapper");
        }
        catch (Exception ex)
        {
            StatusLogger.LogInfo($"Error during Initialize: {ex.Message}", "ApplicationBootstrapper");
            StatusLogger.LogInfo($"Stack trace: {ex.StackTrace}", "ApplicationBootstrapper");

            throw;
        }
    }
}


/// <summary>
/// Единый локатор представлений для ReactiveUI
/// Автоматически связывает ViewModels с соответствующими Views
/// </summary>
public class ReactiveViewLocator : IViewLocator
{
    public IViewFor? ResolveView<T>(T? viewModel, string? contract = null) => viewModel switch
    {
        // Auth ViewModels
        LoginViewModel => new LoginView(),
        RegisterViewModel => new RegisterView(),
        AuthenticationViewModel => new AuthenticationView(),

        // Profile ViewModels
        ProfileViewModel => new ProfileView(),

        // Main ViewModels
        MainViewModel => new MainWindow(),
        HomeViewModel => new HomeView(),

        // Education ViewModels - Main Views
        CoursesViewModel => new CoursesView(),
        AssignmentsViewModel => new AssignmentsView(),
        GradesViewModel => new GradesView(),
        TeachersViewModel => new TeachersView(),
        GroupsViewModel => new GroupsView(),
        StudentsViewModel => new StudentsView(),
        SubjectsViewModel => new SubjectsView(),

        // NEW: Education ViewModels - Advanced Views
        ExamsViewModel => new ExamsView(),
        ScheduleViewModel => new ScheduleView(),
        CurriculumViewModel => new CurriculumView(),
        AcademicPeriodViewModel => new AcademicPeriodView(),

        // Education ViewModels - Editor Views (for navigation)
        TeacherEditorViewModel => new TeacherEditorView(),
        StudentEditorViewModel => new StudentEditorView(),
        CourseEditorViewModel => new CourseEditorView(),
        GroupEditorViewModel => new GroupEditorView(),
        SubjectEditorViewModel => new SubjectEditorView(),
        GradeEditorViewModel => new GradeEditorView(),
        AssignmentEditorViewModel => new AssignmentEditorView(),

        // System ViewModels
        LibraryViewModel => new LibraryView(),
        DepartmentsViewModel => new DepartmentsView(),
        // NotificationCenterViewModel => new NotificationCenterView(), // TODO: Create NotificationCenterView

        // Notification ViewModels
        // NotificationCenterViewModel => new NotificationCenterView(), // TODO: Create NotificationCenterView

        // Fallback - создаем простой UserControl с TextBlock
        _ => new FallbackView { DataContext = viewModel }, 
    };
}

/// <summary>
/// Fallback view для неизвестных ViewModels
/// </summary>
public class FallbackView : UserControl, IViewFor
{
    public FallbackView()
    {
        Content = new TextBlock
        {
            Text = "Представление не найдено",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
    }

    public object? ViewModel
    {
        get => DataContext;
        set => DataContext = value;
    }
}
