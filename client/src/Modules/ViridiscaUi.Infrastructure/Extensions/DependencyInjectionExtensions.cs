using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Services.Auth;
// Domain service interfaces
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.File;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.Statistic;
using ViridiscaUi.Domain.Services.System;
using ViridiscaUi.Infrastructure.ApiClient;
using ViridiscaUi.Infrastructure.Data;
using ViridiscaUi.Infrastructure.Services;
using ViridiscaUi.Services;

namespace ViridiscaUi.Infrastructure.Extensions;

/// <summary>
/// Расширения для регистрации сервисов в DI контейнере
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Регистрирует все сервисы ViridiscaUi в DI контейнере
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="configuration">Конфигурация приложения</param>
    /// <returns>Коллекция сервисов для цепочки вызовов</returns>
    public static IServiceCollection AddViridiscaServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddPostgreSqlDatabase(configuration);

        // API clients (HTTP, replace direct EF access — see PROGRESS.md Phase 1/Phase 2)
        services.AddAcademicApiClients(configuration);

        // Core services
        services.AddServices();

        // Logging
        services.AddLogging();

        return services;
    }

    private static IServiceCollection AddAcademicApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        string apiBaseUrl = configuration["Api:BaseUrl"]
            ?? throw new InvalidOperationException("No API base URL found in configuration (Api:BaseUrl)");

        services.AddSingleton<IAuthTokenStore, AuthTokenStore>();
        services.AddTransient<AuthorizationHandler>();

        services.AddApiClient<IdentityApiClient>(apiBaseUrl);
        services.AddApiClient<StudentApiClient>(apiBaseUrl);
        services.AddApiClient<TeacherApiClient>(apiBaseUrl);
        services.AddApiClient<GroupApiClient>(apiBaseUrl);
        services.AddApiClient<SubjectApiClient>(apiBaseUrl);
        services.AddApiClient<GradeApiClient>(apiBaseUrl);
        services.AddApiClient<AcademicPeriodApiClient>(apiBaseUrl);
        services.AddApiClient<CourseInstanceApiClient>(apiBaseUrl);
        services.AddApiClient<AssignmentApiClient>(apiBaseUrl);
        services.AddApiClient<SubmissionApiClient>(apiBaseUrl);
        services.AddApiClient<ScheduleSlotApiClient>(apiBaseUrl);
        services.AddApiClient<NotificationApiClient>(apiBaseUrl);

        return services;
    }

    private static IServiceCollection AddApiClient<TClient>(this IServiceCollection services, string baseUrl) where TClient : class
    {
        services.AddHttpClient<TClient>(client => client.BaseAddress = new Uri(baseUrl))
            .AddHttpMessageHandler<AuthorizationHandler>();

        return services;
    }

    private static IServiceCollection AddLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Education services - Transient, not Scoped, for every EF-backed service below.
        // This app resolves everything from a single long-lived DI scope for the whole app
        // lifetime (no per-request/per-page scope), so "Scoped" here really means "one shared
        // instance forever" - and since ApplicationDbContext is Transient (see
        // AddPostgreSqlDatabase), a Scoped service captures ONE DbContext at its own
        // construction and reuses it for every call for the rest of the app's life. Two pages
        // loading concurrently (e.g. the dashboard and the home page both counting students at
        // startup) then race on that shared instance and throw "A second operation was started
        // on this context instance before a previous operation completed." Transient services
        // get a fresh instance (and therefore a fresh DbContext) every time the container
        // resolves them - i.e. once per ViewModel, since ViewModels are constructed fresh per
        // navigation via ActivatorUtilities.CreateInstance.
        // Concrete EF-backed classes stay registered under their own type — the Http*
        // decorators below take them as "inner" for operations the backend Academic API
        // doesn't cover yet (grades, attendance, analytics, curricula...). See PROGRESS.md
        // Phase 2. IStudentService/etc. now resolve to the HTTP-backed decorators.
        services.AddTransient<StudentService>();
        services.AddTransient<IStudentService, HttpStudentService>();
        services.AddTransient<GroupService>();
        services.AddTransient<IGroupService, HttpGroupService>();
        services.AddTransient<TeacherService>();
        services.AddTransient<ITeacherService, HttpTeacherService>();
        services.AddTransient<CourseInstanceService>();
        services.AddTransient<ICourseInstanceService, HttpCourseInstanceService>();
        services.AddTransient<AssignmentService>();
        services.AddTransient<IAssignmentService, HttpAssignmentService>();
        services.AddTransient<SubmissionService>();
        services.AddTransient<ISubmissionService, HttpSubmissionService>();
        services.AddTransient<GradeService>();
        services.AddTransient<IGradeService, HttpGradeService>();

        services.AddTransient<SubjectService>();
        services.AddTransient<ISubjectService, HttpSubjectService>();
        services.AddTransient<IDepartmentService, DepartmentService>();
        services.AddTransient<IEnrollmentService, EnrollmentService>();
        // No implementation existed anywhere before - GroupDialogViewModel, StudentDialogViewModel,
        // and the routed CurriculumViewModel page all depend on this and threw a DI resolution
        // failure. See CurriculumService.cs remarks.
        services.AddTransient<ICurriculumService, CurriculumService>();
        // services.AddScoped<ILessonService, LessonService>();
        // services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddTransient<ICourseService, CourseService>();
        services.AddTransient<AcademicPeriodService>();
        services.AddTransient<IAcademicPeriodService, HttpAcademicPeriodService>();
        services.AddTransient<ScheduleSlotService>();
        services.AddTransient<IScheduleSlotService, HttpScheduleSlotService>();
        services.AddTransient<ILibraryService, LibraryService>();
        // No implementation existed anywhere before - ExamsViewModel (routed "exams" page)
        // depends on this and threw a DI resolution failure on every navigation. No backend
        // Exam API client exists yet, so this is EF-only (same pattern as ICurriculumService/
        // ILibraryService above), not a Http-decorated pair like Assignment/ScheduleSlot.
        services.AddTransient<IExamService, ExamService>();

        // Auth services
        // IMPORTANT: Register IPasswordHashingService BEFORE IAuthService
        services.AddTransient<IPasswordHashingService, PasswordHashingService>();
        services.AddTransient<IPersonService, PersonService>();
        services.AddTransient<IRoleService, RoleService>();
        services.AddTransient<IPermissionService, PermissionService>();
        services.AddTransient<IRolePermissionService, RolePermissionService>();
        // HTTP-backed implementation, calls the backend's Identity API — see PROGRESS.md
        // Phase 1. The EF-based AuthService class is kept in the tree but unused for now.
        // Stays Scoped: HttpAuthService/PersonSessionService hold the login session for the
        // app's lifetime, which is the one case here where a single shared instance is wanted.
        services.AddScoped<IAuthService, HttpAuthService>();

        // Register Lazy<IAuthService> for circular dependency resolution
        services.AddScoped<Lazy<IAuthService>>(provider => new Lazy<IAuthService>(() => provider.GetRequiredService<IAuthService>()));

        // System services
        services.AddTransient<NotificationService>();
        services.AddTransient<INotificationService, HttpNotificationService>();
        services.AddTransient<IFileService, FileService>();
        services.AddTransient<IStatisticsService, StatisticsService>();
        services.AddTransient<IStatusService, StatusService>();
        services.AddSingleton<IPersonSessionService, PersonSessionService>();
        services.AddSingleton<IReactivePersonSessionService>(provider =>
            (IReactivePersonSessionService)provider.GetRequiredService<IPersonSessionService>());
        services.AddTransient<IExportService, ExportService>();
        services.AddTransient<IImportService, ImportService>();

        // UI Services - Note: DialogService should be registered in the UI layer, not here
        // This registration will be moved to the UI project's DI configuration

        return services;
    }

    /// <summary>
    /// Добавляет PostgreSQL DbContext в DI контейнер
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="configuration">Конфигурация приложения</param>
    /// <returns>Коллекция сервисов для цепочки вызовов</returns>
    public static IServiceCollection AddPostgreSqlDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            var connectionString = configuration.GetConnectionString("PostgreSQL")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No database connection string found in configuration");

            // Transient, not the default Scoped: this desktop app resolves everything from a single
            // long-lived DI scope for the whole app lifetime (no per-request/per-page scope), so a
            // Scoped DbContext is really a singleton in practice - every EF-backed service across the
            // entire app would share the exact same instance, and any two of them touched concurrently
            // (e.g. two pages loading stats at startup) throw "A second operation was started on this
            // context instance before a previous operation completed." Transient gives every
            // constructor injection its own instance; Npgsql pools the underlying physical connections,
            // so this doesn't multiply real connections.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly("ViridiscaUi.Infrastructure");
                })
                .UseSnakeCaseNamingConvention()
                .EnableSensitiveDataLogging(configuration.GetValue("Logging:EnableSensitiveDataLogging", false))
                .EnableDetailedErrors(configuration.GetValue("Logging:EnableDetailedErrors", false));
            }, ServiceLifetime.Transient, ServiceLifetime.Transient);

            return services;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database configuration error: {ex.Message}");
            throw;
        }
    }
}