using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.System.Enums;

namespace ViridiscaUi.Domain.Entities.System;

/// <summary>
/// Шаблон уведомления
/// </summary>
public class NotificationTemplate : AuditableEntity
{
    /// <summary>
    /// Название шаблона
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание шаблона
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Шаблон заголовка
    /// </summary>
    public string TitleTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Шаблон сообщения
    /// </summary>
    public string MessageTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Тип уведомления
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.Info;

    /// <summary>
    /// Приоритет уведомления
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// Категория уведомления
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Флаг активности шаблона
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON-строка для хранения списка параметров
    /// </summary>
    public string? ParametersJson { get; set; }
} 