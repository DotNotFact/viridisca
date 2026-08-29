using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.ViewModels;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Auth;

/// <summary>
/// ViewModel для страницы входа в систему
/// </summary>
[Route("login", DisplayName = "Вход", IconKey = "🔑", Order = 1, ShowInMenu = false)]
public class LoginViewModel : RoutableViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IUnifiedNavigationService _navigationService;
    private readonly IPersonSessionService _personSessionService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// URL-сегмент для навигации
    /// </summary>
    

    /// <summary>
    /// Имя пользователя (логин)
    /// </summary>
    [Reactive]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Пароль пользователя
    /// </summary>
    [Reactive]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Флаг, указывающий на процесс входа
    /// </summary>
    [Reactive]
    public bool IsLoggingIn { get; set; } = false;

    /// <summary>
    /// Команда для входа в систему
    /// </summary>
    public ReactiveCommand<Unit, Unit> LoginCommand { get; }
    
    /// <summary>
    /// Команда для перехода на страницу регистрации
    /// </summary>
    public ReactiveCommand<Unit, Unit> GoToRegisterCommand { get; }

    /// <summary>
    /// Создает новый экземпляр ViewModel для входа в систему
    /// </summary>
    /// <param name="authService">Сервис аутентификации</param>
    /// <param name="navigationService">Сервис навигации</param>
    /// <param name="hostScreen">Родительский экран</param>
    /// <param name="viewModelFactory">Фабрика ViewModel</param>
    /// <param name="personSessionService">Сервис сессии пользователя</param>
    /// <param name="notificationService">Сервис уведомлений</param>
    public LoginViewModel(
        IScreen hostScreen,
        IAuthService authService,
        IUnifiedNavigationService navigationService,
        IPersonSessionService personSessionService,
        INotificationService notificationService) 
        : base(hostScreen)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _personSessionService = personSessionService ?? throw new ArgumentNullException(nameof(personSessionService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        // Проверка возможности выполнения команды входа
        var canLogin = this.WhenAnyValue(
            x => x.Username,
            x => x.Password,
            x => x.IsLoggingIn,
            (username, password, isLoggingIn) =>
                !string.IsNullOrWhiteSpace(username) &&
                !string.IsNullOrWhiteSpace(password) &&
                !isLoggingIn
        );

        // Создание команды входа
        LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync, canLogin);
        
        // Команда для перехода на страницу регистрации
        GoToRegisterCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await _navigationService.NavigateToAsync("register");
        });
    }

    /// <summary>
    /// Асинхронный метод для выполнения входа в систему
    /// </summary>
    private async Task LoginAsync()
    {
        try
        {
            IsLoggingIn = true;
            ClearError();

            var result = await _authService.AuthenticateAsync(Username, Password);

            if (result.Success)
            {
                // Если вход успешный, перейти на домашнюю страницу или по умолчанию
                await _navigationService.NavigateToAsync("home");
            }
            else
            {
                SetError(result.ErrorMessage ?? "Ошибка входа в систему");
            }
        }
        catch (Exception ex)
        {
            SetError($"Ошибка входа: {ex.Message}", ex);
        }
        finally
        {
            IsLoggingIn = false;
        }
    }
}

