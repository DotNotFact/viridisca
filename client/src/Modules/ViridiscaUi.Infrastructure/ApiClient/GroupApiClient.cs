using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Infrastructure.ApiClient;

/// <summary>
/// Thin HTTP wrapper over the backend's /api/academic/groups/* endpoints.
/// </summary>
public class GroupApiClient(HttpClient httpClient, ILogger<GroupApiClient> logger)
    : ApiClientBase(httpClient, logger)
{
    public Task<(bool Success, Guid Data, string? Error)> CreateAsync(CreateGroupRequestDto request, CancellationToken cancellationToken = default)
        => PostAsync<CreateGroupRequestDto, Guid>("api/academic/groups", request, cancellationToken);

    public Task<(bool Success, GroupResponseDto? Data, string? Error)> GetAsync(Guid groupUid, CancellationToken cancellationToken = default)
        => GetAsync<GroupResponseDto>($"api/academic/groups/{groupUid}", cancellationToken);

    public Task<(bool Success, List<GroupResponseDto>? Data, string? Error)> GetAllAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<GroupResponseDto>>("api/academic/groups", cancellationToken);

    public async Task<(bool Success, string? Error)> UpdateAsync(Guid groupUid, UpdateGroupRequestDto request, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PutAsync<UpdateGroupRequestDto, object>($"api/academic/groups/{groupUid}", request, cancellationToken, expectBody: false);
        return (success, error);
    }

    public async Task<(bool Success, string? Error)> SetCuratorAsync(Guid groupUid, Guid? curatorUid, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PutAsync<SetCuratorRequestDto, object>($"api/academic/groups/{groupUid}/curator", new SetCuratorRequestDto(curatorUid), cancellationToken, expectBody: false);
        return (success, error);
    }
}
