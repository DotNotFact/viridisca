using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// HTTP-backed IStudentService. The backend supports create/get-by-id/update
/// (emergency contact + medical info only)/assign-to-group/add-parent — no list-all,
/// search, or paging endpoint exists yet, so those fall through to <paramref name="inner"/>,
/// the EF-backed StudentService. Grades/GPA/enrollments/attendance/analytics belong to
/// modules that don't exist server-side yet and also fall through unchanged.
/// </summary>
public class HttpStudentService(StudentApiClient apiClient, StudentService inner, ILogger<HttpStudentService> logger) : IStudentService
{
    private readonly StudentApiClient _apiClient = apiClient;
    private readonly StudentService _inner = inner;
    private readonly ILogger<HttpStudentService> _logger = logger;

    public async Task<Student?> GetByUidAsync(Guid uid)
    {
        var (success, data, error) = await _apiClient.GetAsync(uid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetByUidAsync({StudentUid}) failed: {Error}", uid, error);
            return null;
        }

        return AcademicMappers.ToStudent(data);
    }

    public Task<IEnumerable<Student>> GetAllAsync() => _inner.GetAllAsync();

    public Task<IEnumerable<Student>> GetStudentsAsync() => GetAllAsync();

    public Task<IEnumerable<Student>> GetAllStudentsAsync() => GetAllAsync();

    public async Task<Student> CreateAsync(Student student)
    {
        var request = new CreateStudentRequestDto(
            student.PersonUid, student.Person?.FirstName ?? string.Empty, student.Person?.LastName ?? string.Empty,
            student.Person?.Email ?? string.Empty, student.Person?.DateOfBirth ?? default,
            student.StudentCode, student.EnrollmentDate, student.Person?.MiddleName, student.Person?.PhoneNumber, student.GroupUid);

        var (success, studentUid, error) = await _apiClient.CreateAsync(request);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to create student: {error}");
        }

        student.Uid = studentUid;
        return student;
    }

    public Task<bool> UpdateAsync(Student student)
    {
        // Backend's update endpoint only covers emergency contact + medical info, and the
        // client's own Student entity doesn't even carry those fields — there's nothing to
        // send. Falls back to EF, which does persist everything this entity actually has.
        return _inner.UpdateAsync(student);
    }

    public Task<bool> DeleteAsync(Guid uid)
    {
        _logger.LogWarning("DeleteAsync is not supported by the backend yet (no DELETE endpoint for students)");
        return Task.FromResult(false);
    }

    public Task<(IEnumerable<Student> Students, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null) => _inner.GetPagedAsync(page, pageSize, searchTerm);

    public Task<int> CountAsync() => _inner.CountAsync();

    public Task<bool> UpdateStudentAsync(Student student) => UpdateAsync(student);

    public Task<bool> DeleteStudentAsync(Guid uid) => DeleteAsync(uid);

    public Task<Student?> GetStudentAsync(Guid uid) => GetByUidAsync(uid);

    public Task<Student> CreateStudentAsync(Student student) => CreateAsync(student);

    public Task<int> GetStudentsCountByGroupAsync(Guid groupUid) => _inner.GetStudentsCountByGroupAsync(groupUid);

    public Task<IEnumerable<CourseInstance>> GetStudentCoursesAsync(Guid studentUid) => _inner.GetStudentCoursesAsync(studentUid);

    public Task<IEnumerable<Attendance>> GetStudentAttendanceAsync(Guid studentUid) => _inner.GetStudentAttendanceAsync(studentUid);

    public Task<IEnumerable<Student>> GetStudentsByGroupAsync(Guid groupUid) => _inner.GetStudentsByGroupAsync(groupUid);

    public Task<IEnumerable<Student>> GetStudentsByStatusAsync(StudentStatus status) => _inner.GetStudentsByStatusAsync(status);

    public Task<IEnumerable<Student>> GetActiveStudentsAsync() => _inner.GetActiveStudentsAsync();

    public Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm) => _inner.SearchStudentsAsync(searchTerm);

    public Task<(IEnumerable<Student> Students, int TotalCount)> GetStudentsPagedAsync(int page, int pageSize, string? searchTerm = null, Guid? groupUid = null, StudentStatus? status = null)
        => _inner.GetStudentsPagedAsync(page, pageSize, searchTerm, groupUid, status);

    public Task<Student?> GetByStudentCodeAsync(string studentCode) => _inner.GetByStudentCodeAsync(studentCode);

    public Task<bool> ExistsByStudentCodeAsync(string studentCode, Guid? excludeUid = null) => _inner.ExistsByStudentCodeAsync(studentCode, excludeUid);

    public async Task<bool> TransferStudentAsync(Guid studentUid, Guid newGroupUid)
    {
        var (success, error) = await _apiClient.AssignToGroupAsync(studentUid, newGroupUid);
        if (!success)
        {
            _logger.LogWarning("TransferStudentAsync({StudentUid}) failed: {Error}", studentUid, error);
        }

        return success;
    }

    public Task<bool> ChangeStudentStatusAsync(Guid studentUid, StudentStatus newStatus)
    {
        // No backend endpoint for a bare status change (Activate/Deactivate/Graduate exist
        // as domain methods but aren't exposed over HTTP yet) — falls back to EF.
        return _inner.ChangeStudentStatusAsync(studentUid, newStatus);
    }

    public Task<IEnumerable<Grade>> GetStudentGradesAsync(Guid studentUid) => _inner.GetStudentGradesAsync(studentUid);

    public Task<double> GetStudentGPAAsync(Guid studentUid) => _inner.GetStudentGPAAsync(studentUid);

    public Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(Guid studentUid) => _inner.GetStudentEnrollmentsAsync(studentUid);

    public Task<StudentAnalytics> GetAnalyticsAsync(Guid studentUid, Guid? academicPeriodUid = null) => _inner.GetAnalyticsAsync(studentUid, academicPeriodUid);

    public Task<StudentAnalytics> GetStudentStatisticsAsync(Guid studentUid, Guid? academicPeriodUid = null) => _inner.GetStudentStatisticsAsync(studentUid, academicPeriodUid);

    /// <summary>
    /// Not part of IStudentService — adds a parent link via the backend's
    /// AddStudentParent endpoint, which nothing in the existing interface covers.
    /// </summary>
    public async Task<(bool Success, Guid ParentLinkUid, string? Error)> AddParentAsync(Guid studentUid, Guid parentUserUid, string relation, bool isPrimaryContact = false, bool hasLegalGuardianship = false, CancellationToken cancellationToken = default)
    {
        var (success, parentLinkUid, error) = await _apiClient.AddParentAsync(studentUid, new AddParentRequestDto(parentUserUid, relation, isPrimaryContact, hasLegalGuardianship), cancellationToken);
        return (success, parentLinkUid, error);
    }
}
