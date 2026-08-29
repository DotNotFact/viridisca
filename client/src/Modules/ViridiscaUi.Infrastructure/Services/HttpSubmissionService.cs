using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.ApiClient;
using ViridiscaUi.Services;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// HTTP-backed ISubmissionService. Note: per the Phase 4 research, no ViewModel in the
/// UI currently calls ISubmissionService at all — this exists for the Assignment.Submissions
/// collection and grading flow, so it's exercised indirectly rather than through a screen.
/// </summary>
public class HttpSubmissionService(SubmissionApiClient apiClient, SubmissionService inner, ILogger<HttpSubmissionService> logger) : ISubmissionService
{
    private readonly SubmissionApiClient _apiClient = apiClient;
    private readonly SubmissionService _inner = inner;
    private readonly ILogger<HttpSubmissionService> _logger = logger;

    public async Task<Submission?> GetSubmissionAsync(Guid uid)
    {
        var (success, data, error) = await _apiClient.GetAsync(uid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetSubmissionAsync({SubmissionUid}) failed: {Error}", uid, error);
            return null;
        }

        return CurriculumMappers.ToSubmission(data);
    }

    public Task<IEnumerable<Submission>> GetAllSubmissionsAsync() => _inner.GetAllSubmissionsAsync();

    public async Task<IEnumerable<Submission>> GetSubmissionsByStudentAsync(Guid studentUid)
    {
        var (success, data, error) = await _apiClient.GetByStudentAsync(studentUid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetSubmissionsByStudentAsync({StudentUid}) failed: {Error}", studentUid, error);
            return [];
        }

        return data.Select(CurriculumMappers.ToSubmission);
    }

    public async Task<IEnumerable<Submission>> GetSubmissionsByAssignmentAsync(Guid assignmentUid)
    {
        var (success, data, error) = await _apiClient.GetByAssignmentAsync(assignmentUid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetSubmissionsByAssignmentAsync({AssignmentUid}) failed: {Error}", assignmentUid, error);
            return [];
        }

        return data.Select(CurriculumMappers.ToSubmission);
    }

    public Task<Submission?> GetSubmissionByStudentAndAssignmentAsync(Guid studentUid, Guid assignmentUid) => _inner.GetSubmissionByStudentAndAssignmentAsync(studentUid, assignmentUid);

    public async Task AddSubmissionAsync(Submission submission)
    {
        var request = new CreateSubmissionRequestDto(submission.AssignmentUid, submission.StudentUid, submission.Content, submission.FilePath);
        var (success, submissionUid, error) = await _apiClient.CreateAsync(request);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to create submission: {error}");
        }

        submission.Uid = submissionUid;
    }

    public Task<bool> UpdateSubmissionAsync(Submission submission)
    {
        // No PUT endpoint for submission content on the backend yet — falls back to EF.
        return _inner.UpdateSubmissionAsync(submission);
    }

    public Task<bool> DeleteSubmissionAsync(Guid uid)
    {
        _logger.LogWarning("DeleteSubmissionAsync is not supported by the backend yet (no DELETE endpoint)");
        return _inner.DeleteSubmissionAsync(uid);
    }

    public async Task<bool> GradeSubmissionAsync(Guid uid, int grade, string feedback)
    {
        // ISubmissionService's grade param is a bare int with no grader identity — the
        // backend's GradeSubmission requires GradedByUid. There's no session/current-user
        // concept threaded through this interface, so this falls back to EF, which at
        // least has that context via its own tracked entities.
        return await _inner.GradeSubmissionAsync(uid, grade, feedback);
    }
}
