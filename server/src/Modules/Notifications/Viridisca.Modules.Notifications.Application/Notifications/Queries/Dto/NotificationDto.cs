using System;

namespace Viridisca.Modules.Notifications.Application.Notifications.Queries.Dto;

public sealed class NotificationDto
{
    public Guid Uid { get; set; }
    public Guid RecipientUid { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
