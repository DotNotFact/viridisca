using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Entities.System.Enums;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// HTTP-backed INotificationService. Covers the create/read/mark-read/delete surface the
/// backend Notifications API supports — the same surface that was already reachable from
/// real UI actions per the Phase 8 audit (Library/Courses/Curriculum/Exams/Departments/
/// Grades/Students create notifications; HomeViewModel reads recent ones). Role-broadcast
/// (<see cref="SendNotificationToRoleAsync"/>) has zero UI callers and would need a
/// cross-module join into Identity's roles, so it stays on <paramref name="inner"/>. The
/// four `Show*` toast methods are pure client-side UI (no persistence in either
/// implementation) and also stay on <paramref name="inner"/>.
/// </summary>
public class HttpNotificationService(
    NotificationApiClient apiClient,
    NotificationService inner,
    IPersonSessionService personSessionService,
    ILogger<HttpNotificationService> logger) : INotificationService
{
    private readonly NotificationApiClient _apiClient = apiClient;
    private readonly NotificationService _inner = inner;
    private readonly IPersonSessionService _personSessionService = personSessionService;
    private readonly ILogger<HttpNotificationService> _logger = logger;

    public async Task<Notification> CreateNotificationAsync(
        Guid recipientUid,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        NotificationPriority priority = NotificationPriority.Normal,
        string? category = null,
        string? actionUrl = null)
    {
        var request = new CreateNotificationRequestDto(recipientUid, title, message, type.ToString(), priority.ToString(), category, actionUrl);
        var (success, notificationUid, error) = await _apiClient.CreateAsync(request);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to create notification: {error}");
        }

        return new Notification
        {
            Uid = notificationUid,
            PersonUid = recipientUid,
            Title = title,
            Message = message,
            Type = type,
            Priority = priority,
            Category = category,
            ActionUrl = actionUrl,
            SentAt = DateTime.UtcNow,
            IsRead = false,
        };
    }

    public async Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid userUid)
    {
        var (success, data, error) = await _apiClient.GetUnreadAsync(userUid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetUnreadNotificationsAsync({UserUid}) failed: {Error}", userUid, error);
            return [];
        }

        return data.Select(NotificationMappers.ToNotification);
    }

    public async Task<IEnumerable<Notification>> GetRecentNotificationsAsync(int count = 10)
    {
        var (success, data, error) = await _apiClient.GetRecentAsync(count);
        if (!success || data is null)
        {
            _logger.LogWarning("GetRecentNotificationsAsync({Count}) failed: {Error}", count, error);
            return [];
        }

        return data.Select(NotificationMappers.ToNotification);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationUid)
    {
        var (success, error) = await _apiClient.MarkAsReadAsync(notificationUid);
        if (!success)
        {
            _logger.LogWarning("MarkAsReadAsync({NotificationUid}) failed: {Error}", notificationUid, error);
        }

        return success;
    }

    public async Task<int> MarkAllAsReadAsync(Guid userUid)
    {
        var (success, count, error) = await _apiClient.MarkAllAsReadAsync(userUid);
        if (!success)
        {
            _logger.LogWarning("MarkAllAsReadAsync({UserUid}) failed: {Error}", userUid, error);
            return 0;
        }

        return count;
    }

    public async Task<bool> DeleteNotificationAsync(Guid notificationUid)
    {
        var (success, error) = await _apiClient.DeleteAsync(notificationUid);
        if (!success)
        {
            _logger.LogWarning("DeleteNotificationAsync({NotificationUid}) failed: {Error}", notificationUid, error);
        }

        return success;
    }

    public void ShowSuccess(string message) => _inner.ShowSuccess(message);

    public void ShowError(string message) => _inner.ShowError(message);

    public void ShowInfo(string message) => _inner.ShowInfo(message);

    public void ShowWarning(string message) => _inner.ShowWarning(message);

    public async Task SendNotificationAsync(string title, string message, NotificationType type = NotificationType.Info, NotificationPriority priority = NotificationPriority.Normal)
    {
        var currentPersonUid = _personSessionService.CurrentPerson?.Uid;
        if (currentPersonUid is null)
        {
            _logger.LogWarning("SendNotificationAsync({Title}) skipped: no current session person", title);
            return;
        }

        await CreateNotificationAsync(currentPersonUid.Value, title, message, type, priority);
    }

    public Task SendNotificationToRoleAsync(string role, string title, string message, NotificationType type = NotificationType.Info, NotificationPriority priority = NotificationPriority.Normal)
        => _inner.SendNotificationToRoleAsync(role, title, message, type, priority);
}
