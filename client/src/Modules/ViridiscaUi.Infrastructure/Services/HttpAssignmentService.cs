using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Models;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// HTTP-backed IAssignmentService for Create/GetByUid/Update/Publish/
/// GetByCourseInstance — the operations the backend Curriculum API supports.
/// No list-all/search/paging endpoint exists server-side, and analytics/reminders/
/// pending-grading/duplicate/archive are entirely out of the backend's current scope,
/// so those fall through to <paramref name="inner"/>, the EF-backed AssignmentService.
/// </summary>
public class HttpAssignmentService(AssignmentApiClient apiClient, AssignmentService inner, ILogger<HttpAssignmentService> logger) : IAssignmentService
{
    private readonly AssignmentApiClient _apiClient = apiClient;
    private readonly AssignmentService _inner = inner;
    private readonly ILogger<HttpAssignmentService> _logger = logger;

    public Task<IEnumerable<Assignment>> GetAllAsync() => _inner.GetAllAsync();

    public async Task<Assignment?> GetByIdAsync(Guid assignmentUid)
    {
        var (success, data, error) = await _apiClient.GetAsync(assignmentUid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetByIdAsync({AssignmentUid}) failed: {Error}", assignmentUid, error);
            return null;
        }

        return CurriculumMappers.ToAssignment(data);
    }

    public async Task<Assignment> CreateAsync(Assignment assignment)
    {
        var request = new CreateAssignmentRequestDto(
            assignment.CourseInstanceUid, assignment.Title, assignment.Description, assignment.Type.ToString(),
            (decimal)assignment.MaxScore, assignment.DueDate, assignment.Instructions, assignment.Difficulty.ToString());

        var (success, assignmentUid, error) = await _apiClient.CreateAsync(request);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to create assignment: {error}");
        }

        assignment.Uid = assignmentUid;
        return assignment;
    }

    public async Task<Assignment> UpdateAsync(Assignment assignment)
    {
        var request = new UpdateAssignmentRequestDto(assignment.Title, assignment.Description, assignment.Instructions, assignment.DueDate, (decimal)assignment.MaxScore);
        var (success, error) = await _apiClient.UpdateAsync(assignment.Uid, request);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to update assignment: {error}");
        }

        return assignment;
    }

    public Task<bool> DeleteAsync(Guid assignmentUid)
    {
        _logger.LogWarning("DeleteAsync is not supported by the backend yet (no DELETE endpoint for assignments)");
        return _inner.DeleteAsync(assignmentUid);
    }

    public async Task<IEnumerable<Assignment>> GetByCourseInstanceAsync(Guid courseInstanceUid)
    {
        var (success, data, error) = await _apiClient.GetByCourseInstanceAsync(courseInstanceUid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetByCourseInstanceAsync({CourseInstanceUid}) failed: {Error}", courseInstanceUid, error);
            return [];
        }

        return data.Select(CurriculumMappers.ToAssignment);
    }

    public Task<IEnumerable<Assignment>> GetByStudentAsync(Guid studentUid) => _inner.GetByStudentAsync(studentUid);

    public Task<IEnumerable<Assignment>> GetOverdueAsync() => _inner.GetOverdueAsync();

    public Task<IEnumerable<Assignment>> GetUpcomingDeadlinesAsync(int days) => _inner.GetUpcomingDeadlinesAsync(days);

    public Task<ValidationResult> ValidateAsync(Assignment assignment) => _inner.ValidateAsync(assignment);

    public Task<IEnumerable<Assignment>> GetAssignmentsByCourseInstanceAsync(Guid courseInstanceUid) => GetByCourseInstanceAsync(courseInstanceUid);

    public Task<IEnumerable<Assignment>> GetAssignmentsByStatusAsync(AssignmentStatus status) => _inner.GetAssignmentsByStatusAsync(status);

    public Task<IEnumerable<Assignment>> GetAssignmentsByTypeAsync(AssignmentType type) => _inner.GetAssignmentsByTypeAsync(type);

    public Task<IEnumerable<Assignment>> GetAssignmentsByDifficultyAsync(AssignmentDifficulty difficulty) => _inner.GetAssignmentsByDifficultyAsync(difficulty);

    public Task<IEnumerable<Assignment>> GetUpcomingAssignmentsAsync(DateTime? beforeDate = null) => _inner.GetUpcomingAssignmentsAsync(beforeDate);

    public Task<IEnumerable<Assignment>> GetOverdueAssignmentsAsync() => _inner.GetOverdueAssignmentsAsync();

    public Task<IEnumerable<Assignment>> SearchAssignmentsAsync(string searchTerm) => _inner.SearchAssignmentsAsync(searchTerm);

    public Task<(IEnumerable<Assignment> Assignments, int TotalCount)> GetAssignmentsPagedAsync(int page, int pageSize, string? searchTerm = null, Guid? courseInstanceUid = null, AssignmentStatus? status = null)
        => _inner.GetAssignmentsPagedAsync(page, pageSize, searchTerm, courseInstanceUid, status);

    public Task<(IEnumerable<Assignment> Assignments, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null, Guid? courseInstanceUid = null, AssignmentStatus? status = null)
        => _inner.GetPagedAsync(page, pageSize, searchTerm, courseInstanceUid, status);

    public Task<AssignmentAnalytics> GetAnalyticsAsync(Guid assignmentUid) => _inner.GetAnalyticsAsync(assignmentUid);

    public Task<Assignment?> GetByTitleAndCourseAsync(string title, Guid courseInstanceUid) => _inner.GetByTitleAndCourseAsync(title, courseInstanceUid);

    public Task<int> GetGradesCountAsync(Guid assignmentUid) => _inner.GetGradesCountAsync(assignmentUid);

    public Task<AssignmentAnalytics> GetAssignmentAnalyticsAsync(Guid assignmentUid) => _inner.GetAssignmentAnalyticsAsync(assignmentUid);

    public Task<IEnumerable<Submission>> GetSubmissionsByAssignmentAsync(Guid assignmentUid) => _inner.GetSubmissionsByAssignmentAsync(assignmentUid);

    public Task SendDueDateReminderAsync(Guid assignmentUid) => _inner.SendDueDateReminderAsync(assignmentUid);

    public Task<IEnumerable<Assignment>> GetAssignmentsPendingGradingAsync() => _inner.GetAssignmentsPendingGradingAsync();

    public async Task<bool> PublishAssignmentAsync(Guid assignmentUid)
    {
        var (success, error) = await _apiClient.PublishAsync(assignmentUid);
        if (!success)
        {
            _logger.LogWarning("PublishAssignmentAsync({AssignmentUid}) failed: {Error}", assignmentUid, error);
        }

        return success;
    }

    public Task<bool> UnpublishAssignmentAsync(Guid assignmentUid)
    {
        // Backend can publish but has no unpublish endpoint yet.
        _logger.LogWarning("UnpublishAssignmentAsync is not supported by the backend yet");
        return _inner.UnpublishAssignmentAsync(assignmentUid);
    }

    public Task<bool> ArchiveAssignmentAsync(Guid assignmentUid) => _inner.ArchiveAssignmentAsync(assignmentUid);

    public Task<bool> DuplicateAssignmentAsync(Guid assignmentUid, string newTitle) => _inner.DuplicateAssignmentAsync(assignmentUid, newTitle);

    public Task<int> GetSubmissionsCountAsync(Guid assignmentUid) => _inner.GetSubmissionsCountAsync(assignmentUid);

    public Task<IEnumerable<Submission>> GetSubmissionsAsync(Guid assignmentUid) => _inner.GetSubmissionsAsync(assignmentUid);

    public Task<double> GetAverageGradeAsync(Guid assignmentUid) => _inner.GetAverageGradeAsync(assignmentUid);

    public Task<bool> SetMaxPointsAsync(Guid assignmentUid, int maxPoints) => _inner.SetMaxPointsAsync(assignmentUid, maxPoints);

    public Task<bool> UpdateDueDateAsync(Guid assignmentUid, DateTime? dueDate) => _inner.UpdateDueDateAsync(assignmentUid, dueDate);

    public Task<IEnumerable<Assignment>> GetAssignmentsByTeacherAsync(Guid teacherUid) => _inner.GetAssignmentsByTeacherAsync(teacherUid);

    public Task<IEnumerable<Assignment>> GetAssignmentsByStudentAsync(Guid studentUid) => _inner.GetAssignmentsByStudentAsync(studentUid);

    public Task<bool> CanStudentSubmitAsync(Guid assignmentUid, Guid studentUid) => _inner.CanStudentSubmitAsync(assignmentUid, studentUid);

    public Task<Assignment?> GetAssignmentWithSubmissionsAsync(Guid assignmentUid) => _inner.GetAssignmentWithSubmissionsAsync(assignmentUid);

    public Task<bool> IsAssignmentOverdueAsync(Guid assignmentUid) => _inner.IsAssignmentOverdueAsync(assignmentUid);

    public Task<TimeSpan?> GetTimeUntilDueAsync(Guid assignmentUid) => _inner.GetTimeUntilDueAsync(assignmentUid);
}
