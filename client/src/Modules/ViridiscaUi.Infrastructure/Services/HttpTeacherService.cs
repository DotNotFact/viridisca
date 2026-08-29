using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// HTTP-backed ITeacherService. The backend only has single-entity operations for
/// teachers (create, get-by-id, assign-subject — no list-all endpoint), so every
/// "get all / search / paged" method here falls through to <paramref name="inner"/>,
/// the EF-backed TeacherService. Course/grade/workload/export methods are entirely
/// out of the backend's current scope and also fall through unchanged.
/// </summary>
public class HttpTeacherService(TeacherApiClient apiClient, TeacherService inner, ILogger<HttpTeacherService> logger) : ITeacherService
{
    private readonly TeacherApiClient _apiClient = apiClient;
    private readonly TeacherService _inner = inner;
    private readonly ILogger<HttpTeacherService> _logger = logger;

    public async Task<Teacher?> GetByUidAsync(Guid uid)
    {
        var (success, data, error) = await _apiClient.GetAsync(uid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetByUidAsync({TeacherUid}) failed: {Error}", uid, error);
            return null;
        }

        return AcademicMappers.ToTeacher(data);
    }

    public Task<Teacher?> GetTeacherAsync(Guid uid) => GetByUidAsync(uid);

    public Task<IEnumerable<Teacher>> GetAllAsync() => _inner.GetAllAsync();

    public Task<(IEnumerable<Teacher> teachers, int totalCount)> GetPagedAsync(int page, int pageSize, string? searchQuery = null) => _inner.GetPagedAsync(page, pageSize, searchQuery);

    public async Task<Teacher> CreateAsync(Teacher teacher)
    {
        var request = new CreateTeacherRequestDto(
            teacher.PersonUid, teacher.EmployeeCode, teacher.HireDate,
            teacher.Specialization, teacher.Qualification, 0, teacher.DepartmentUid);

        var (success, teacherUid, error) = await _apiClient.CreateAsync(request);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to create teacher: {error}");
        }

        teacher.Uid = teacherUid;
        return teacher;
    }

    public Task<bool> UpdateAsync(Teacher teacher)
    {
        // Backend has no update-teacher endpoint yet (only create/get/assign-subject) —
        // falls back to direct EF update rather than silently discarding the change.
        return _inner.UpdateAsync(teacher);
    }

    public Task<bool> DeleteAsync(Guid uid)
    {
        _logger.LogWarning("DeleteAsync is not supported by the backend yet (no DELETE endpoint for teachers)");
        return Task.FromResult(false);
    }

    public Task<Teacher?> GetByPersonEmailAsync(string email) => _inner.GetByPersonEmailAsync(email);

    public Task<bool> UpdateTeacherAsync(Teacher teacher) => UpdateAsync(teacher);

    public Task<Teacher> CreateTeacherAsync(Teacher teacher) => CreateAsync(teacher);

    public Task<IEnumerable<Teacher>> GetTeachersByDepartmentAsync(Guid departmentUid) => _inner.GetTeachersByDepartmentAsync(departmentUid);

    public Task<IEnumerable<Teacher>> GetActiveTeachersAsync() => _inner.GetActiveTeachersAsync();

    public Task<IEnumerable<Teacher>> SearchTeachersAsync(string searchTerm) => _inner.SearchTeachersAsync(searchTerm);

    public Task<(IEnumerable<Teacher> Teachers, int TotalCount)> GetTeachersPagedAsync(int page, int pageSize, string? searchTerm = null, Guid? departmentUid = null)
        => _inner.GetTeachersPagedAsync(page, pageSize, searchTerm, departmentUid);

    public Task<Teacher?> GetByEmployeeCodeAsync(string employeeCode) => _inner.GetByEmployeeCodeAsync(employeeCode);

    public Task<bool> ExistsByEmployeeCodeAsync(string employeeCode, Guid? excludeUid = null) => _inner.ExistsByEmployeeCodeAsync(employeeCode, excludeUid);

    public Task<TeacherAnalytics> GetAnalyticsAsync(Guid teacherUid) => _inner.GetAnalyticsAsync(teacherUid);

    public Task<TeacherAnalytics> GetTeacherStatisticsAsync(Guid teacherUid) => _inner.GetTeacherStatisticsAsync(teacherUid);

    public Task<IEnumerable<CourseInstance>> GetTeacherCourseInstancesAsync(Guid teacherUid) => _inner.GetTeacherCourseInstancesAsync(teacherUid);

    public Task<IEnumerable<Group>> GetTeacherGroupsAsync(Guid teacherUid) => _inner.GetTeacherGroupsAsync(teacherUid);

    public Task<IEnumerable<Student>> GetTeacherStudentsAsync(Guid teacherUid) => _inner.GetTeacherStudentsAsync(teacherUid);

    public Task<double> GetTeacherWorkloadAsync(Guid teacherUid) => _inner.GetTeacherWorkloadAsync(teacherUid);

    public Task<IEnumerable<Teacher>> GetAllTeachersAsync() => GetAllAsync();

    public Task<IEnumerable<Teacher>> GetTeachersAsync() => GetAllAsync();

    public async Task AddTeacherAsync(Teacher teacher) => await CreateAsync(teacher);

    public Task<bool> AssignToCourseAsync(Guid teacherUid, Guid courseUid) => _inner.AssignToCourseAsync(teacherUid, courseUid);

    public Task<IEnumerable<Group>> GetCuratedGroupsAsync(Guid teacherUid) => _inner.GetCuratedGroupsAsync(teacherUid);

    public Task<string> ExportTeachersAsync(IEnumerable<Teacher> teachers, string format = "xlsx") => _inner.ExportTeachersAsync(teachers, format);

    public async Task<bool> AssignToGroupAsync(Guid teacherUid, Guid groupUid)
    {
        // Backend's teacher<->group link (TeacherGroup) requires a subject too, which this
        // interface's signature doesn't carry — falls back to EF until that's reconciled.
        return await _inner.AssignToGroupAsync(teacherUid, groupUid);
    }

    public Task<bool> UnassignFromGroupAsync(Guid teacherUid, Guid groupUid) => _inner.UnassignFromGroupAsync(teacherUid, groupUid);

    public Task<bool> UnassignFromCourseAsync(Guid teacherUid, Guid courseUid) => _inner.UnassignFromCourseAsync(teacherUid, courseUid);

    public Task<IEnumerable<Teacher>> GetAvailableCuratorsAsync() => _inner.GetAvailableCuratorsAsync();

    public Task<IEnumerable<Teacher>> GetAvailableCuratorsForGroupAsync(Guid groupUid) => _inner.GetAvailableCuratorsForGroupAsync(groupUid);

    public Task<bool> ExistsByEmailAsync(string email) => _inner.ExistsByEmailAsync(email);

    public Task<bool> ExistsByEmailAsync(string email, Guid excludeUid) => _inner.ExistsByEmailAsync(email, excludeUid);

    public Task<bool> DeleteTeacherAsync(Guid uid) => DeleteAsync(uid);

    /// <summary>
    /// Not part of ITeacherService — used by HttpStudentService-style callers that need the
    /// real backend-side subject assignment (with IsMainTeacher), which the interface above
    /// has no matching method for.
    /// </summary>
    public async Task<(bool Success, string? Error)> AssignSubjectAsync(Guid teacherUid, Guid subjectUid, bool isMainTeacher, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await _apiClient.AssignSubjectAsync(teacherUid, new AssignSubjectRequestDto(subjectUid, isMainTeacher), cancellationToken);
        return (success, error);
    }
}
