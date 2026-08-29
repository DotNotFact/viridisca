using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Infrastructure.ApiClient;

public class AssignmentApiClient(HttpClient httpClient, ILogger<AssignmentApiClient> logger)
    : ApiClientBase(httpClient, logger)
{
    public Task<(bool Success, Guid Data, string? Error)> CreateAsync(CreateAssignmentRequestDto request, CancellationToken cancellationToken = default)
        => PostAsync<CreateAssignmentRequestDto, Guid>("api/curriculum/assignments", request, cancellationToken);

    public Task<(bool Success, AssignmentResponseDto? Data, string? Error)> GetAsync(Guid assignmentUid, CancellationToken cancellationToken = default)
        => GetAsync<AssignmentResponseDto>($"api/curriculum/assignments/{assignmentUid}", cancellationToken);

    public async Task<(bool Success, string? Error)> UpdateAsync(Guid assignmentUid, UpdateAssignmentRequestDto request, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PutAsync<UpdateAssignmentRequestDto, object>($"api/curriculum/assignments/{assignmentUid}", request, cancellationToken, expectBody: false);
        return (success, error);
    }

    public async Task<(bool Success, string? Error)> PublishAsync(Guid assignmentUid, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PostAsync<object?, object>($"api/curriculum/assignments/{assignmentUid}/publish", null, cancellationToken, expectBody: false);
        return (success, error);
    }

    public Task<(bool Success, List<AssignmentResponseDto>? Data, string? Error)> GetByCourseInstanceAsync(Guid courseInstanceUid, CancellationToken cancellationToken = default)
        => GetAsync<List<AssignmentResponseDto>>($"api/curriculum/assignments/by-course-instance/{courseInstanceUid}", cancellationToken);
}
