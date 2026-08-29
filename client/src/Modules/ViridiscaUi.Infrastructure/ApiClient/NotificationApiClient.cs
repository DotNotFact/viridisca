using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Infrastructure.ApiClient;

public class NotificationApiClient(HttpClient httpClient, ILogger<NotificationApiClient> logger)
    : ApiClientBase(httpClient, logger)
{
    public Task<(bool Success, Guid Data, string? Error)> CreateAsync(CreateNotificationRequestDto request, CancellationToken cancellationToken = default)
        => PostAsync<CreateNotificationRequestDto, Guid>("api/notifications", request, cancellationToken);

    public Task<(bool Success, List<NotificationResponseDto>? Data, string? Error)> GetRecentAsync(int count, CancellationToken cancellationToken = default)
        => GetAsync<List<NotificationResponseDto>>($"api/notifications/recent?count={count}", cancellationToken);

    public Task<(bool Success, List<NotificationResponseDto>? Data, string? Error)> GetUnreadAsync(Guid recipientUid, CancellationToken cancellationToken = default)
        => GetAsync<List<NotificationResponseDto>>($"api/notifications/unread/{recipientUid}", cancellationToken);

    public async Task<(bool Success, string? Error)> MarkAsReadAsync(Guid notificationUid, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PutAsync<object?, object>($"api/notifications/{notificationUid}/read", null, cancellationToken, expectBody: false);
        return (success, error);
    }

    public Task<(bool Success, int Data, string? Error)> MarkAllAsReadAsync(Guid recipientUid, CancellationToken cancellationToken = default)
        => PutAsync<object?, int>($"api/notifications/read-all/{recipientUid}", null, cancellationToken);

    public Task<(bool Success, string? Error)> DeleteAsync(Guid notificationUid, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/notifications/{notificationUid}", cancellationToken);
}
