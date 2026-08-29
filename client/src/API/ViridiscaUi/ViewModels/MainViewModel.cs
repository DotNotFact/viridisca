using ViridiscaUi.ViewModels.Components;
using ViridiscaUi.ViewModels.Auth;
using ViridiscaUi.Models;
using ViridiscaUi.Infrastructure.Services;

namespace ViridiscaUi.ViewModels;

/// <summary>
/// Главная ViewModel приложения, управляющая навигацией и состоянием пользователя
/// </summary>
public class MainViewModel : ViewModelBase, IScreen, IDisposable
{
    private readonly IAuthService _authService;
    private readonly IReactivePersonSessionService _personSessionService;
    private readonly IStatusService _statusService;
    private readonly IUnifiedNavigationService _navigationService;
    private readonly IStatisticsService _statisticsService;
    private readonly INotificationService _notificationService;
    private readonly IViewLocator _viewLocator;

    private readonly CompositeDisposable _disposables = [];

    #region Properties

    /// <summary>
    /// RoutingState для управления навигацией
    /// </summary>
    public RoutingState Router { get; } = new RoutingState();

    /// <summary>
    /// ViewLocator для ReactiveUI навигации
    /// </summary>
    public IViewLocator ViewLocator { get; }

    /// <summary>
    /// StatusBar ViewModel
    /// </summary>
    public StatusBarViewModel StatusBar { get; private set; }

    /// <summary>
    /// Текущий аутентифицированный пользователь
    /// </summary>
    [Reactive] public string? CurrentUser { get; private set; }

    /// <summary>
    /// Полная информация о текущем пользователе
    /// </summary>
    [Reactive] public Models.ViewModels.CurrentUserInfo? CurrentUserInfo { get; private set; }

    /// <summary>
    /// Флаг, указывающий на то, что пользователь авторизован
    /// </summary>
    [Reactive] public bool IsLoggedIn { get; private set; }

    /// <summary>
    /// Роль текущего пользователя
    /// </summary>
    [Reactive] public string? UserRole { get; private set; }

    /// <summary>
    /// Инициалы пользователя для аватара
    /// </summary>
    [Reactive] public string UserInitials { get; private set; } = string.Empty;

    /// <summary>
    /// Флаг возможности навигации назад
    /// </summary>
    [Reactive] public bool CanGoBack { get; private set; }

    /// <summary>
    /// Пункты меню сгруппированные по секциям
    /// </summary>
    [Reactive] public ObservableCollection<MenuGroup> GroupedMenuItems { get; private set; } = new();

    /// <summary>
    /// Общее количество студентов
    /// </summary>
    [Reactive] public int TotalStudents { get; set; } = 0;

    /// <summary>
    /// Общее количество курсов
    /// </summary>
    [Reactive] public int TotalCourses { get; set; } = 0;

    /// <summary>
    /// Общее количество преподавателей
    /// </summary>
    [Reactive] public int TotalTeachers { get; set; } = 0;

    /// <summary>
    /// Общее количество заданий
    /// </summary>
    [Reactive] public int TotalAssignments { get; set; } = 0;

    /// <summary>
    /// Количество пользователей онлайн
    /// </summary>
    [Reactive] public int OnlineUsersCount { get; set; } = 1;

    /// <summary>
    /// Версия приложения
    /// </summary>
    [Reactive] public string AppVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Флаг загрузки статистики
    /// </summary>
    [Reactive] public bool IsLoadingStatistics { get; set; } = false;

    [Reactive] public Person? CurrentPerson { get; set; }

    [Reactive] public string PersonName { get; set; } = string.Empty;
    [Reactive] public string UserDisplayName { get; set; } = string.Empty;
    [Reactive] public string[] UserRoles { get; set; } = [];
    [Reactive] public Guid UserId { get; set; }

    #endregion

    #region Commands

    /// <summary>
    /// Команда для выхода из системы
    /// </summary>
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; private set; } = null!;

    /// <summary>
    /// Команда для возврата назад
    /// </summary>
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; private set; } = null!;

    /// <summary>
    /// Команда навигации к маршруту
    /// </summary>
    public ReactiveCommand<string, Unit> NavigateToRouteCommand { get; private set; } = null!;

    /// <summary>
    /// Команда открытия меню пользователя
    /// </summary>
    public ReactiveCommand<Unit, Unit> OpenUserMenuCommand { get; private set; } = null!;

    /// <summary>
    /// Команда быстрого действия
    /// </summary>
    public ReactiveCommand<Unit, Unit> QuickActionCommand { get; private set; } = null!;

    /// <summary>
    /// Команда обновления статистики
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshStatisticsCommand { get; private set; } = null!;

    #endregion

    public MainViewModel(
        IAuthService authService,
        IReactivePersonSessionService personSessionService,
        IStatusService statusService,
        IUnifiedNavigationService navigationService,
        IStatisticsService statisticsService,
        IViewLocator viewLocator,
        INotificationService notificationService)
    {
        StatusLogger.LogInfo($"Инициализация главной модели представления");

        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _personSessionService = personSessionService ?? throw new ArgumentNullException(nameof(personSessionService));
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        _viewLocator = viewLocator ?? throw new ArgumentNullException(nameof(viewLocator));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        // Инициализация Router для ReactiveUI навигации
        Router = new RoutingState();

        // Инициализация навигационного сервиса с текущим экраном
        _navigationService.Initialize(this);
        _navigationService.ScanAndRegisterRoutes();

        // Инициализация команд
        InitializeCommands();

        // Подписка на изменения сессии пользователя
        InitializeSubscriptions();

        // Начальная навигация
        InitializeNavigation();

        StatusLogger.LogInfo($"Главная модель представления инициализирована");
    }

    #region Private Methods

    private void InitializeSubscriptions()
    {
        StatusLogger.LogInfo("Настройка подписок на изменения сессии пользователя", "MainViewModel");
        
        // Subscribe to user changes - используем простую логику как в старом коде
        Observable.Return(_personSessionService.CurrentPerson)
            .Merge(_personSessionService.CurrentPersonObservable)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(person =>
            {
                StatusLogger.LogInfo($"Получено изменение сессии пользователя: {(person != null ? $"{person.FirstName} {person.LastName}" : "null")}", "MainViewModel");
                CurrentPerson = person;
                if (person != null)
                {
                    HandleUserLoggedIn(person);
                }
                else
                {
                    HandleUserLoggedOut();
                }
            })
            .DisposeWith(_disposables);
            
        StatusLogger.LogInfo("Подписки на изменения сессии настроены", "MainViewModel");
    }

    private void InitializeCommands()
    {
        // Initialize status bar
        StatusBar = new StatusBarViewModel(_statusService, this);
        
        // Initialize collections
        GroupedMenuItems = [];
        
        // Load initial statistics
        LoadStatisticsAsync().ConfigureAwait(false);
    }

    private void HandleUserLoggedIn(Person person)
    {
        StatusLogger.LogInfo($"Обработка входа пользователя: {person.FirstName} {person.LastName}", "MainViewModel");
        
        // Set IsLoggedIn to true to show sidebar
        IsLoggedIn = true;
        StatusLogger.LogInfo("IsLoggedIn установлен в true", "MainViewModel");
        
        // Set user information for UI
        CurrentUser = person.Email;
        UserInitials = GetUserInitials(person);
        UserRole = person.PersonRoles?.FirstOrDefault(pr => pr.IsActive)?.Role?.Name ?? "Unknown";
        
        // Create CurrentUserInfo for sidebar display
        CurrentUserInfo = new CurrentUserInfo
        {
            Uid = person.Uid,
            FirstName = person.FirstName,
            LastName = person.LastName,
            Email = person.Email,
            Role = person.PersonRoles?.FirstOrDefault(pr => pr.IsActive)?.Role?.Name ?? "Unknown",
            LastLoginAt = DateTime.UtcNow
        };
        
        StatusLogger.LogInfo($"Информация о пользователе установлена: {CurrentUser}, роль: {UserRole}", "MainViewModel");
        
        // Update menu based on user roles
        UpdateMenuItems(person);
        StatusLogger.LogInfo("Меню обновлено", "MainViewModel");
        
        // Navigate to appropriate default page
        NavigateToDefaultPage(person);
        
        // Load user-specific data
        LoadUserDataAsync(person);
        
        StatusLogger.LogInfo("Обработка входа пользователя завершена", "MainViewModel");
    }

    private void NavigateToDefaultPage(Person person)
    {
        try
        {
            StatusLogger.LogInfo("Начинаем навигацию на главную страницу после входа", "MainViewModel");
            
            // Navigate to home page by default after login
            _navigationService.NavigateToAsync("home");
            StatusLogger.LogInfo("Навигация на главную страницу инициирована", "MainViewModel");
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка навигации на главную страницу: {ex.Message}", "MainViewModel");
        }
    }

    private void HandleUserLoggedOut()
    {
        LogInfo("User logged out");
        
        // Set IsLoggedIn to false to hide sidebar
        IsLoggedIn = false;
        
        // Clear user information
        CurrentUser = null;
        CurrentUserInfo = null;
        UserRole = null;
        UserInitials = string.Empty;
        CurrentPerson = null;
        
        ShowInfo("Вы вышли из системы");

        var currentViewModel = Router.GetCurrentViewModel();

        if (currentViewModel is not AuthenticationViewModel)
        {
            try
            {
                _navigationService.NavigateAndResetAsync("auth");
                LogInfo("Navigated to authentication screen after logout");
            }
            catch (Exception ex)
            {
                LogError($"Error navigating to auth after logout: {ex.Message}");
            }
        }
    }

    // Начальная навигация: если пользователь не авторизован и стек пуст, переходим на авторизацию
    private void InitializeNavigation()
    {
        if (_personSessionService.CurrentPerson == null && Router.NavigationStack.Count == 0)
        {
            try
            {
                _navigationService.NavigateToAsync("auth");
                LogInfo("Navigated to authentication screen - user not logged in");
            }
            catch (Exception ex)
            {
                LogError($"Error navigating to auth: {ex.Message}");
            }
        }
        else if (_personSessionService.CurrentPerson != null && Router.NavigationStack.Count == 0)
        {
            try
            {
                _navigationService.NavigateToAsync("home");
                LogInfo("Navigated to home screen - user already logged in");
            }
            catch (Exception ex)
            {
                LogError($"Error navigating to home: {ex.Message}");
            }
        }
    }

    private async Task ExecuteLogoutAsync()
    {
        await _authService.LogoutAsync();
        ShowInfo("Выход из системы выполнен");
    }

    private async void LoadStatistics()
    {
        await LoadStatisticsAsync();
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            IsLoadingStatistics = true;

            // Загружаем статистику параллельно
            var systemStats = await _statisticsService.GetSystemStatisticsAsync();

            if (systemStats != null)
            {
                TotalStudents = systemStats.TotalStudents;
                TotalCourses = systemStats.TotalCourses;
                TotalTeachers = systemStats.TotalTeachers;
                TotalAssignments = systemStats.TotalAssignments;
            }

            // Симуляция онлайн пользователей (в реальном приложении это будет из SignalR или другого источника)
            OnlineUsersCount = Random.Shared.Next(1, 25);

            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser != null)
            {
                dynamic dynamicUser = currentUser;
                UserId = (Guid)(dynamicUser.PersonUid ?? Guid.Empty);
                PersonName = $"{dynamicUser.FirstName ?? ""} {dynamicUser.LastName ?? ""}";
                UserDisplayName = PersonName;
                
                var roles = dynamicUser.Roles as List<string>;
                UserRoles = roles?.ToArray() ?? Array.Empty<string>();
                UserRole = UserRoles.FirstOrDefault() ?? "Роль не определена";
            }

            // Загружаем маршруты на основе ролей пользователя
            LoadMenuRoutes();
        }
        catch (Exception ex)
        {
            LogError($"Ошибка загрузки статистики: {ex.Message}");
            ShowError("Не удалось загрузить статистику");

            // Устанавливаем значения по умолчанию
            TotalStudents = 0;
            TotalTeachers = 0;
            TotalCourses = 0;
            TotalAssignments = 0;
            OnlineUsersCount = 1;
        }
        finally
        {
            IsLoadingStatistics = false;
        }
    }

    private string GetUserInitials(Person person)
    {
        if (person == null) return "??";
        
        var firstInitial = !string.IsNullOrEmpty(person.FirstName) ? person.FirstName[0].ToString().ToUpper() : "";
        var lastInitial = !string.IsNullOrEmpty(person.LastName) ? person.LastName[0].ToString().ToUpper() : "";
        
        return firstInitial + lastInitial;
    }

    private void UpdateMenuItems(Person? person)
    {
        var userRoles = person?.PersonRoles?.Select(pr => pr.Role?.Name).Where(r => r != null).ToArray() ?? new string[0];
        var menuRoutes = _navigationService.GetMenuRoutes(userRoles);

        // Группировка маршрутов
        var groupedMenuItems = menuRoutes
            .GroupBy(route => route.Group ?? "Основное")
            .OrderBy(group => group.Min(r => r.Order))
            .Select(group => new MenuGroup
            {
                GroupName = group.Key,
                Order = group.Min(r => r.Order),
                Items = new ObservableCollection<NavigationRoute>(
                    group.OrderBy(r => r.Order)
                         .ThenBy(r => r.DisplayName)
                         .Select(r =>
                         {
                             // Создание команды навигации для каждого маршрута
                             r.NavigateCommand = ReactiveCommand.CreateFromTask(async () =>
                             {
                                 await _navigationService.NavigateToAsync(r.Path);
                             });
                             return r;
                         }))
            });

        GroupedMenuItems.Clear();
        foreach (var group in groupedMenuItems)
        {
            GroupedMenuItems.Add(group);
        }
    }

    private async Task LoadUserDataAsync(Person person)
    {
        // Implementation of LoadUserDataAsync method
    }

    private async Task InitializeNavigationAsync()
    {
        // Временная заглушка для компиляции
        await Task.CompletedTask;
    }

    private async Task OpenUserMenuAsync()
    {
        try
        {
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                await _navigationService.NavigateToAsync("profile");
            }
            else
            {
                await _navigationService.NavigateToAsync("auth");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка открытия меню пользователя");
            ShowError("Ошибка открытия меню пользователя");
        }
    }

    private async Task ExecuteQuickActionAsync()
    {
        try
        {
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson != null)
            {
                // Быстрое действие - переход к созданию нового элемента
                await _navigationService.NavigateToAsync("students");
            }
            else
            {
                ShowError("Необходимо войти в систему для выполнения действий");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка выполнения быстрого действия");
            ShowError("Ошибка выполнения быстрого действия");
        }
    }

    private void LoadMenuRoutes()
    {
        try
        {
            // Получаем маршруты на основе ролей пользователя
            var routes = _navigationService.GetMenuRoutes(UserRoles);
            
            GroupedMenuItems.Clear();
            foreach (var route in routes)
            {
                GroupedMenuItems.Add(new MenuGroup
                {
                    GroupName = route.Group ?? "Основное",
                    Order = route.Order,
                    Items = new ObservableCollection<NavigationRoute> { route }
                });
            }
            
            LogInfo($"Loaded {GroupedMenuItems.Count} menu routes for user roles: {string.Join(", ", UserRoles)}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load menu routes");
        }
    }

    #endregion

    public override void Dispose()
    {
        _disposables?.Dispose();
        StatusBar?.Dispose();
        base.Dispose();
    }
}




