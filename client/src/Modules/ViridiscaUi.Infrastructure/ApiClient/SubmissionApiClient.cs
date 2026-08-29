using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Infrastructure.ApiClient;

public class SubmissionApiClient(HttpClient httpClient, ILogger<SubmissionApiClient> logger)
    : ApiClientBase(httpClient, logger)
{
    public Task<(bool Success, Guid Data, string? Error)> CreateAsync(CreateSubmissionRequestDto request, CancellationToken cancellationToken = default)
        => PostAsync<CreateSubmissionRequestDto, Guid>("api/curriculum/submissions", request, cancellationToken);

    public Task<(bool Success, SubmissionResponseDto? Data, string? Error)> GetAsync(Guid submissionUid, CancellationToken cancellationToken = default)
        => GetAsync<SubmissionResponseDto>($"api/curriculum/submissions/{submissionUid}", cancellationToken);

    public async Task<(bool Success, string? Error)> GradeAsync(Guid submissionUid, GradeSubmissionRequestDto request, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PostAsync<GradeSubmissionRequestDto, object>($"api/curriculum/submissions/{submissionUid}/grade", request, cancellationToken, expectBody: false);
        return (success, error);
    }

    public Task<(bool Success, List<SubmissionResponseDto>? Data, string? Error)> GetByStudentAsync(Guid studentUid, CancellationToken cancellationToken = default)
        => GetAsync<List<SubmissionResponseDto>>($"api/curriculum/submissions/by-student/{studentUid}", cancellationToken);

    public Task<(bool Success, List<SubmissionResponseDto>? Data, string? Error)> GetByAssignmentAsync(Guid assignmentUid, CancellationToken cancellationToken = default)
        => GetAsync<List<SubmissionResponseDto>>($"api/curriculum/submissions/by-assignment/{assignmentUid}", cancellationToken);
}
