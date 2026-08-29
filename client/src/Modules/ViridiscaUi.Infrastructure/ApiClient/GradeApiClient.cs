using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Infrastructure.ApiClient;

/// <summary>
/// Thin HTTP wrapper over the backend's /api/grading/grades/* endpoints.
/// </summary>
public class GradeApiClient(HttpClient httpClient, ILogger<GradeApiClient> logger)
    : ApiClientBase(httpClient, logger)
{
    public Task<(bool Success, Guid Data, string? Error)> CreateAsync(CreateGradeRequestDto request, CancellationToken cancellationToken = default)
        => PostAsync<CreateGradeRequestDto, Guid>("api/grading/grades", request, cancellationToken);

    public Task<(bool Success, GradeResponseDto? Data, string? Error)> GetAsync(Guid gradeUid, CancellationToken cancellationToken = default)
        => GetAsync<GradeResponseDto>($"api/grading/grades/{gradeUid}", cancellationToken);

    public async Task<(bool Success, string? Error)> UpdateAsync(Guid gradeUid, UpdateGradeRequestDto request, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PutAsync<UpdateGradeRequestDto, object>($"api/grading/grades/{gradeUid}", request, cancellationToken, expectBody: false);
        return (success, error);
    }

    public async Task<(bool Success, string? Error)> PublishAsync(Guid gradeUid, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PostAsync<object?, object>($"api/grading/grades/{gradeUid}/publish", null, cancellationToken, expectBody: false);
        return (success, error);
    }

    public async Task<(bool Success, string? Error)> UnpublishAsync(Guid gradeUid, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PostAsync<object?, object>($"api/grading/grades/{gradeUid}/unpublish", null, cancellationToken, expectBody: false);
        return (success, error);
    }

    public Task<(bool Success, List<GradeResponseDto>? Data, string? Error)> GetByStudentAsync(Guid studentUid, CancellationToken cancellationToken = default)
        => GetAsync<List<GradeResponseDto>>($"api/grading/grades/by-student/{studentUid}", cancellationToken);

    public Task<(bool Success, List<GradeResponseDto>? Data, string? Error)> GetBySubjectAsync(Guid subjectUid, CancellationToken cancellationToken = default)
        => GetAsync<List<GradeResponseDto>>($"api/grading/grades/by-subject/{subjectUid}", cancellationToken);
}
