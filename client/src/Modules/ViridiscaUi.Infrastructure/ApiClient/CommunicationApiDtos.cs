namespace ViridiscaUi.Infrastructure.ApiClient;

public sealed record CreateNotificationRequestDto(
    Guid RecipientUid,
    string Title,
    string Message,
    string Type,
    string Priority,
    string? Category,
    string? ActionUrl);

public sealed record NotificationResponseDto(
    Guid Uid,
    Guid RecipientUid,
    string Title,
    string Message,
    string Type,
    string Priority,
    string? Category,
    string? ActionUrl,
    bool IsRead,
    DateTime SentAtUtc,
    DateTime? ReadAtUtc);
