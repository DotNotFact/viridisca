using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Entities.System.Enums;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Maps backend Notifications API DTOs to the client's own <see cref="Notification"/>.
/// Both NotificationType and NotificationPriority match the backend member-for-member,
/// so plain Enum.Parse is safe (same approach as CurriculumMappers/SchedulerMappers).
/// </summary>
internal static class NotificationMappers
{
    public static Notification ToNotification(NotificationResponseDto dto) => new()
    {
        Uid = dto.Uid,
        PersonUid = dto.RecipientUid,
        Title = dto.Title,
        Message = dto.Message,
        Type = Enum.Parse<NotificationType>(dto.Type),
        Priority = Enum.Parse<NotificationPriority>(dto.Priority),
        Category = dto.Category,
        ActionUrl = dto.ActionUrl,
        IsRead = dto.IsRead,
        SentAt = dto.SentAtUtc,
        ReadAt = dto.ReadAtUtc,
    };
}
