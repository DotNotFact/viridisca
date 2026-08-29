using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// HTTP-backed IGradeService for Create/Update/Publish/Unpublish/GetByUid/GetByStudent/
/// GetBySubject — the operations the backend Grading API supports (see PROGRESS.md
/// Phase 3). Everything else (course-instance/assignment/exam-scoped queries, averages,
/// statistics, bulk-publish, paging) falls through to <paramref name="inner"/>, the
/// EF-backed GradeService.
///
/// Important caveat: backend-created grades live in the server's Postgres `grading.grades`
/// table; <paramref name="inner"/> reads the client's own local `ApplicationDbContext`
/// table. These are two different data sets — a grade created via CreateAsync here won't
/// show up in GetAllAsync (which still reads local EF) until that method is HTTP-backed
/// too. Same trade-off already accepted for Student/Teacher/Group/Subject in Phase 2.
/// </summary>
public class HttpGradeService(GradeApiClient apiClient, GradeService inner, ILogger<HttpGradeService> logger) : IGradeService
{
    private readonly GradeApiClient _apiClient = apiClient;
    private readonly GradeService _inner = inner;
    private readonly ILogger<HttpGradeService> _logger = logger;

    public async Task<Grade?> GetByUidAsync(Guid uid)
    {
        var (success, data, error) = await _apiClient.GetAsync(uid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetByUidAsync({GradeUid}) failed: {Error}", uid, error);
            return null;
        }

        return GradingMappers.ToGrade(data);
    }

    public Task<IEnumerable<Grade>> GetAllAsync() => _inner.GetAllAsync();

    public async Task<Grade> CreateAsync(Grade grade)
    {
        var request = new CreateGradeRequestDto(grade.StudentUid, grade.SubjectUid, grade.TeacherUid, grade.Value, grade.Type.ToString(), grade.Description);
        var (success, gradeUid, error) = await _apiClient.CreateAsync(request);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to create grade: {error}");
        }

        grade.Uid = gradeUid;
        return grade;
    }

    public async Task<bool> UpdateAsync(Grade grade)
    {
        var (success, error) = await _apiClient.UpdateAsync(grade.Uid, new UpdateGradeRequestDto(grade.Value, grade.Description));
        if (!success)
        {
            _logger.LogWarning("UpdateAsync({GradeUid}) failed: {Error}", grade.Uid, error);
        }

        return success;
    }

    public Task<bool> DeleteAsync(Guid uid)
    {
        // No DELETE endpoint on the backend yet — falls back to EF so the UI's delete
        // action still works for locally-sourced grades rather than silently no-op'ing.
        return _inner.DeleteAsync(uid);
    }

    public Task<(IEnumerable<Grade> Grades, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null, Guid? studentUid = null, Guid? courseInstanceUid = null)
        => _inner.GetPagedAsync(page, pageSize, searchTerm, studentUid, courseInstanceUid);

    public Task<Grade?> GetByStudentAndAssignmentAsync(Guid studentUid, Guid assignmentUid) => _inner.GetByStudentAndAssignmentAsync(studentUid, assignmentUid);

    public async Task<IEnumerable<Grade>> GetGradesByStudentAsync(Guid studentUid)
    {
        var (success, data, error) = await _apiClient.GetByStudentAsync(studentUid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetGradesByStudentAsync({StudentUid}) failed: {Error}", studentUid, error);
            return [];
        }

        return data.Select(GradingMappers.ToGrade);
    }

    public Task<IEnumerable<Grade>> GetGradesByTeacherAsync(Guid teacherUid) => _inner.GetGradesByTeacherAsync(teacherUid);

    public Task<IEnumerable<Grade>> GetGradesByCourseInstanceAsync(Guid courseInstanceUid) => _inner.GetGradesByCourseInstanceAsync(courseInstanceUid);

    public Task<IEnumerable<Grade>> GetGradesByAssignmentAsync(Guid assignmentUid) => _inner.GetGradesByAssignmentAsync(assignmentUid);

    public async Task<IEnumerable<Grade>> GetGradesBySubjectAsync(Guid subjectUid)
    {
        var (success, data, error) = await _apiClient.GetBySubjectAsync(subjectUid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetGradesBySubjectAsync({SubjectUid}) failed: {Error}", subjectUid, error);
            return [];
        }

        return data.Select(GradingMappers.ToGrade);
    }

    public Task<IEnumerable<Grade>> GetGradesByTypeAsync(GradeType gradeType) => _inner.GetGradesByTypeAsync(gradeType);

    public Task<double> GetStudentAverageGradeAsync(Guid studentUid) => _inner.GetStudentAverageGradeAsync(studentUid);

    public Task<double> GetCourseInstanceAverageGradeAsync(Guid courseInstanceUid) => _inner.GetCourseInstanceAverageGradeAsync(courseInstanceUid);

    public Task<(IEnumerable<Grade> Grades, int TotalCount)> GetGradesPagedAsync(int page, int pageSize, string? searchTerm = null, Guid? studentUid = null, Guid? courseInstanceUid = null)
        => _inner.GetGradesPagedAsync(page, pageSize, searchTerm, studentUid, courseInstanceUid);

    public async Task<bool> PublishGradeAsync(Guid gradeUid)
    {
        var (success, error) = await _apiClient.PublishAsync(gradeUid);
        if (!success)
        {
            _logger.LogWarning("PublishGradeAsync({GradeUid}) failed: {Error}", gradeUid, error);
        }

        return success;
    }

    public async Task<bool> UnpublishGradeAsync(Guid gradeUid)
    {
        var (success, error) = await _apiClient.UnpublishAsync(gradeUid);
        if (!success)
        {
            _logger.LogWarning("UnpublishGradeAsync({GradeUid}) failed: {Error}", gradeUid, error);
        }

        return success;
    }

    public Task<(decimal AverageGrade, decimal MaxGrade, decimal MinGrade, int TotalGrades, Dictionary<string, int> GradeDistribution)> GetGradeStatisticsAsync(Guid? courseInstanceUid = null, Guid? studentUid = null)
        => _inner.GetGradeStatisticsAsync(courseInstanceUid, studentUid);

    public Task<IEnumerable<Grade>> GetRecentGradesAsync(int count = 10) => _inner.GetRecentGradesAsync(count);

    public Task<IEnumerable<Grade>> GetUnpublishedGradesAsync() => _inner.GetUnpublishedGradesAsync();

    public Task<bool> BulkPublishGradesAsync(IEnumerable<Guid> gradeUids) => _inner.BulkPublishGradesAsync(gradeUids);
}
