using ViridiscaUi.Domain.Entities.System.Enums;

namespace ViridiscaUi.Domain.Models.System;

/// <summary>
/// Статистика уведомлений пользователя
/// </summary>
public class NotificationStatistics
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserUid { get; set; }

    /// <summary>
    /// Общее количество уведомлений
    /// </summary>
    public int TotalNotifications { get; set; }

    /// <summary>
    /// Количество непрочитанных уведомлений
    /// </summary>
    public int UnreadNotifications { get; set; }

    /// <summary>
    /// Количество важных уведомлений
    /// </summary>
    public int ImportantNotifications { get; set; }

    /// <summary>
    /// Количество уведомлений за сегодня
    /// </summary>
    public int TodayNotifications { get; set; }

    /// <summary>
    /// Количество уведомлений за неделю
    /// </summary>
    public int WeekNotifications { get; set; }

    /// <summary>
    /// Статистика по типам уведомлений
    /// </summary>
    public Dictionary<NotificationType, int> ByType { get; set; } = new();

    /// <summary>
    /// Статистика по категориям уведомлений
    /// </summary>
    public Dictionary<string, int> ByCategory { get; set; } = new();

    /// <summary>
    /// Статистика по приоритетам уведомлений
    /// </summary>
    public Dictionary<NotificationPriority, int> ByPriority { get; set; } = new();

    /// <summary>
    /// Дата последнего уведомления
    /// </summary>
    public DateTime? LastNotificationDate { get; set; }

    /// <summary>
    /// Среднее время прочтения уведомлений (в минутах)
    /// </summary>
    public double AverageReadTime { get; set; }

    /// <summary>
    /// Процент прочитанных уведомлений
    /// </summary>
    public decimal ReadPercentage => TotalNotifications > 0 
        ? (decimal)(TotalNotifications - UnreadNotifications) / TotalNotifications * 100 
        : 0;

    /// <summary>
    /// Процент важных уведомлений
    /// </summary>
    public decimal ImportantPercentage => TotalNotifications > 0 
        ? (decimal)ImportantNotifications / TotalNotifications * 100 
        : 0;
}

/// <summary>
/// Системная статистика уведомлений
/// </summary>
public class SystemNotificationStatistics
{
    /// <summary>
    /// Общее количество уведомлений в системе
    /// </summary>
    public int TotalNotifications { get; set; }

    /// <summary>
    /// Количество отправленных уведомлений за сегодня
    /// </summary>
    public int TodayNotifications { get; set; }

    /// <summary>
    /// Количество отправленных уведомлений за неделю
    /// </summary>
    public int WeekNotifications { get; set; }

    /// <summary>
    /// Количество отправленных уведомлений за месяц
    /// </summary>
    public int MonthNotifications { get; set; }

    /// <summary>
    /// Количество активных пользователей (получивших уведомления)
    /// </summary>
    public int ActiveUsers { get; set; }

    /// <summary>
    /// Средний процент прочтения уведомлений
    /// </summary>
    public decimal AverageReadRate { get; set; }

    /// <summary>
    /// Статистика по типам уведомлений
    /// </summary>
    public Dictionary<NotificationType, int> ByType { get; set; } = new();

    /// <summary>
    /// Статистика по приоритетам уведомлений
    /// </summary>
    public Dictionary<NotificationPriority, int> ByPriority { get; set; } = new();

    /// <summary>
    /// Статистика по категориям уведомлений
    /// </summary>
    public Dictionary<string, int> ByCategory { get; set; } = new();

    /// <summary>
    /// Топ-10 самых активных получателей уведомлений
    /// </summary>
    public List<UserNotificationSummary> TopRecipients { get; set; } = new();

    /// <summary>
    /// Время последнего обновления статистики
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Краткая информация о пользователе и его уведомлениях
/// </summary>
public class UserNotificationSummary
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserUid { get; set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Email пользователя
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Количество полученных уведомлений
    /// </summary>
    public int NotificationCount { get; set; }

    /// <summary>
    /// Количество прочитанных уведомлений
    /// </summary>
    public int ReadCount { get; set; }

    /// <summary>
    /// Процент прочтения
    /// </summary>
    public decimal ReadPercentage => NotificationCount > 0 
        ? (decimal)ReadCount / NotificationCount * 100 
        : 0;
} 