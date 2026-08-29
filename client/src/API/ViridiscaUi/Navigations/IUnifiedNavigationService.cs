namespace ViridiscaUi.Navigations;

/// <summary>
/// Единый сервис навигации, объединяющий всю функциональность
/// </summary>
public interface IUnifiedNavigationService
{
    // Свойства
    IReadOnlyList<NavigationRoute> Routes { get; }
    bool CanGoBack { get; }

    /// <summary>
    /// Реальный IScreen (MainViewModel), переданный через Initialize() — нужен всем
    /// местам, которые создают RoutableViewModelBase-наследники вручную (диалоги,
    /// редакторы), поскольку IScreen не зарегистрирован в DI-контейнере напрямую.
    /// </summary>
    IScreen? CurrentScreen { get; }

    // Основная навигация
    Task<bool> NavigateToAsync(string path);
    Task<bool> NavigateToAsync<TViewModel>() where TViewModel : class, IRoutableViewModel;
    Task<bool> NavigateAndResetAsync(string path);
    Task<bool> NavigateAndResetAsync<TViewModel>() where TViewModel : class, IRoutableViewModel;
    Task<bool> GoBackAsync();
    void ClearNavigationStack();

    // Работа с маршрутами
    NavigationRoute? GetRoute(string path);
    NavigationRoute? GetRoute<TViewModel>() where TViewModel : class, IRoutableViewModel;
    IEnumerable<NavigationRoute> GetMenuRoutes(string[]? userRoles = null);
    IEnumerable<NavigationRoute> GetChildRoutes(string parentPath, string[]? userRoles = null);
    IEnumerable<NavigationRoute> GetRoutesByGroup(string group, string[]? userRoles = null);
    IEnumerable<NavigationRoute> GetRoutesByTags(string[] tags, string[]? userRoles = null);
     
    // Инициализация
    void Initialize(IScreen screen);
    void ScanAndRegisterRoutes();
}