using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.ViewModels;
using ViridiscaUi.ViewModels.System;

namespace ViridiscaUi.Navigations;

/// <summary>
/// Реализация единого сервиса навигации
/// </summary>
public class UnifiedNavigationService(IServiceProvider serviceProvider) : IUnifiedNavigationService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly List<NavigationRoute> _routes = [];
    private IScreen? _screen;

    public IReadOnlyList<NavigationRoute> Routes => _routes.AsReadOnly();
    public bool CanGoBack => _screen?.Router.NavigationStack.Count > 1;
    public IScreen? CurrentScreen => _screen;

    public void Initialize(IScreen screen)
    {
        _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        StatusLogger.LogDebug("Navigation service initialized", "UnifiedNavigation");
    }

    public void ScanAndRegisterRoutes()
    {
        StatusLogger.LogInfo("Сканирование маршрутов...", "UnifiedNavigation");

        var assemblies = new[]
        {
            Assembly.GetExecutingAssembly(),
            Assembly.GetEntryAssembly()
        }.Where(a => a != null).Distinct();

        var registeredCount = 0;

        foreach (var assembly in assemblies)
        {
            var viewModelTypes = assembly.GetTypes().Where(type => type.IsClass
                && !type.IsAbstract
                && typeof(IRoutableViewModel).IsAssignableFrom(type)
                && type.GetCustomAttribute<RouteAttribute>() != null);

            foreach (var viewModelType in viewModelTypes)
            {
                try
                {
                    RegisterRoute(viewModelType);
                    registeredCount++;
                }
                catch (Exception ex)
                {
                    StatusLogger.LogWarning($"Не удалось зарегистрировать маршрут для {viewModelType.Name}: {ex.Message}", "UnifiedNavigation");
                }
            }
        }

        StatusLogger.LogSuccess($"Зарегистрировано {registeredCount} маршрутов", "UnifiedNavigation");
    }

    private void RegisterRoute(Type viewModelType)
    {
        var routeAttribute = viewModelType.GetCustomAttribute<RouteAttribute>();

        if (routeAttribute is null)
            return;

        // Проверяем дублирование
        if (_routes.Any(r => r.Path.Equals(routeAttribute.Path, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Маршрут '{routeAttribute.Path}' уже зарегистрирован");
        }

        var displayName = routeAttribute.DisplayName ?? viewModelType.Name.Replace("ViewModel", "");

        var route = new NavigationRoute(
            routeAttribute.Path,
            viewModelType,
            displayName,
            routeAttribute.IconKey,
            routeAttribute.Order,
            routeAttribute.Group,
            routeAttribute.RequiredRoles,
            routeAttribute.ShowInMenu,
            routeAttribute.ParentRoute,
            routeAttribute.Description,
            routeAttribute.Shortcut,
            routeAttribute.Tags,
            routeAttribute.IsBeta,
            routeAttribute.RequiresConfirmation);

        _routes.Add(route);
        StatusLogger.LogDebug($"Зарегистрирован маршрут: {route.Path} -> {route.ViewModelType.Name}", "UnifiedNavigation");
    }

    public async Task<bool> NavigateToAsync(string path)
    {
        try
        {
            var route = GetRoute(path);
            if (route == null)
            {
                StatusLogger.LogError($"Маршрут '{path}' не найден", "UnifiedNavigation");
                return false;
            }
            
            var command = CreateNavigationCommand(path);
            await command.Execute();

            StatusLogger.LogInfo($"Переход к {path}", "UnifiedNavigation");
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка навигации к {path}: {ex.Message}", "UnifiedNavigation");
            return false;
        }
    }

    public async Task<bool> NavigateToAsync<TViewModel>() where TViewModel : class, IRoutableViewModel
    {
        try
        {
            var command = CreateNavigationCommand<TViewModel>();
            await command.Execute();

            StatusLogger.LogInfo($"Переход к {typeof(TViewModel).Name}", "UnifiedNavigation");
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка навигации к {typeof(TViewModel).Name}: {ex.Message}", "UnifiedNavigation");
            return false;
        }
    }

    public async Task<bool> NavigateAndResetAsync(string path)
    {
        try
        {
            _screen?.Router.NavigationStack.Clear();
            return await NavigateToAsync(path);
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка навигации с сбросом к {path}: {ex.Message}", "UnifiedNavigation");
            return false;
        }
    }

    public async Task<bool> NavigateAndResetAsync<TViewModel>() where TViewModel : class, IRoutableViewModel
    {
        try
        {
            _screen?.Router.NavigationStack.Clear();
            return await NavigateToAsync<TViewModel>();
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка навигации с сбросом к {typeof(TViewModel).Name}: {ex.Message}", "UnifiedNavigation");
            return false;
        }
    }

    public async Task<bool> GoBackAsync()
    {
        try
        {
            if (!CanGoBack)
            {
                StatusLogger.LogWarning("Невозможно вернуться назад - это первая страница", "UnifiedNavigation");
                return false;
            }

            await _screen!.Router.NavigateBack.Execute();
            StatusLogger.LogInfo("Возврат назад", "UnifiedNavigation");
            return true;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка возврата назад: {ex.Message}", "UnifiedNavigation");
            return false;
        }
    }

    public void ClearNavigationStack()
    {
        _screen?.Router.NavigationStack.Clear();
        StatusLogger.LogInfo("Стек навигации очищен", "UnifiedNavigation");
    }

    public NavigationRoute? GetRoute(string path)
    {
        return _routes.FirstOrDefault(r => r.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
    }

    public NavigationRoute? GetRoute<TViewModel>() where TViewModel : class, IRoutableViewModel
    {
        return _routes.FirstOrDefault(r => r.ViewModelType == typeof(TViewModel));
    }

    public IEnumerable<NavigationRoute> GetMenuRoutes(string[]? userRoles = null)
    {
        var menuRoutes = _routes.Where(r => r.ShowInMenu);

        if (userRoles != null && userRoles.Length > 0)
        {
            var filteredRoutes = menuRoutes.Where(r => r.RequiredRoles.Length == 0 || r.RequiredRoles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase)));

            menuRoutes = filteredRoutes;
        }

        var result = menuRoutes.OrderBy(r => r.Order).ThenBy(r => r.DisplayName);

        return result;
    }

    public IEnumerable<NavigationRoute> GetChildRoutes(string parentPath, string[]? userRoles = null)
    {
        var childRoutes = _routes.Where(r =>
            !string.IsNullOrEmpty(r.ParentRoute) &&
            r.ParentRoute.Equals(parentPath, StringComparison.OrdinalIgnoreCase));

        if (userRoles != null && userRoles.Length > 0)
        {
            childRoutes = childRoutes.Where(r => r.RequiredRoles.Length == 0 || r.RequiredRoles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase)));
        }

        return childRoutes.OrderBy(r => r.Order).ThenBy(r => r.DisplayName);
    }

    public IEnumerable<NavigationRoute> GetRoutesByGroup(string group, string[]? userRoles = null)
    {
        var groupRoutes = _routes.Where(r =>
            !string.IsNullOrEmpty(r.Group) &&
            r.Group.Equals(group, StringComparison.OrdinalIgnoreCase));

        if (userRoles != null && userRoles.Length > 0)
        {
            groupRoutes = groupRoutes.Where(r => r.RequiredRoles.Length == 0 || r.RequiredRoles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase)));
        }

        return groupRoutes.OrderBy(r => r.Order).ThenBy(r => r.DisplayName);
    }

    public IEnumerable<NavigationRoute> GetRoutesByTags(string[] tags, string[]? userRoles = null)
    {
        var taggedRoutes = _routes.Where(r => r.Tags.Length > 0 && r.Tags.Any(tag => tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));

        if (userRoles != null && userRoles.Length > 0)
        {
            taggedRoutes = taggedRoutes.Where(r => r.RequiredRoles.Length == 0 || r.RequiredRoles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase)));
        }

        return taggedRoutes.OrderBy(r => r.Order).ThenBy(r => r.DisplayName);
    }

    public ReactiveCommand<Unit, IRoutableViewModel> CreateNavigationCommand(string path)
    {
        if (_screen == null)
        {
            throw new InvalidOperationException("Navigation service not initialized. Call Initialize() first.");
        }

        var command = ReactiveCommand.CreateFromObservable(() =>
        {
            var route = GetRoute(path);
            if (route == null)
            {
                StatusLogger.LogError($"Маршрут '{path}' не найден", "UnifiedNavigation");
                return Observable.Empty<IRoutableViewModel>();
            }

            try
            {
                IRoutableViewModel viewModel;
                
                if (route.ViewModelType == typeof(AuthenticationViewModel))
                {
                    var authService = _serviceProvider.GetRequiredService<IAuthService>();
                    var navigationService = _serviceProvider.GetRequiredService<IUnifiedNavigationService>();
                    var roleService = _serviceProvider.GetRequiredService<IRoleService>();
                    var personSessionService = _serviceProvider.GetRequiredService<IPersonSessionService>();
                    
                    viewModel = new AuthenticationViewModel(authService, navigationService, roleService, personSessionService, _screen);
                }
                else if (route.ViewModelType == typeof(HomeViewModel))
                {
                    var personService = _serviceProvider.GetRequiredService<IPersonService>();
                    var authService = _serviceProvider.GetRequiredService<IAuthService>();
                    var notificationService = _serviceProvider.GetRequiredService<INotificationService>();
                    var scheduleSlotService = _serviceProvider.GetRequiredService<IScheduleSlotService>();
                    var studentService = _serviceProvider.GetRequiredService<IStudentService>();
                    var teacherService = _serviceProvider.GetRequiredService<ITeacherService>();
                    var courseInstanceService = _serviceProvider.GetRequiredService<ICourseInstanceService>();
                    var enrollmentService = _serviceProvider.GetRequiredService<IEnrollmentService>();
                    var gradeService = _serviceProvider.GetRequiredService<IGradeService>();
                    
                    viewModel = new HomeViewModel(_screen, personService, authService, notificationService, 
                        scheduleSlotService, studentService, teacherService, courseInstanceService, 
                        enrollmentService, gradeService);
                }
                else if (route.ViewModelType == typeof(ProfileViewModel))
                {
                    var personService = _serviceProvider.GetRequiredService<IPersonService>();
                    var authService = _serviceProvider.GetRequiredService<IAuthService>();
                    var notificationService = _serviceProvider.GetRequiredService<INotificationService>();
                    var dialogService = _serviceProvider.GetRequiredService<IDialogService>();
                    var personSessionService = _serviceProvider.GetRequiredService<IPersonSessionService>();
                    var navigationService = _serviceProvider.GetRequiredService<IUnifiedNavigationService>();
                    
                    viewModel = new ProfileViewModel(_screen, personService, authService, notificationService, dialogService, personSessionService, navigationService);
                }
                else
                {
                    // Every other routable ViewModel takes IScreen hostScreen as a constructor
                    // parameter, but IScreen is never registered in the DI container (MainViewModel
                    // is the real IScreen, wired in via Initialize() below, not through DI) - a plain
                    // GetRequiredService(route.ViewModelType) throws "No service for type 'IScreen'"
                    // for every route except the three hard-coded above, caught by the handler below
                    // and silently swallowed, which is why most of the app never navigated. Supplying
                    // _screen explicitly here and letting ActivatorUtilities resolve the rest from DI
                    // fixes every route at once instead of hand-rolling a special case per ViewModel.
                    viewModel = (IRoutableViewModel)ActivatorUtilities.CreateInstance(_serviceProvider, route.ViewModelType, _screen);
                }

                return _screen.Router.Navigate.Execute(viewModel)
                    .Catch<IRoutableViewModel, Exception>(ex =>
                    {
                        StatusLogger.LogError($"Ошибка навигации к маршруту '{path}': {ex.Message}", "UnifiedNavigation");
                        return Observable.Empty<IRoutableViewModel>();
                    });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("No service for type"))
            {
                StatusLogger.LogError($"ViewModel типа {route.ViewModelType.Name} не зарегистрирован в DI контейнере для маршрута '{path}': {ex.Message}", "UnifiedNavigation");
                return Observable.Empty<IRoutableViewModel>();
            }
            catch (Exception ex)
            {
                StatusLogger.LogError($"Ошибка создания ViewModel для маршрута '{path}': {ex}", "UnifiedNavigation");
                return Observable.Empty<IRoutableViewModel>();
            }
        });

        command.ThrownExceptions
            .Subscribe(ex =>
            {
                StatusLogger.LogError($"Необработанная ошибка в команде навигации к '{path}': {ex.Message}", "UnifiedNavigation");
            });

        return command;
    }

    public ReactiveCommand<Unit, IRoutableViewModel> CreateNavigationCommand<TViewModel>()
        where TViewModel : class, IRoutableViewModel
    {
        if (_screen == null)
            throw new InvalidOperationException("Navigation service not initialized. Call Initialize() first.");

        var command = ReactiveCommand.CreateFromObservable(() =>
        {
            try
            {
                var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

                return _screen.Router.Navigate.Execute(viewModel)
                    .Catch<IRoutableViewModel, Exception>(ex =>
                    {
                        StatusLogger.LogError($"Ошибка навигации к {typeof(TViewModel).Name}: {ex.Message}", "UnifiedNavigation");
                        return Observable.Empty<IRoutableViewModel>();
                    });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("No service for type"))
            {
                StatusLogger.LogError($"ViewModel типа {typeof(TViewModel).Name} не зарегистрирован в DI контейнере: {ex.Message}", "UnifiedNavigation");
                return Observable.Empty<IRoutableViewModel>();
            }
            catch (Exception ex)
            {
                StatusLogger.LogError($"Ошибка создания {typeof(TViewModel).Name}: {ex.Message}", "UnifiedNavigation");
                return Observable.Empty<IRoutableViewModel>();
            }
        });

        command.ThrownExceptions
            .Subscribe(ex =>
            {
                StatusLogger.LogError($"Необработанная ошибка в команде навигации к {typeof(TViewModel).Name}: {ex.Message}", "UnifiedNavigation");
            });

        return command;
    }
}