using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Infrastructure.ApiClient;

public class CourseInstanceApiClient(HttpClient httpClient, ILogger<CourseInstanceApiClient> logger)
    : ApiClientBase(httpClient, logger)
{
    public Task<(bool Success, Guid Data, string? Error)> CreateAsync(CreateCourseInstanceRequestDto request, CancellationToken cancellationToken = default)
        => PostAsync<CreateCourseInstanceRequestDto, Guid>("api/curriculum/course-instances", request, cancellationToken);

    public Task<(bool Success, CourseInstanceResponseDto? Data, string? Error)> GetAsync(Guid courseInstanceUid, CancellationToken cancellationToken = default)
        => GetAsync<CourseInstanceResponseDto>($"api/curriculum/course-instances/{courseInstanceUid}", cancellationToken);

    public Task<(bool Success, List<CourseInstanceResponseDto>? Data, string? Error)> GetAllAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<CourseInstanceResponseDto>>("api/curriculum/course-instances", cancellationToken);

    public async Task<(bool Success, string? Error)> UpdateAsync(Guid courseInstanceUid, UpdateCourseInstanceRequestDto request, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PutAsync<UpdateCourseInstanceRequestDto, object>($"api/curriculum/course-instances/{courseInstanceUid}", request, cancellationToken, expectBody: false);
        return (success, error);
    }

    public async Task<(bool Success, string? Error)> AssignTeacherAsync(Guid courseInstanceUid, Guid? teacherUid, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PutAsync<AssignTeacherRequestDto, object>($"api/curriculum/course-instances/{courseInstanceUid}/teacher", new AssignTeacherRequestDto(teacherUid), cancellationToken, expectBody: false);
        return (success, error);
    }

    public Task<(bool Success, List<CourseInstanceResponseDto>? Data, string? Error)> GetByGroupAsync(Guid groupUid, CancellationToken cancellationToken = default)
        => GetAsync<List<CourseInstanceResponseDto>>($"api/curriculum/course-instances/by-group/{groupUid}", cancellationToken);

    public Task<(bool Success, List<CourseInstanceResponseDto>? Data, string? Error)> GetByTeacherAsync(Guid teacherUid, CancellationToken cancellationToken = default)
        => GetAsync<List<CourseInstanceResponseDto>>($"api/curriculum/course-instances/by-teacher/{teacherUid}", cancellationToken);
}
