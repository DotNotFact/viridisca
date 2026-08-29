using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Infrastructure.ApiClient;

/// <summary>
/// Thin HTTP wrapper over the backend's /api/academic/students/* endpoints
/// (see server/src/Modules/Academic/Viridisca.Modules.Academic.Presentation/Students).
/// </summary>
public class StudentApiClient(HttpClient httpClient, ILogger<StudentApiClient> logger)
    : ApiClientBase(httpClient, logger)
{
    public Task<(bool Success, Guid Data, string? Error)> CreateAsync(CreateStudentRequestDto request, CancellationToken cancellationToken = default)
        => PostGuidAsync("api/academic/students", request, cancellationToken);

    public Task<(bool Success, StudentResponseDto? Data, string? Error)> GetAsync(Guid studentUid, CancellationToken cancellationToken = default)
        => GetAsync<StudentResponseDto>($"api/academic/students/{studentUid}", cancellationToken);

    public async Task<(bool Success, string? Error)> UpdateAsync(Guid studentUid, UpdateStudentRequestDto request, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PutAsync<UpdateStudentRequestDto, object>($"api/academic/students/{studentUid}", request, cancellationToken, expectBody: false);
        return (success, error);
    }

    public async Task<(bool Success, string? Error)> AssignToGroupAsync(Guid studentUid, Guid groupUid, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PostAsync<AssignToGroupRequestDto, object>($"api/academic/students/{studentUid}/group", new AssignToGroupRequestDto(groupUid), cancellationToken, expectBody: false);
        return (success, error);
    }

    public Task<(bool Success, Guid Data, string? Error)> AddParentAsync(Guid studentUid, AddParentRequestDto request, CancellationToken cancellationToken = default)
        => PostGuidAsync($"api/academic/students/{studentUid}/parents", request, cancellationToken);

    private async Task<(bool Success, Guid Data, string? Error)> PostGuidAsync<TRequest>(string url, TRequest request, CancellationToken cancellationToken)
    {
        var (success, data, error) = await PostAsync<TRequest, Guid>(url, request, cancellationToken);
        return (success, data, error);
    }
}
