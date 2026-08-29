using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.Auth;

/// <summary>
/// Базовая сущность для всех людей в системе
/// </summary>
public class Person : AuditableEntity
{
    /// <summary>
    /// Имя
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Фамилия
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Отчество
    /// </summary>
    public string? MiddleName { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Телефон
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Телефон (алиас для совместимости)
    /// </summary>
    public string? Phone 
    { 
        get => PhoneNumber; 
        set => PhoneNumber = value; 
    }

    /// <summary>
    /// Дата рождения
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// URL изображения профиля
    /// </summary>
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Адрес
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Активен ли человек
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Роли человека в системе
    /// </summary>
    public ICollection<PersonRole> PersonRoles { get; set; } = new List<PersonRole>();
    
    /// <summary>
    /// Аккаунт для аутентификации
    /// </summary>
    public Account? Account { get; set; }

    /// <summary>
    /// Полное имя
    /// </summary>
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
} 