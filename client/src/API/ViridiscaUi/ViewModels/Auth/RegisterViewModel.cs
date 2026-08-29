using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.ViewModels.Auth
{
    /// <summary>
    /// ViewModel для страницы регистрации пользователя
    /// </summary>
    [Route("register", DisplayName = "Регистрация", IconKey = "📝", Order = 2, ShowInMenu = false)]
    public class RegisterViewModel : RoutableViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IUnifiedNavigationService _navigationService;
        private readonly IRoleService _roleService;
        private readonly INotificationService _notificationService;
        private readonly IPersonSessionService _personSessionService;

        /// <summary>
        /// URL-сегмент для навигации
        /// </summary>
        

        /// <summary>
        /// Имя пользователя (логин)
        /// </summary>
        [Reactive]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Электронная почта
        /// </summary>
        [Reactive]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Имя пользователя
        /// </summary>
        [Reactive]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        [Reactive]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        [Reactive]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Подтверждение пароля
        /// </summary>
        [Reactive]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// Выбранная роль пользователя
        /// </summary>
        [Reactive]
        public Role? SelectedRole { get; set; }

        /// <summary>
        /// Список доступных ролей
        /// </summary>
        [Reactive]
        public ObservableCollection<Role> AvailableRoles { get; set; } = new();

        /// <summary>
        /// Флаг, указывающий на процесс регистрации
        /// </summary>
        [Reactive]
        public bool IsRegistering { get; set; } = false;

        /// <summary>
        /// Флаг загрузки ролей
        /// </summary>
        [Reactive]
        public bool IsLoadingRoles { get; set; } = false;

        /// <summary>
        /// Команда для регистрации пользователя
        /// </summary>
        public ReactiveCommand<Unit, Unit> RegisterCommand { get; private set; } = null!;

        /// <summary>
        /// Команда для перехода на страницу входа
        /// </summary>
        public ReactiveCommand<Unit, Unit> GoToLoginCommand { get; private set; } = null!;

        /// <summary>
        /// Создает новый экземпляр ViewModel для регистрации пользователя
        /// </summary>
        /// <param name="authService">Сервис аутентификации</param>
        /// <param name="navigationService">Сервис навигации</param>
        /// <param name="roleService">Сервис ролей</param>
        /// <param name="hostScreen">Родительский экран</param>
        /// <param name="viewModelFactory">Фабрика ViewModel</param>
        public RegisterViewModel(
            IScreen hostScreen, 
            IAuthService authService,
            IUnifiedNavigationService navigationService,
            IRoleService roleService,
            INotificationService notificationService,
            IPersonSessionService personSessionService) 
            : base(hostScreen)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _personSessionService = personSessionService ?? throw new ArgumentNullException(nameof(personSessionService));

            InitializeCommands();
            
            // Загружаем роли при создании ViewModel
            _ = LoadRolesAsync();
            
            LogInfo("RegisterViewModel initialized");
        }

        /// <summary>
        /// Инициализирует команды
        /// </summary>
        private void InitializeCommands()
        {
            // Проверка возможности выполнения команды регистрации
            var canRegister = this.WhenAnyValue(
                x => x.Username,
                x => x.Email,
                x => x.FirstName,
                x => x.LastName,
                x => x.Password,
                x => x.ConfirmPassword,
                x => x.SelectedRole,
                x => x.IsRegistering,
                (username, email, firstName, lastName, password, confirmPassword, selectedRole, isRegistering) =>
                    !string.IsNullOrWhiteSpace(username) &&
                    !string.IsNullOrWhiteSpace(email) &&
                    !string.IsNullOrWhiteSpace(firstName) &&
                    !string.IsNullOrWhiteSpace(lastName) &&
                    !string.IsNullOrWhiteSpace(password) &&
                    password == confirmPassword &&
                    selectedRole != null &&
                    !isRegistering
            );

            // Используем стандартизированные методы создания команд из ViewModelBase
            RegisterCommand = CreateCommand(RegisterAsync, canRegister, "Ошибка регистрации");
            GoToLoginCommand = CreateCommand(async () =>
            {
                await _navigationService.NavigateToAsync("login");
            }, null, "Ошибка навигации");
        }

        /// <summary>
        /// Загружает список доступных ролей
        /// </summary>
        private async Task LoadRolesAsync()
        {
            try
            {
                IsLoadingRoles = true;
                var roles = await _roleService.GetAllRolesAsync();
                
                AvailableRoles.Clear();
                foreach (var role in roles)
                {
                    AvailableRoles.Add(role);
                }

                // Устанавливаем роль студента по умолчанию
                SelectedRole = AvailableRoles.FirstOrDefault(r => r.Name == "Student");
            }
            catch (Exception ex)
            {
                SetError($"Ошибка загрузки ролей: {ex.Message}", ex);
            }
            finally
            {
                IsLoadingRoles = false;
            }
        }

        /// <summary>
        /// Асинхронный метод для выполнения регистрации
        /// </summary>
        private async Task RegisterAsync()
        {
            try
            {
                IsRegistering = true;
                ClearError();

                // Дополнительная проверка на совпадение паролей
                if (Password != ConfirmPassword)
                {
                    SetError("Пароли не совпадают");
                    return;
                }

                if (SelectedRole == null)
                {
                    SetError("Выберите роль");
                    return;
                }

                var result = await _authService.RegisterAsync(
                    Username, 
                    Email, 
                    Password, 
                    FirstName, 
                    LastName,
                    SelectedRole.Uid);

                if (result.Success)
                {
                    // Если регистрация успешная, перейти на страницу входа
                    await _navigationService.NavigateToAsync("login");
                }
                else
                {
                    SetError(result.ErrorMessage ?? "Ошибка регистрации");
                }
            }
            catch (Exception ex)
            {
                SetError($"Ошибка регистрации: {ex.Message}", ex);
            }
            finally
            {
                IsRegistering = false;
            }
        }
    }
}


