using System;
using Viridisca.Common.Domain;

namespace Viridisca.Modules.Notifications.Domain.Models;

/// <summary>
/// Уведомление пользователя
/// </summary>
public class Notification : Entity
{
    public Guid Uid { get; private set; }
    public Guid RecipientUid { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public string? Category { get; private set; }
    public string? ActionUrl { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime SentAtUtc { get; private set; }
    public DateTime? ReadAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    protected Notification() { }

    public static Result<Notification> Create(
        Guid recipientUid,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        NotificationPriority priority = NotificationPriority.Normal,
        string? category = null,
        string? actionUrl = null)
    {
        if (recipientUid == Guid.Empty)
            return Result.Failure<Notification>(new Error("RecipientUid.Empty", "ID получателя не может быть пустым", ErrorType.Validation));

        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Notification>(new Error("Title.Empty", "Заголовок уведомления не может быть пустым", ErrorType.Validation));

        var notification = new Notification
        {
            Uid = Guid.NewGuid(),
            RecipientUid = recipientUid,
            Title = title.Trim(),
            Message = message,
            Type = type,
            Priority = priority,
            Category = category,
            ActionUrl = actionUrl,
            IsRead = false,
            SentAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };

        notification.Raise(new NotificationCreatedDomainEvent(notification.Uid, notification.RecipientUid));
        return notification;
    }

    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAtUtc = DateTime.UtcNow;
    }
}

public enum NotificationType
{
    Info,
    Warning,
    Error,
    Success,
    System,
    Grade,
    Attendance,
    Assignment,
    Reminder
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Critical
}
