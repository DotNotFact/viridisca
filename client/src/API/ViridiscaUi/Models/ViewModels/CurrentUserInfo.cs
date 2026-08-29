using System;
using ReactiveUI.Fody.Helpers;

namespace ViridiscaUi.Models.ViewModels;

/// <summary>
/// Информация о текущем пользователе
/// </summary>
public class CurrentUserInfo : ViewModelBase
{
    /// <summary>
    /// Уникальный идентификатор пользователя
    /// </summary>
    [Reactive] public Guid Uid { get; set; }

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
    /// Полное имя пользователя
    /// </summary>
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    /// <summary>
    /// Email пользователя
    /// </summary>
    [Reactive] public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя
    /// </summary>
    [Reactive] public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Отображаемое название роли
    /// </summary>
    [Reactive] public string RoleDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Аватар пользователя (URL или путь к файлу)
    /// </summary>
    [Reactive] public string? AvatarUrl { get; set; }

    /// <summary>
    /// Активен ли пользователь
    /// </summary>
    [Reactive] public bool IsActive { get; set; } = true;

    /// <summary>
    /// Дата последнего входа
    /// </summary>
    [Reactive] public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Права доступа пользователя
    /// </summary>
    [Reactive] public List<string> Permissions { get; set; } = new();

    /// <summary>
    /// Роли пользователя
    /// </summary>
    [Reactive] public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Основная роль пользователя
    /// </summary>
    public string PrimaryRole => Roles.FirstOrDefault() ?? Role ?? "Пользователь";

    /// <summary>
    /// Приветственное сообщение
    /// </summary>
    public string WelcomeMessage => $"Добро пожаловать, {FirstName}!";

    /// <summary>
    /// Создает экземпляр информации о пользователе
    /// </summary>
    public CurrentUserInfo()
    {
        // Настройка уведомлений об изменении свойств
        this.WhenAnyValue(x => x.FirstName, x => x.LastName, x => x.MiddleName)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(FullName)));
    }

    /// <summary>
    /// Проверяет, есть ли у пользователя указанное разрешение
    /// </summary>
    /// <param name="permission">Название разрешения</param>
    /// <returns>True, если разрешение есть</returns>
    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission);
    }

    /// <summary>
    /// Создает информацию о пользователе из сущности Person
    /// </summary>
    /// <param name="person">Сущность Person</param>
    /// <returns>Информация о пользователе</returns>
    public static CurrentUserInfo FromPerson(Person person)
    {
        return new CurrentUserInfo
        {
            Uid = person.Uid,
            FirstName = person.FirstName,
            LastName = person.LastName,
            MiddleName = person.MiddleName ?? string.Empty,
            Email = person.Email,
            IsActive = person.IsActive,
            LastLoginAt = DateTime.UtcNow // Заглушка
        };
    }

    /// <summary>
    /// Возвращает строковое представление пользователя
    /// </summary>
    public override string ToString()
    {
        return $"{FullName} ({Email})";
    }
} 