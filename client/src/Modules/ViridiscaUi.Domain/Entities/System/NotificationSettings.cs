using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.System.Enums;
using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.Domain.Entities.System;

/// <summary>
/// Настройки уведомлений пользователя
/// </summary>
public class NotificationSettings : AuditableEntity
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid PersonUid { get; set; }

    /// <summary>
    /// Идентификатор пользователя (алиас для совместимости)
    /// </summary>
    public Guid UserUid 
    { 
        get => PersonUid; 
        set => PersonUid = value; 
    }

    /// <summary>
    /// Включены ли уведомления для пользователя
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Включены ли email уведомления
    /// </summary>
    public bool EmailNotifications { get; set; } = true;

    /// <summary>
    /// Включены ли SMS уведомления
    /// </summary>
    public bool SmsNotifications { get; set; } = false;

    /// <summary>
    /// Включены ли push уведомления
    /// </summary>
    public bool PushNotifications { get; set; } = true;

    /// <summary>
    /// Минимальный приоритет уведомлений для отправки
    /// </summary>
    public NotificationPriority MinimumPriority { get; set; } = NotificationPriority.Low;

    /// <summary>
    /// JSON-строка для хранения настроек по типам уведомлений
    /// </summary>
    public string? TypeSettingsJson { get; set; }
    
    /// <summary>
    /// Пользователь
    /// </summary>
    public Person? Person { get; set; }
} 