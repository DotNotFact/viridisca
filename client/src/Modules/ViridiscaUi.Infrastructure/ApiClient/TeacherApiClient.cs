using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Infrastructure.ApiClient;

/// <summary>
/// Thin HTTP wrapper over the backend's /api/academic/teachers/* endpoints.
/// </summary>
public class TeacherApiClient(HttpClient httpClient, ILogger<TeacherApiClient> logger)
    : ApiClientBase(httpClient, logger)
{
    public Task<(bool Success, Guid Data, string? Error)> CreateAsync(CreateTeacherRequestDto request, CancellationToken cancellationToken = default)
        => PostAsync<CreateTeacherRequestDto, Guid>("api/academic/teachers", request, cancellationToken);

    public Task<(bool Success, TeacherResponseDto? Data, string? Error)> GetAsync(Guid teacherUid, CancellationToken cancellationToken = default)
        => GetAsync<TeacherResponseDto>($"api/academic/teachers/{teacherUid}", cancellationToken);

    public Task<(bool Success, Guid Data, string? Error)> AssignSubjectAsync(Guid teacherUid, AssignSubjectRequestDto request, CancellationToken cancellationToken = default)
        => PostAsync<AssignSubjectRequestDto, Guid>($"api/academic/teachers/{teacherUid}/subjects", request, cancellationToken);
}
