using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.Auth;

/// <summary>
/// Аккаунт для аутентификации
/// </summary>
public class Account : AuditableEntity 
{
    /// <summary>
    /// ID связанного человека
    /// </summary>
    public Guid PersonUid { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Хеш пароля
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Подтвержден ли email
    /// </summary>
    public bool IsEmailConfirmed { get; set; }

    /// <summary>
    /// Заблокирован ли аккаунт
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Активен ли аккаунт
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Количество неудачных попыток входа
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Последний вход в систему
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Последняя неудачная попытка входа
    /// </summary>
    public DateTime? LastFailedLoginAt { get; set; }

    /// <summary>
    /// Заблокирован до
    /// </summary>
    public DateTime? LockedUntil { get; set; }

    /// <summary>
    /// Связанный человек
    /// </summary>
    public Person? Person { get; set; }
} 