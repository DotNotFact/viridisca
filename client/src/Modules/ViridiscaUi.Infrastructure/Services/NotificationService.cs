using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Models.Common;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.System.Enums;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Infrastructure.Data;
using ViridiscaUi.Domain.Services.System;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Domain.Models;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для работы с уведомлениями
/// </summary>
public class NotificationService(ApplicationDbContext dbContext, ILogger<NotificationService> logger, IPersonSessionService personSessionService) : INotificationService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<NotificationService> _logger = logger;
    private readonly IPersonSessionService _personSessionService = personSessionService;

    /// <summary>
    /// Создает новое уведомление
    /// </summary>
    public async Task<Notification> CreateNotificationAsync(
        Guid recipientUid, 
        string title, 
        string message, 
        NotificationType type = NotificationType.Info, 
        NotificationPriority priority = NotificationPriority.Normal, 
        string? category = null, 
        string? actionUrl = null)
    {
        var notification = new Notification
        {
            PersonUid = recipientUid,
            Title = title,
            Message = message,
            Type = type,
            Priority = priority,
            Category = category,
            ActionUrl = actionUrl,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Notification created for user {UserId}: {Title}", recipientUid, title);
        return notification;
    }

    /// <summary>
    /// Отправляет уведомление пользователю
    /// </summary>
    public async Task<Notification> SendNotificationAsync(
        Guid recipientUid, 
        string title, 
        string message, 
        NotificationType type = NotificationType.Info, 
        NotificationPriority priority = NotificationPriority.Normal, 
        string? category = null, 
        string? actionUrl = null, 
        Dictionary<string, object>? metadata = null, 
        DateTime? expiresAt = null)
    {
        var notification = new Notification
        {
            PersonUid = recipientUid,
            Title = title,
            Message = message,
            Type = type,
            Priority = priority,
            Category = category,
            ActionUrl = actionUrl,
            SentAt = DateTime.UtcNow,
            IsRead = false,
            ExpiresAt = expiresAt
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();

        return notification;
    }

    /// <summary>
    /// Получает уведомления пользователя с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Notification> Notifications, int TotalCount)> GetUserNotificationsPagedAsync(
        Guid userUid, 
        int page = 1, 
        int pageSize = 20, 
        bool includeRead = true, 
        NotificationType? typeFilter = null, 
        NotificationPriority? priorityFilter = null, 
        string? categoryFilter = null)
    {
        var query = _dbContext.Notifications
            .Where(n => n.PersonUid == userUid)
            .AsQueryable();

        if (!includeRead)
            query = query.Where(n => !n.IsRead);

        if (typeFilter.HasValue)
            query = query.Where(n => n.Type == typeFilter.Value);

        if (priorityFilter.HasValue)
            query = query.Where(n => n.Priority == priorityFilter.Value);

        //if (!string.IsNullOrEmpty(categoryFilter) && Guid.TryParse(categoryFilter, out var categoryUid))
        //    query = query.Where(n => n.Category == categoryUid);

        // Исключаем истекшие уведомления
        query = query.Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow);

        var totalCount = await query.CountAsync();
        var notifications = await query
            .OrderByDescending(n => n.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (notifications, totalCount);
    }

    /// <summary>
    /// Получает все уведомления пользователя
    /// </summary>
    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(
        Guid userUid, 
        bool includeRead = true, 
        int? limit = null)
    {
        var query = _dbContext.Notifications
            .Where(n => n.PersonUid == userUid)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
            .AsQueryable();

        if (!includeRead)
            query = query.Where(n => !n.IsRead);

        query = query.OrderByDescending(n => n.SentAt);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Получает количество непрочитанных уведомлений
    /// </summary>
    public async Task<int> GetUnreadCountAsync(Guid userUid)
    {
        return await _dbContext.Notifications
            .Where(n => n.PersonUid == userUid && !n.IsRead)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
            .CountAsync();
    }

    /// <summary>
    /// Отмечает уведомление как прочитанное
    /// </summary>
    public async Task<bool> MarkAsReadAsync(Guid notificationUid)
    {
        var notification = await _dbContext.Notifications.FindAsync(notificationUid);
        if (notification == null)
            return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Отмечает несколько уведомлений как прочитанные
    /// </summary>
    public async Task<int> MarkMultipleAsReadAsync(IEnumerable<Guid> notificationUids)
    {
        var notifications = await _dbContext.Notifications
            .Where(n => notificationUids.Contains(n.Uid))
            .ToListAsync();

        var count = 0;
        foreach (var notification in notifications)
        {
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                count++;
            }
        }

        await _dbContext.SaveChangesAsync();
        return count;
    }

    /// <summary>
    /// Отмечает все уведомления пользователя как прочитанные
    /// </summary>
    public async Task<int> MarkAllAsReadAsync(Guid userUid)
    {
        var notifications = await _dbContext.Notifications
            .Where(n => n.PersonUid == userUid && !n.IsRead)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        return notifications.Count;
    }

    /// <summary>
    /// Удаляет уведомление
    /// </summary>
    public async Task<bool> DeleteNotificationAsync(Guid notificationUid)
    {
        var notification = await _dbContext.Notifications.FindAsync(notificationUid);
        if (notification == null)
            return false;

        _dbContext.Notifications.Remove(notification);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Удаляет несколько уведомлений
    /// </summary>
    public async Task<int> DeleteMultipleNotificationsAsync(IEnumerable<Guid> notificationUids)
    {
        var notifications = await _dbContext.Notifications
            .Where(n => notificationUids.Contains(n.Uid))
            .ToListAsync();

        _dbContext.Notifications.RemoveRange(notifications);
        await _dbContext.SaveChangesAsync();
        return notifications.Count;
    }

    /// <summary>
    /// Отправляет общее уведомление
    /// </summary>
    public async Task SendNotificationAsync(string title, string message, NotificationType type = NotificationType.Info, NotificationPriority priority = NotificationPriority.Normal)
    {
        // No recipient parameter on this overload — this is the "notify the current user about
        // their own action" pattern (see StudentsViewModel's Create/Edit/Delete confirmations).
        // Was previously a Debug.WriteLine-only no-op; the caller's write was silently discarded.
        var currentPersonUid = _personSessionService.CurrentPerson?.Uid;
        if (currentPersonUid is null)
        {
            _logger.LogWarning("SendNotificationAsync({Title}) skipped: no current session person", title);
            return;
        }

        await CreateNotificationAsync(currentPersonUid.Value, title, message, type, priority);
    }

    /// <summary>
    /// Отправляет уведомление пользователям с определенной ролью
    /// </summary>
    public async Task SendNotificationToRoleAsync(string role, string title, string message, NotificationType type = NotificationType.Info, NotificationPriority priority = NotificationPriority.Normal)
    {
        try
        {
            // Получаем всех пользователей с указанными ролями
            var targetPersons = await _dbContext.PersonRoles
                .Where(pr => pr.Role.Name == role)
                .Include(pr => pr.Person)
                .Select(pr => pr.Person)
                .Distinct()
                .ToListAsync();

            // Отправляем уведомления по одному
            foreach (var person in targetPersons)
            {
                await SendNotificationAsync(person.Uid, title, message, type, priority);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sending notification to role: {ex.Message}");
        }
    }

    /// <summary>
    /// Отправляет уведомление всем студентам курса
    /// </summary>
    public async Task SendNotificationToCourseAsync(Guid courseInstanceUid, string title, string message, NotificationType type)
    {
        // Получаем студентов через записи на курс (Enrollments)
        var enrolledStudents = await _dbContext.Enrollments
            .Where(e => e.CourseInstanceUid == courseInstanceUid)
            .Select(e => e.StudentUid)
            .ToListAsync();

        // Отправляем уведомления по одному
        foreach (var studentUid in enrolledStudents)
        {
            await SendNotificationAsync(studentUid, title, message, type, NotificationPriority.Normal);
        }
    }

    /// <summary>
    /// Получает статистику уведомлений пользователя
    /// </summary>
    public async Task<NotificationStatistics> GetUserNotificationStatisticsAsync(Guid userUid)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddMonths(-1);

        var totalNotifications = await _dbContext.Notifications
            .Where(n => n.PersonUid == userUid)
            .CountAsync();

        var unreadNotifications = await _dbContext.Notifications
            .Where(n => n.PersonUid == userUid && !n.IsRead)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > now)
            .CountAsync();

        var todayNotifications = await _dbContext.Notifications
            .Where(n => n.PersonUid == userUid && n.SentAt >= today)
            .CountAsync();

        var weekNotifications = await _dbContext.Notifications
            .Where(n => n.PersonUid == userUid && n.SentAt >= weekAgo)
            .CountAsync();

        var monthNotifications = await _dbContext.Notifications
            .Where(n => n.PersonUid == userUid && n.SentAt >= monthAgo)
            .CountAsync();

        var importantNotifications = await _dbContext.Notifications
            .Where(n => n.PersonUid == userUid && n.Priority == NotificationPriority.High)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > now)
            .CountAsync();

        return new NotificationStatistics
        {
            UserUid = userUid,
            TotalNotifications = totalNotifications,
            UnreadNotifications = unreadNotifications,
            ImportantNotifications = importantNotifications,
            TodayNotifications = todayNotifications,
            WeekNotifications = weekNotifications,
            ByType = new Dictionary<NotificationType, int>(),
            ByCategory = new Dictionary<string, int>(),
            ByPriority = new Dictionary<NotificationPriority, int>(),
            LastNotificationDate = await _dbContext.Notifications
                .Where(n => n.PersonUid == userUid)
                .MaxAsync(n => (DateTime?)n.SentAt),
            AverageReadTime = 0.0 // TODO: Calculate actual average read time
        };
    }

    /// <summary>
    /// Получает статистику уведомлений пользователя
    /// </summary>
    public async Task<NotificationStatistics> GetNotificationStatisticsAsync(Guid userUid) =>
        await GetUserNotificationStatisticsAsync(userUid);

    /// <summary>
    /// Очищает старые уведомления
    /// </summary>
    public async Task CleanupOldNotificationsAsync(TimeSpan maxAge)
    {
        var cutoffDate = DateTime.UtcNow - maxAge;
        
        var oldNotifications = await _dbContext.Notifications
            .Where(n => n.SentAt < cutoffDate && n.IsRead)
            .ToListAsync();

        _dbContext.Notifications.RemoveRange(oldNotifications);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Архивирует старые уведомления
    /// </summary>
    public async Task<int> ArchiveOldNotificationsAsync(DateTime olderThan)
    {
        var notificationsToArchive = await _dbContext.Notifications
            .Where(n => n.CreatedAt < olderThan && n.IsRead)
            .ToListAsync();

        // В будущем можно добавить архивную таблицу
        // Пока просто удаляем старые прочитанные уведомления
        _dbContext.Notifications.RemoveRange(notificationsToArchive);
        await _dbContext.SaveChangesAsync();

        return notificationsToArchive.Count;
    }

    /// <summary>
    /// Уведомляет родителей об оценках
    /// </summary>
    public async Task NotifyParentsAboutGradesAsync(IEnumerable<Grade> grades)
    {
        foreach (var grade in grades)
        {
            // В будущем можно добавить связь студент-родитель
            // Пока отправляем уведомление самому студенту
            await CreateNotificationAsync(
                grade.StudentUid,
                "Новая оценка",
                $"Вы получили оценку {grade.Value} за задание",
                NotificationType.Info);
        }
    }

    /// <summary>
    /// Получает статистику уведомлений пользователя (алиас для совместимости)
    /// </summary>
    public async Task<NotificationStatistics> GetUserStatisticsAsync(Guid userUid) => 
        await GetUserNotificationStatisticsAsync(userUid);

    /// <summary>
    /// Получает системную статистику уведомлений (алиас для совместимости)
    /// </summary>
    public async Task<SystemNotificationStatistics> GetSystemStatisticsAsync() => 
        await GetSystemNotificationStatisticsAsync();

    /// <summary>
    /// Получает системную статистику уведомлений
    /// </summary>
    public async Task<SystemNotificationStatistics> GetSystemNotificationStatisticsAsync()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddMonths(-1);

        var totalNotifications = await _dbContext.Notifications.CountAsync();
        var todayNotifications = await _dbContext.Notifications
            .Where(n => n.SentAt >= today)
            .CountAsync();

        var weekNotifications = await _dbContext.Notifications
            .Where(n => n.SentAt >= weekAgo)
            .CountAsync();

        var monthNotifications = await _dbContext.Notifications
            .Where(n => n.SentAt >= monthAgo)
            .CountAsync();

        return new SystemNotificationStatistics
        {
            TotalNotifications = totalNotifications,
            TodayNotifications = todayNotifications,
            WeekNotifications = weekNotifications,
            MonthNotifications = monthNotifications
        };
    }

    // Методы-заглушки для совместимости с интерфейсом
    public async Task<(IEnumerable<Notification> Notifications, int TotalCount)> GetNotificationsAdvancedAsync(
        Guid userUid, int page, int pageSize, NotificationFilter filter) =>
        await GetUserNotificationsPagedAsync(userUid, page, pageSize);

    public async Task<bool> MarkAsImportantAsync(Guid notificationUid, Guid userUid)
    {
        // Можно добавить поле IsImportant в модель Notification
        return true;
    }

    public async Task<bool> UnmarkAsImportantAsync(Guid notificationUid, Guid userUid)
    {
        // Можно добавить поле IsImportant в модель Notification
        return true;
    }

    public async Task<Notification> ScheduleNotificationAsync(
        Guid recipientUid, string title, string message, DateTime scheduledFor, 
        NotificationType type = NotificationType.Info, NotificationPriority priority = NotificationPriority.Normal, 
        string? category = null, string? actionUrl = null, Dictionary<string, object>? metadata = null, 
        TimeSpan? repeatInterval = null)
    {
        // В будущем можно добавить планировщик задач
        return await CreateNotificationAsync(recipientUid, title, message, type, priority, category, actionUrl);
    }

    public async Task<bool> CancelScheduledNotificationAsync(Guid notificationUid) 
    {
        await Task.CompletedTask;
        return true;
    }

    public async Task ProcessScheduledNotificationsAsync() 
    {
        await Task.CompletedTask;
    }

    public async Task<Notification> CreateReminderAsync(
        Guid userUid, string title, string message, DateTime reminderTime, 
        TimeSpan? repeatInterval = null, Dictionary<string, object>? metadata = null) =>
        await CreateNotificationAsync(userUid, title, message, NotificationType.Reminder);

    public async Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync() 
    {
        await Task.CompletedTask;
        return new List<NotificationTemplate>();
    }

    public async Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template) => template;

    public async Task<Notification> SendFromTemplateAsync(
        Guid templateUid, Guid recipientUid, Dictionary<string, object>? parameters = null) =>
        await CreateNotificationAsync(recipientUid, "Template", "Message");

    public async Task<ViridiscaUi.Domain.Entities.System.NotificationSettings> GetUserSettingsAsync(Guid userUid) 
    {
        var settings = await _dbContext.NotificationSettings
            .FirstOrDefaultAsync(ns => ns.PersonUid == userUid);
        
        return settings ?? new ViridiscaUi.Domain.Entities.System.NotificationSettings
        {
            PersonUid = userUid,
            IsEnabled = true,
            EmailNotifications = true,
            PushNotifications = true
        };
    }

    public async Task<bool> UpdateUserSettingsAsync(Guid userUid, ViridiscaUi.Domain.Entities.System.NotificationSettings settings) 
    {
        var existingSettings = await _dbContext.NotificationSettings
            .FirstOrDefaultAsync(ns => ns.PersonUid == userUid);
        
        if (existingSettings != null)
        {
            existingSettings.IsEnabled = settings.IsEnabled;
            existingSettings.EmailNotifications = settings.EmailNotifications;
            existingSettings.PushNotifications = settings.PushNotifications;
            _dbContext.NotificationSettings.Update(existingSettings);
        }
        else
        {
            settings.PersonUid = userUid;
            _dbContext.NotificationSettings.Add(settings);
        }
        
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<ViridiscaUi.Domain.Entities.System.NotificationSettings> GetNotificationSettingsAsync(Guid userUid) =>
        await GetUserSettingsAsync(userUid);

    public async Task<NotificationTemplate> GetNotificationTemplateAsync(string templateName) =>
        new NotificationTemplate
        {
            Uid = Guid.NewGuid(),
            Name = templateName,
            TitleTemplate = $"Шаблон {templateName}",
            MessageTemplate = $"Содержимое шаблона {templateName}",
            CreatedAt = DateTime.UtcNow
        };

    /// <summary>
    /// Показывает уведомление об успехе
    /// </summary>
    public void ShowSuccess(string message)
    {
        // В реальном приложении здесь будет показ toast-уведомления
        // Пока просто логируем
        Console.WriteLine($"SUCCESS: {message}");
    }

    /// <summary>
    /// Показывает уведомление об ошибке
    /// </summary>
    public void ShowError(string message)
    {
        // В реальном приложении здесь будет показ toast-уведомления
        // Пока просто логируем
        Console.WriteLine($"ERROR: {message}");
    }

    /// <summary>
    /// Показывает информационное уведомление
    /// </summary>
    public void ShowInfo(string message)
    {
        // В реальном приложении здесь будет показ toast-уведомления
        // Пока просто логируем
        Console.WriteLine($"INFO: {message}");
    }

    /// <summary>
    /// Показывает предупреждение
    /// </summary>
    public void ShowWarning(string message)
    {
        // В реальном приложении здесь будет показ toast-уведомления
        // Пока просто логируем
        Console.WriteLine($"WARNING: {message}");
    }

    // === ДОПОЛНИТЕЛЬНЫЕ МЕТОДЫ ИНТЕРФЕЙСА ===

    /// <summary>
    /// Получает все уведомления
    /// </summary>
    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        return await _dbContext.Notifications
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();
    }

    /// <summary>
    /// Получает уведомление по идентификатору
    /// </summary>
    public async Task<Notification?> GetByIdAsync(Guid uid)
    {
        return await _dbContext.Notifications.FindAsync(uid);
    }

    /// <summary>
    /// Получает уведомления пользователя
    /// </summary>
    public async Task<IEnumerable<Notification>> GetByPersonAsync(Guid personUid)
    {
        return await GetUserNotificationsAsync(personUid);
    }

    /// <summary>
    /// Создает новое уведомление
    /// </summary>
    public async Task<Notification> CreateAsync(Notification notification)
    {
        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();
        return notification;
    }

    /// <summary>
    /// Обновляет уведомление
    /// </summary>
    public async Task<Notification> UpdateAsync(Notification notification)
    {
        _dbContext.Notifications.Update(notification);
        await _dbContext.SaveChangesAsync();
        return notification;
    }

    /// <summary>
    /// Удаляет уведомление (возвращает bool для совместимости с интерфейсом)
    /// </summary>
    public async Task<bool> DeleteAsync(Guid uid)
    {
        return await DeleteNotificationAsync(uid);
    }

    /// <summary>
    /// Проверяет существование уведомления
    /// </summary>
    public async Task<bool> ExistsAsync(Guid uid)
    {
        return await _dbContext.Notifications.AnyAsync(n => n.Uid == uid);
    }

    /// <summary>
    /// Получает количество уведомлений
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        return await _dbContext.Notifications.CountAsync();
    }

    /// <summary>
    /// Отправляет уведомление пользователю (простая версия)
    /// </summary>
    public async Task SendNotificationAsync(Guid recipientUid, string title, string message)
    {
        await SendNotificationAsync(recipientUid, title, message, NotificationType.Info);
    }

    /// <summary>
    /// Отправляет уведомление пользователю с категорией
    /// </summary>
    public async Task SendNotificationAsync(Guid recipientUid, string title, string message, string category)
    {
        await SendNotificationAsync(recipientUid, title, message, NotificationType.Info, NotificationPriority.Normal, category);
    }

    /// <summary>
    /// Массовая отправка уведомлений (простая версия)
    /// </summary>
    public async Task SendBulkNotificationAsync(IEnumerable<Guid> recipientUids, string title, string message)
    {
        foreach (var recipientUid in recipientUids)
        {
            await SendNotificationAsync(recipientUid, title, message, NotificationType.Info, NotificationPriority.Normal);
        }
    }

    /// <summary>
    /// Отправляет уведомления о просроченных заданиях
    /// </summary>
    public async Task SendOverdueAssignmentNotificationsAsync()
    {
        var overdueAssignments = await _dbContext.Assignments
            .Where(a => a.DueDate.HasValue && a.DueDate < DateTime.UtcNow)
            .Where(a => a.Status == AssignmentStatus.Published)
            .Include(a => a.CourseInstance)
            .ToListAsync();

        foreach (var assignment in overdueAssignments)
        {
            await SendNotificationToCourseAsync(
                assignment.CourseInstanceUid,
                "Просроченное задание",
                $"Задание '{assignment.Title}' просрочено. Срок сдачи был: {assignment.DueDate:dd.MM.yyyy}",
                NotificationType.Warning);
        }
    }

    /// <summary>
    /// Отправляет уведомления о приближающихся дедлайнах
    /// </summary>
    public async Task SendUpcomingDeadlineNotificationsAsync()
    {
        var tomorrow = DateTime.UtcNow.AddDays(1);
        var dayAfterTomorrow = DateTime.UtcNow.AddDays(2);

        var upcomingAssignments = await _dbContext.Assignments
            .Where(a => a.DueDate.HasValue && a.DueDate >= tomorrow && a.DueDate <= dayAfterTomorrow)
            .Where(a => a.Status == AssignmentStatus.Published)
            .Include(a => a.CourseInstance)
            .ToListAsync();

        foreach (var assignment in upcomingAssignments)
        {
            await SendNotificationToCourseAsync(
                assignment.CourseInstanceUid,
                "Приближается дедлайн",
                $"Задание '{assignment.Title}' нужно сдать до {assignment.DueDate:dd.MM.yyyy HH:mm}",
                NotificationType.Info);
        }
    }

    /// <summary>
    /// Получает последние уведомления
    /// </summary>
    public async Task<IEnumerable<Notification>> GetRecentNotificationsAsync(int count = 10)
    {
        try
        {
            return await _dbContext.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent notifications");
            return [];
        }
    }

    /// <summary>
    /// Получает все непрочитанные уведомления для пользователя
    /// </summary>
    /// <param name="userUid">UID пользователя</param>
    /// <returns>Список непрочитанных уведомлений</returns>
    public async Task<IEnumerable<Domain.Entities.System.Notification>> GetUnreadNotificationsAsync(Guid userUid)
    {
        return await _dbContext.Notifications
            .Where(n => n.PersonUid == userUid && !n.IsRead)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();
    }
}
