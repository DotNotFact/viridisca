using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.System.Enums;
using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.Domain.Entities.System;

/// <summary>
/// Уведомление пользователя
/// </summary>
public class Notification : AuditableEntity
{
    /// <summary>
    /// Идентификатор получателя
    /// </summary>
    public Guid PersonUid { get; set; }

    /// <summary>
    /// Идентификатор шаблона уведомления (опционально)
    /// </summary>
    public Guid? TemplateUid { get; set; }

    /// <summary>
    /// Заголовок уведомления
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Текст уведомления
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Тип уведомления
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// Категория уведомления (например, "Оценки", "Расписание")
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Приоритет уведомления
    /// </summary>
    public NotificationPriority Priority { get; set; }

    /// <summary>
    /// Флаг прочитанности
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Время отправки
    /// </summary>
    public DateTime SentAt { get; set; }

    /// <summary>
    /// Время прочтения
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Дата и время истечения срока действия уведомления
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// URL для действия
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Дополнительные метаданные в формате JSON
    /// </summary>
    public string? MetadataJson { get; set; }
    
    /// <summary>
    /// Получатель уведомления
    /// </summary>
    public Person? Person { get; set; }

    /// <summary>
    /// Шаблон уведомления
    /// </summary>
    public NotificationTemplate? Template { get; set; }
} 