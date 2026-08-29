using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Entities.System.Enums;

namespace ViridiscaUi.Domain.Services.Notification;

/// <summary>
/// Интерфейс сервиса уведомлений для создания и управления уведомлениями в системе
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Создать новое уведомление и сохранить в базу данных
    /// </summary>
    /// <param name="recipientUid">UID получателя уведомления</param>
    /// <param name="title">Заголовок уведомления</param>
    /// <param name="message">Текст уведомления</param>
    /// <param name="type">Тип уведомления</param>
    /// <param name="priority">Приоритет уведомления</param>
    /// <param name="category">Категория уведомления</param>
    /// <param name="actionUrl">URL для действия</param>
    /// <returns>Созданное уведомление</returns>
    Task<Domain.Entities.System.Notification> CreateNotificationAsync(
        Guid recipientUid, 
        string title, 
        string message, 
        NotificationType type = NotificationType.Info, 
        NotificationPriority priority = NotificationPriority.Normal,
        string? category = null,
        string? actionUrl = null);

    /// <summary>
    /// Получить все непрочитанные уведомления для пользователя
    /// </summary>
    /// <param name="userUid">UID пользователя</param>
    /// <returns>Список непрочитанных уведомлений</returns>
    Task<IEnumerable<Domain.Entities.System.Notification>> GetUnreadNotificationsAsync(Guid userUid);

    /// <summary>
    /// Получить последние уведомления
    /// </summary>
    /// <param name="count">Количество уведомлений</param>
    /// <returns>Список последних уведомлений</returns>
    Task<IEnumerable<Domain.Entities.System.Notification>> GetRecentNotificationsAsync(int count = 10);

    /// <summary>
    /// Отметить уведомление как прочитанное
    /// </summary>
    /// <param name="notificationUid">UID уведомления</param>
    /// <returns>True если успешно обновлено</returns>
    Task<bool> MarkAsReadAsync(Guid notificationUid);

    /// <summary>
    /// Отметить все уведомления пользователя как прочитанные
    /// </summary>
    /// <param name="userUid">UID пользователя</param>
    /// <returns>Количество обновленных уведомлений</returns>
    Task<int> MarkAllAsReadAsync(Guid userUid);

    /// <summary>
    /// Удалить уведомление (мягкое удаление)
    /// </summary>
    /// <param name="notificationUid">UID уведомления для удаления</param>
    /// <returns>True если успешно удалено</returns>
    Task<bool> DeleteNotificationAsync(Guid notificationUid);
    
    // UI Notification Methods
    /// <summary>
    /// Показывает уведомление об успехе
    /// </summary>
    void ShowSuccess(string message);
    
    /// <summary>
    /// Показывает уведомление об ошибке
    /// </summary>
    void ShowError(string message);
    
    /// <summary>
    /// Показывает информационное уведомление
    /// </summary>
    void ShowInfo(string message);
    
    /// <summary>
    /// Показывает предупреждение
    /// </summary>
    void ShowWarning(string message);
    
    /// <summary>
    /// Отправляет общее уведомление
    /// </summary>
    Task SendNotificationAsync(string title, string message, NotificationType type = NotificationType.Info, NotificationPriority priority = NotificationPriority.Normal);
    
    /// <summary>
    /// Отправляет уведомление пользователям с определенной ролью
    /// </summary>
    Task SendNotificationToRoleAsync(string role, string title, string message, NotificationType type = NotificationType.Info, NotificationPriority priority = NotificationPriority.Normal);
}