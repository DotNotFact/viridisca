using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Entities.System.Enums;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// ViewModel для отображения и управления уведомлением
/// </summary>
public class NotificationViewModel : ReactiveObject
{
    private DomainNotification _notification = null!;

    public DomainNotification Notification
    {
        get => _notification;
        set => this.RaiseAndSetIfChanged(ref _notification, value);
    }

    public NotificationViewModel()
    {
    }

    public NotificationViewModel(DomainNotification notification)
    {
        Notification = notification ?? throw new ArgumentNullException(nameof(notification));
    }

    /// <summary>
    /// Заголовок уведомления
    /// </summary>
    public string Title => Notification?.Title ?? string.Empty;

    /// <summary>
    /// Содержимое уведомления
    /// </summary>
    public string Content => _notification.Message ?? string.Empty;

    /// <summary>
    /// Тип уведомления
    /// </summary>
    public NotificationType Type => Notification?.Type ?? NotificationType.Info;

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime CreatedAt => Notification?.CreatedAt ?? DateTime.Now;

    /// <summary>
    /// Прочитано ли уведомление
    /// </summary>
    public bool IsRead => Notification?.IsRead ?? false;

    /// <summary>
    /// Форматированная дата
    /// </summary>
    public string FormattedDate => CreatedAt.ToString("dd.MM.yyyy HH:mm");

    /// <summary>
    /// Иконка типа уведомления
    /// </summary>
    public string TypeIcon => GetTypeIcon(Type);

    /// <summary>
    /// Цвет типа уведомления
    /// </summary>
    public string TypeColor => GetTypeColor(Type);

    /// <summary>
    /// Имя отправителя
    /// </summary>
    public string SenderName => "Система";

    /// <summary>
    /// Отображаемое имя типа
    /// </summary>
    public string TypeDisplayName => Type switch
    {
        NotificationType.Info => "Информация",
        NotificationType.Warning => "Предупреждение",
        NotificationType.Error => "Ошибка",
        NotificationType.Success => "Успех",
        NotificationType.System => "Система",
        NotificationType.Grade => "Оценка",
        NotificationType.Attendance => "Посещаемость",
        NotificationType.Assignment => "Задание",
        NotificationType.Reminder => "Напоминание",
        _ => "Информация"
    };

    private string GetTypeIcon(NotificationType type)
    {
        return type switch
        {
            NotificationType.Info => "Information",
            NotificationType.Warning => "Warning",
            NotificationType.Error => "Error",
            NotificationType.Success => "CheckCircle",
            NotificationType.System => "Cog",
            NotificationType.Grade => "SchoolOutline",
            NotificationType.Attendance => "AccountCheck",
            NotificationType.Assignment => "FileDocument",
            NotificationType.Reminder => "Bell",
            _ => "Information"
        };
    }

    private string GetTypeColor(NotificationType type)
    {
        return type switch
        {
            NotificationType.Info => "#2196F3",
            NotificationType.Warning => "#FF9800",
            NotificationType.Error => "#F44336",
            NotificationType.Success => "#4CAF50",
            NotificationType.System => "#9E9E9E",
            NotificationType.Grade => "#3F51B5",
            NotificationType.Attendance => "#FF5722",
            NotificationType.Assignment => "#795548",
            NotificationType.Reminder => "#FFC107",
            _ => "#2196F3"
        };
    }
} 

