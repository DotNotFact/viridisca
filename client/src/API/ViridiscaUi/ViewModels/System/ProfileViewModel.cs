using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Auth;
using ViridiscaUi.Domain.Services;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.Navigations;
using ViridiscaUi.ViewModels.Bases;
using ViridiscaUi.ViewModels.Auth;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// ViewModel для страницы профиля пользователя
/// Современный дизайн с полной функциональностью
/// </summary>
[Route("profile", DisplayName = "Профиль", IconKey = "Account", Order = 10, Group = "Система", ShowInMenu = true)]
public class ProfileViewModel : RoutableViewModelBase
{
    private readonly IPersonService _personService;
    private readonly IAuthService _authService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService _dialogService;
    private readonly IPersonSessionService _personSessionService;
    private readonly IUnifiedNavigationService _navigationService;

    /// <summary>
    /// Сервис сессии пользователя
    /// </summary>
    protected IPersonSessionService PersonSessionService => _personSessionService;

    #region Personal Information Properties

    /// <summary>
    /// Имя пользователя
    /// </summary>
    [Reactive] public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    [Reactive] public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Отчество пользователя
    /// </summary>
    [Reactive] public string MiddleName { get; set; } = string.Empty;

    /// <summary>
    /// Дата рождения
    /// </summary>
    [Reactive] public DateTimeOffset? DateOfBirth { get; set; }

    /// <summary>
    /// Электронная почта
    /// </summary>
    [Reactive] public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Номер телефона
    /// </summary>
    [Reactive] public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// URL изображения профиля
    /// </summary>
    [Reactive] public string ProfileImageUrl { get; set; } = string.Empty;

    #endregion

    #region Password Change Properties

    /// <summary>
    /// Текущий пароль
    /// </summary>
    [Reactive] public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Новый пароль
    /// </summary>
    [Reactive] public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Подтверждение нового пароля
    /// </summary>
    [Reactive] public string ConfirmPassword { get; set; } = string.Empty;

    #endregion

    #region User Info Properties

    /// <summary>
    /// Роли пользователя
    /// </summary>
    [Reactive] public ObservableCollection<PersonRole> UserRoles { get; set; } = new();

    /// <summary>
    /// Дата регистрации
    /// </summary>
    [Reactive] public DateTime RegistrationDate { get; set; }

    /// <summary>
    /// Дата последнего входа
    /// </summary>
    [Reactive] public DateTime? LastLoginDate { get; set; }

    #endregion

    #region State Properties

    /// <summary>
    /// Флаг загрузки данных
    /// </summary>
    [Reactive] public bool IsLoading { get; set; }

    /// <summary>
    /// Флаг сохранения
    /// </summary>
    [Reactive] public bool IsSaving { get; set; }

    /// <summary>
    /// Флаг изменения пароля
    /// </summary>
    [Reactive] public bool IsChangingPassword { get; set; }

    /// <summary>
    /// Флаг загрузки фото
    /// </summary>
    [Reactive] public bool IsUploadingPhoto { get; set; }

    #endregion

    #region Commands

    /// <summary>
    /// Команда сохранения профиля
    /// </summary>
    public ReactiveCommand<Unit, Unit> SaveCommand { get; private set; } = null!;

    /// <summary>
    /// Команда отмены изменений
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; } = null!;

    /// <summary>
    /// Команда изменения пароля
    /// </summary>
    public ReactiveCommand<Unit, Unit> ChangePasswordCommand { get; private set; } = null!;

    /// <summary>
    /// Команда загрузки фото
    /// </summary>
    public ReactiveCommand<Unit, Unit> UploadPhotoCommand { get; private set; } = null!;

    /// <summary>
    /// Команда удаления фото
    /// </summary>
    public ReactiveCommand<Unit, Unit> RemovePhotoCommand { get; private set; } = null!;

    /// <summary>
    /// Команда обновления данных
    /// </summary>
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;

    /// <summary>
    /// Команда выхода из аккаунта
    /// </summary>
    public ReactiveCommand<Unit, Unit> LogoutCommand { get; private set; } = null!;

    /// <summary>
    /// Команда удаления профиля
    /// </summary>
    public ReactiveCommand<Unit, Unit> DeleteProfileCommand { get; private set; } = null!;

    #endregion

    public ProfileViewModel(
        IScreen hostScreen,
        IPersonService personService,
        IAuthService authService,
        INotificationService notificationService,
        IDialogService dialogService,
        IPersonSessionService personSessionService,
        IUnifiedNavigationService navigationService) : base(hostScreen)
    {
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _personSessionService = personSessionService ?? throw new ArgumentNullException(nameof(personSessionService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        InitializeCommands();
        
        // Загружаем данные сразу при создании ViewModel
        _ = LoadUserDataAsync();
    }

    #region Lifecycle Methods

    /// <summary>
    /// Вызывается при первой загрузке ViewModel
    /// </summary>
    protected override async Task OnFirstTimeLoadedAsync()
    {
        await base.OnFirstTimeLoadedAsync();
        await LoadUserDataAsync();
    }

    /// <summary>
    /// Вызывается при активации ViewModel
    /// </summary>
    protected override void OnActivated()
    {
        base.OnActivated();
        _ = LoadUserDataAsync();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Инициализирует команды
    /// </summary>
    private void InitializeCommands()
    {
        // Команда сохранения
        SaveCommand = CreateCommand(SaveProfileAsync, null, "Ошибка сохранения профиля");

        // Команда отмены
        CancelCommand = CreateCommand(CancelChangesAsync, null, "Ошибка отмены изменений");

        // Команда изменения пароля
        ChangePasswordCommand = CreateCommand(ChangePasswordAsync, null, "Ошибка изменения пароля");

        // Команда загрузки фото
        UploadPhotoCommand = CreateCommand(UploadPhotoAsync, null, "Ошибка загрузки фото");

        // Команда удаления фото
        RemovePhotoCommand = CreateCommand(RemovePhotoAsync, null, "Ошибка удаления фото");

        // Команда обновления
        RefreshCommand = CreateCommand(LoadUserDataAsync, null, "Ошибка обновления данных");

        // Команда выхода из аккаунта
        LogoutCommand = CreateCommand(LogoutAsync, null, "Ошибка выхода из аккаунта");

        // Команда удаления профиля
        DeleteProfileCommand = CreateCommand(DeleteProfileAsync, null, "Ошибка удаления профиля");
    }

    /// <summary>
    /// Загружает данные пользователя
    /// </summary>
    private async Task LoadUserDataAsync()
    {
        try
        {
            IsLoading = true;
            
            // Проверяем состояние сессии
            var currentPerson = PersonSessionService.CurrentPerson;
            
            if (currentPerson != null)
            {
                PopulateFromPerson(currentPerson);
                await LoadUserStatisticsAsync(currentPerson.Uid);
                ShowInfo("Данные профиля загружены");
            }
            else
            {
                // Очищаем все поля
                ClearProfileData();
                
                // Показываем сообщение пользователю
                SetError("Пользователь не авторизован. Пожалуйста, войдите в систему.");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки данных профиля");
            SetError("Критическая ошибка загрузки данных профиля", ex);
            
            // Очищаем данные при ошибке
            ClearProfileData();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Загружает статистику пользователя
    /// </summary>
    private async Task LoadUserStatisticsAsync(Guid personUid)
    {
        try
        {
            // Загружаем последний вход из Account
            var account = await _personService.GetPersonAccountAsync(personUid);
            LastLoginDate = account?.LastLoginAt;
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки статистики пользователя");
            LastLoginDate = null;
        }
    }

    /// <summary>
    /// Сохраняет изменения профиля
    /// </summary>
    private async Task SaveProfileAsync()
    {
        try
        {
            IsSaving = true;

            var currentPerson = PersonSessionService.CurrentPerson;
            if (currentPerson == null)
            {
                await _dialogService.ShowErrorAsync("Ошибка", "Пользователь не найден");
                SetError("Пользователь не найден");
                return;
            }

            // Создаем копию для обновления, чтобы не изменять оригинал до подтверждения
            var updatedPerson = new Person
            {
                Uid = currentPerson.Uid,
                FirstName = FirstName?.Trim() ?? string.Empty,
                LastName = LastName?.Trim() ?? string.Empty,
                MiddleName = string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Trim(),
                DateOfBirth = DateOfBirth?.DateTime,
                Email = Email?.Trim() ?? string.Empty,
                PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
                ProfileImageUrl = ProfileImageUrl,
                Address = currentPerson.Address,
                IsActive = currentPerson.IsActive,
                CreatedAt = currentPerson.CreatedAt,
                LastModifiedAt = DateTime.UtcNow,
                IsDeleted = currentPerson.IsDeleted,
                DeletedAt = currentPerson.DeletedAt
            };

            // Валидация данных перед сохранением
            if (string.IsNullOrWhiteSpace(updatedPerson.FirstName))
            {
                await _dialogService.ShowErrorAsync("Ошибка валидации", "Имя обязательно для заполнения");
                return;
            }

            if (string.IsNullOrWhiteSpace(updatedPerson.LastName))
            {
                await _dialogService.ShowErrorAsync("Ошибка валидации", "Фамилия обязательна для заполнения");
                return;
            }

            if (string.IsNullOrWhiteSpace(updatedPerson.Email))
            {
                await _dialogService.ShowErrorAsync("Ошибка валидации", "Email обязателен для заполнения");
                return;
            }

            // Проверка формата email
            if (!IsValidEmail(updatedPerson.Email))
            {
                await _dialogService.ShowErrorAsync("Ошибка валидации", "Неверный формат email");
                return;
            }

            StatusLogger.LogInfo($"Updating person data for user: {currentPerson.Uid}", "ProfileViewModel");
            StatusLogger.LogInfo($"Data: {updatedPerson.FirstName} {updatedPerson.LastName}, Email: {updatedPerson.Email}", "ProfileViewModel");

            var success = await _personService.UpdateAsync(updatedPerson);
            StatusLogger.LogInfo($"PersonService.UpdateAsync result: {success}", "ProfileViewModel");
            
            if (success)
            {
                // Обновляем сессию с новыми данными
                _personSessionService.SetCurrentPerson(updatedPerson);
                StatusLogger.LogInfo("Person session updated with new data", "ProfileViewModel");
                
                // Перезагружаем данные из базы для синхронизации
                await LoadUserDataAsync();
                
                await _dialogService.ShowInfoAsync("Успешно", "Профиль успешно обновлен");
                ShowInfo("Профиль успешно сохранен");
            }
            else
            {
                await _dialogService.ShowErrorAsync("Ошибка", "Не удалось сохранить профиль. Возможно, пользователь с таким email уже существует.");
                SetError("Ошибка сохранения профиля");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка сохранения профиля");
            await _dialogService.ShowErrorAsync("Ошибка", $"Произошла ошибка при сохранении профиля: {ex.Message}");
            SetError("Ошибка сохранения профиля", ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Отменяет изменения
    /// </summary>
    private async Task CancelChangesAsync()
    {
        try
        {
            var result = await _dialogService.ShowConfirmationAsync(
                "Отмена изменений",
                "Вы уверены, что хотите отменить все изменения?");

            if (result)
            {
                await LoadUserDataAsync();
                ShowInfo("Изменения отменены");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка отмены изменений");
            SetError("Ошибка отмены изменений", ex);
        }
    }

    /// <summary>
    /// Изменяет пароль пользователя
    /// </summary>
    private async Task ChangePasswordAsync()
    {
        try
        {
            IsChangingPassword = true;

            // Валидация полей
            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                await _dialogService.ShowErrorAsync("Ошибка валидации", "Введите текущий пароль");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                await _dialogService.ShowErrorAsync("Ошибка валидации", "Введите новый пароль");
                return;
            }

            if (NewPassword.Length < 6)
            {
                await _dialogService.ShowErrorAsync("Ошибка валидации", "Новый пароль должен содержать не менее 6 символов");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                await _dialogService.ShowErrorAsync("Ошибка валидации", "Пароли не совпадают");
                return;
            }

            // Проверка на совпадение с текущим паролем
            if (CurrentPassword == NewPassword)
            {
                await _dialogService.ShowErrorAsync("Ошибка валидации", "Новый пароль должен отличаться от текущего");
                return;
            }

            var currentPerson = PersonSessionService.CurrentPerson;
            if (currentPerson == null)
            {
                await _dialogService.ShowErrorAsync("Ошибка", "Пользователь не авторизован");
                return;
            }

            // Показываем информацию о том, что будет происходить
            var infoConfirmation = await _dialogService.ShowConfirmationAsync(
                "Информация о смене пароля",
                $"Вы собираетесь изменить пароль для пользователя: {currentPerson.Email}\n\nТекущий пароль будет заменен на новый.\n\nПродолжить?");

            if (!infoConfirmation) return;

            // Первое подтверждение
            var firstConfirmation = await _dialogService.ShowConfirmationAsync(
                "Подтверждение смены пароля",
                "Вы уверены, что хотите изменить пароль?\n\nУбедитесь, что вы запомнили новый пароль!");

            if (!firstConfirmation) return;

            // Финальное подтверждение с показом нового пароля (замаскированного)
            var maskedPassword = new string('*', NewPassword.Length);
            var secondConfirmation = await _dialogService.ShowConfirmationAsync(
                "Окончательное подтверждение",
                $"ВНИМАНИЕ! Вы действительно хотите изменить пароль?\n\nНовый пароль: {maskedPassword} ({NewPassword.Length} символов)\n\nПосле смены пароля рекомендуется выйти и войти заново для проверки.\n\nПродолжить?");

            if (!secondConfirmation) return;

            StatusLogger.LogInfo($"Password change initiated for user: {currentPerson.Uid} ({currentPerson.Email})", "ProfileViewModel");

            // Попытка смены пароля
            var success = await _authService.ChangePasswordAsync(currentPerson.Uid, CurrentPassword, NewPassword);

            if (success)
            {
                // Очищаем поля паролей для безопасности
                ClearPasswordFields();
                
                StatusLogger.LogSuccess($"Password changed successfully for user: {currentPerson.Email}", "ProfileViewModel");
                
                // Показываем успешное сообщение с рекомендациями
                await _dialogService.ShowInfoAsync("Пароль успешно изменен!", 
                    $"Пароль для пользователя {currentPerson.Email} успешно изменен.\n\n" +
                    "РЕКОМЕНДАЦИИ:\n" +
                    "1. Запишите новый пароль в безопасном месте\n" +
                    "2. Попробуйте войти с новым паролем в новой вкладке для проверки\n" +
                    "3. Поля пароля очищены для безопасности");
                
                ShowInfo("Пароль успешно изменен");
            }
            else
            {
                await _dialogService.ShowErrorAsync("Ошибка смены пароля", 
                    "Не удалось изменить пароль.\n\nВозможные причины:\n" +
                    "• Неверный текущий пароль\n" +
                    "• Проблемы с подключением к базе данных\n" +
                    "• Аккаунт заблокирован\n\n" +
                    "Проверьте правильность текущего пароля и попробуйте снова.");
                SetError("Ошибка изменения пароля");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка изменения пароля");
            await _dialogService.ShowErrorAsync("Критическая ошибка", 
                $"Произошла критическая ошибка при изменении пароля:\n\n{ex.Message}\n\n" +
                "Если проблема повторяется, обратитесь к администратору системы.");
            SetError("Ошибка изменения пароля", ex);
        }
        finally
        {
            IsChangingPassword = false;
        }
    }

    /// <summary>
    /// Загружает фото профиля
    /// </summary>
    private async Task UploadPhotoAsync()
    {
        try
        {
            IsUploadingPhoto = true;
            
            // Показываем диалог выбора файла
            var filePath = await _dialogService.ShowFilePickerAsync(
                "Выберите фото профиля", 
                new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" });

            if (string.IsNullOrEmpty(filePath))
                return;

            // TODO: Implement actual photo upload logic
            // For now, just set the path
            ProfileImageUrl = filePath;
            
            await _dialogService.ShowInfoAsync("Успешно", "Фото профиля загружено");
            ShowInfo("Фото профиля обновлено");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка загрузки фото");
            await _dialogService.ShowErrorAsync("Ошибка", "Не удалось загрузить фото");
            SetError("Ошибка загрузки фото", ex);
        }
        finally
        {
            IsUploadingPhoto = false;
        }
    }

    /// <summary>
    /// Удаляет фото профиля
    /// </summary>
    private async Task RemovePhotoAsync()
    {
        try
        {
            var confirmation = await _dialogService.ShowConfirmationAsync(
                "Удаление фото",
                "Вы уверены, что хотите удалить фото профиля?");

            if (!confirmation) return;

            ProfileImageUrl = string.Empty;
            
            await _dialogService.ShowInfoAsync("Успешно", "Фото профиля удалено");
            ShowInfo("Фото профиля удалено");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка удаления фото");
            await _dialogService.ShowErrorAsync("Ошибка", "Не удалось удалить фото");
            SetError("Ошибка удаления фото", ex);
        }
    }

    /// <summary>
    /// Заполняет поля из объекта Person
    /// </summary>
    private void PopulateFromPerson(Person person)
    {
        // Заполняем поля с проверкой
        FirstName = person.FirstName ?? string.Empty;
        LastName = person.LastName ?? string.Empty;
        MiddleName = person.MiddleName ?? string.Empty;
        DateOfBirth = person.DateOfBirth.HasValue ? new DateTimeOffset(person.DateOfBirth.Value) : null;
        Email = person.Email ?? string.Empty;
        PhoneNumber = person.PhoneNumber ?? string.Empty;
        RegistrationDate = person.CreatedAt;

        // Загружаем роли
        UserRoles.Clear();
        
        if (person.PersonRoles != null)
        {
            var activeRoles = person.PersonRoles.Where(pr => pr.IsActive).ToList();
            
            foreach (var personRole in activeRoles)
            {
                UserRoles.Add(personRole);
            }
        }
    }

    /// <summary>
    /// Очищает данные профиля
    /// </summary>
    private void ClearProfileData()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        MiddleName = string.Empty;
        DateOfBirth = null;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        ProfileImageUrl = string.Empty;
        UserRoles.Clear();
        LastLoginDate = null;
        RegistrationDate = DateTime.MinValue;
        
        ClearPasswordFields();
    }

    /// <summary>
    /// Очищает поля пароля
    /// </summary>
    private void ClearPasswordFields()
    {
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
    }

    /// <summary>
    /// Выход из аккаунта
    /// </summary>
    private async Task LogoutAsync()
    {
        try
        {
            var confirmation = await _dialogService.ShowConfirmationAsync(
                "Выход из системы",
                "Вы уверены, что хотите выйти из системы?");

            if (!confirmation) return;

            StatusLogger.LogInfo("User logout initiated", "ProfileViewModel");

            await _authService.LogoutAsync();
            
            // Навигация на страницу входа
            await _navigationService.NavigateToAsync<LoginViewModel>();
            
            StatusLogger.LogSuccess("User logged out successfully", "ProfileViewModel");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка выхода из системы");
            await _dialogService.ShowErrorAsync("Ошибка", "Произошла ошибка при выходе из системы");
            SetError("Ошибка выхода из системы", ex);
        }
    }

    /// <summary>
    /// Удаляет профиль пользователя (деактивация)
    /// </summary>
    private async Task DeleteProfileAsync()
    {
        try
        {
            // Двойное подтверждение для критической операции
            var firstConfirmation = await _dialogService.ShowConfirmationAsync(
                "Удаление профиля",
                "ВНИМАНИЕ! Это действие приведет к деактивации вашего аккаунта.\n\nВы уверены, что хотите продолжить?");

            if (!firstConfirmation) return;

            var secondConfirmation = await _dialogService.ShowConfirmationAsync(
                "Окончательное подтверждение",
                "Это действие нельзя отменить!\n\nВаш аккаунт будет деактивирован, но данные сохранятся для аудита.\n\nВы ДЕЙСТВИТЕЛЬНО хотите удалить профиль?");

            if (!secondConfirmation) return;

            var currentPerson = PersonSessionService.CurrentPerson;
            if (currentPerson == null)
            {
                await _dialogService.ShowErrorAsync("Ошибка", "Пользователь не найден");
                return;
            }

            StatusLogger.LogInfo($"Profile deletion confirmed for user: {currentPerson.Uid}", "ProfileViewModel");

            // Деактивируем аккаунт
            var success = await _personService.DeactivateAsync(currentPerson.Uid);

            if (success)
            {
                await _dialogService.ShowInfoAsync("Профиль удален", "Ваш профиль был успешно деактивирован. Вы будете перенаправлены на страницу входа.");
                
                // Выходим из системы
                await LogoutAsync();
            }
            else
            {
                await _dialogService.ShowErrorAsync("Ошибка", "Не удалось деактивировать профиль");
                SetError("Ошибка удаления профиля");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка удаления профиля");
            await _dialogService.ShowErrorAsync("Ошибка", "Произошла ошибка при удалении профиля");
            SetError("Ошибка удаления профиля", ex);
        }
    }

    /// <summary>
    /// Проверка валидности email
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}

