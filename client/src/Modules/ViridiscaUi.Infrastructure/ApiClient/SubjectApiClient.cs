using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Infrastructure.ApiClient;

/// <summary>
/// Thin HTTP wrapper over the backend's /api/academic/subjects/* endpoints.
/// </summary>
public class SubjectApiClient(HttpClient httpClient, ILogger<SubjectApiClient> logger)
    : ApiClientBase(httpClient, logger)
{
    public Task<(bool Success, Guid Data, string? Error)> CreateAsync(CreateSubjectRequestDto request, CancellationToken cancellationToken = default)
        => PostAsync<CreateSubjectRequestDto, Guid>("api/academic/subjects", request, cancellationToken);

    public Task<(bool Success, SubjectResponseDto? Data, string? Error)> GetAsync(Guid subjectUid, CancellationToken cancellationToken = default)
        => GetAsync<SubjectResponseDto>($"api/academic/subjects/{subjectUid}", cancellationToken);

    public Task<(bool Success, List<SubjectResponseDto>? Data, string? Error)> GetAllAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<SubjectResponseDto>>("api/academic/subjects", cancellationToken);

    public async Task<(bool Success, string? Error)> UpdateAsync(Guid subjectUid, UpdateSubjectRequestDto request, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PutAsync<UpdateSubjectRequestDto, object>($"api/academic/subjects/{subjectUid}", request, cancellationToken, expectBody: false);
        return (success, error);
    }
}
