using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// HTTP-backed ICourseInstanceService for Create/GetByUid/GetAll/Update/GetByGroup/
/// GetByTeacher — the operations the backend Curriculum API supports. Enrollment
/// management, state-machine transitions (Start/Complete/Suspend), analytics/progress,
/// cloning, search/paging, and student-recommendation methods all fall through to
/// <paramref name="inner"/>, the EF-backed CourseInstanceService — those depend on
/// concepts (Enrollment, per-instance analytics) that don't exist server-side yet.
/// </summary>
public class HttpCourseInstanceService(CourseInstanceApiClient apiClient, CourseInstanceService inner, ILogger<HttpCourseInstanceService> logger) : ICourseInstanceService
{
    private readonly CourseInstanceApiClient _apiClient = apiClient;
    private readonly CourseInstanceService _inner = inner;
    private readonly ILogger<HttpCourseInstanceService> _logger = logger;

    public async Task<CourseInstance?> GetByUidAsync(Guid uid)
    {
        var (success, data, error) = await _apiClient.GetAsync(uid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetByUidAsync({CourseInstanceUid}) failed: {Error}", uid, error);
            return null;
        }

        return CurriculumMappers.ToCourseInstance(data);
    }

    public Task<CourseInstance?> GetCourseInstanceAsync(Guid uid) => GetByUidAsync(uid);

    public Task<CourseInstance?> GetCourseAsync(Guid uid) => GetByUidAsync(uid);

    public async Task<IEnumerable<CourseInstance>> GetAllAsync()
    {
        var (success, data, error) = await _apiClient.GetAllAsync();
        if (!success || data is null)
        {
            _logger.LogWarning("GetAllAsync (course instances) failed: {Error}", error);
            return [];
        }

        return data.Select(CurriculumMappers.ToCourseInstance);
    }

    public Task<IEnumerable<CourseInstance>> GetAllCoursesAsync() => GetAllAsync();

    public async Task<CourseInstance> CreateAsync(CourseInstance courseInstance)
    {
        var request = new CreateCourseInstanceRequestDto(
            courseInstance.SubjectUid, courseInstance.GroupUid, courseInstance.AcademicPeriodUid,
            courseInstance.Name, courseInstance.Code, courseInstance.StartDate,
            courseInstance.TeacherUid, courseInstance.Description, courseInstance.MaxEnrollments);

        var (success, courseInstanceUid, error) = await _apiClient.CreateAsync(request);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to create course instance: {error}");
        }

        courseInstance.Uid = courseInstanceUid;
        return courseInstance;
    }

    public async Task<bool> UpdateAsync(CourseInstance courseInstance)
    {
        var (success, error) = await _apiClient.UpdateAsync(courseInstance.Uid, new UpdateCourseInstanceRequestDto(courseInstance.Name, courseInstance.Description, courseInstance.MaxEnrollments));
        if (!success)
        {
            _logger.LogWarning("UpdateAsync({CourseInstanceUid}) failed: {Error}", courseInstance.Uid, error);
            return false;
        }

        if (courseInstance.TeacherUid.HasValue)
        {
            var (teacherSuccess, teacherError) = await _apiClient.AssignTeacherAsync(courseInstance.Uid, courseInstance.TeacherUid);
            if (!teacherSuccess)
            {
                _logger.LogWarning("AssignTeacherAsync({CourseInstanceUid}) failed: {Error}", courseInstance.Uid, teacherError);
            }
        }

        return true;
    }

    public Task<bool> DeleteAsync(Guid uid)
    {
        _logger.LogWarning("DeleteAsync is not supported by the backend yet (no DELETE endpoint for course instances)");
        return _inner.DeleteAsync(uid);
    }

    public Task<bool> DeleteCourseInstanceAsync(Guid uid) => DeleteAsync(uid);

    public async Task<IEnumerable<CourseInstance>> GetByTeacherUidAsync(Guid teacherUid)
    {
        var (success, data, error) = await _apiClient.GetByTeacherAsync(teacherUid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetByTeacherUidAsync({TeacherUid}) failed: {Error}", teacherUid, error);
            return [];
        }

        return data.Select(CurriculumMappers.ToCourseInstance);
    }

    public Task<IEnumerable<CourseInstance>> GetUnassignedAsync() => _inner.GetUnassignedAsync();

    public async Task<IEnumerable<CourseInstance>> GetCourseInstancesByGroupAsync(Guid groupUid)
    {
        var (success, data, error) = await _apiClient.GetByGroupAsync(groupUid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetCourseInstancesByGroupAsync({GroupUid}) failed: {Error}", groupUid, error);
            return [];
        }

        return data.Select(CurriculumMappers.ToCourseInstance);
    }

    public Task<IEnumerable<CourseInstance>> GetCourseInstancesByTeacherAsync(Guid teacherUid) => GetByTeacherUidAsync(teacherUid);

    public Task<IEnumerable<CourseInstance>> GetCourseInstancesBySubjectAsync(Guid subjectUid) => _inner.GetCourseInstancesBySubjectAsync(subjectUid);

    public Task<IEnumerable<CourseInstance>> GetCourseInstancesByPeriodAsync(Guid academicPeriodUid) => _inner.GetCourseInstancesByPeriodAsync(academicPeriodUid);

    public Task<IEnumerable<CourseInstance>> GetCourseInstancesByStudentAsync(Guid studentUid) => _inner.GetCourseInstancesByStudentAsync(studentUid);

    public Task<IEnumerable<CourseInstance>> GetCoursesByStudentAsync(Guid studentUid) => _inner.GetCoursesByStudentAsync(studentUid);

    public async Task<IEnumerable<CourseInstance>> GetActiveCourseInstancesAsync()
        => (await GetAllAsync()).Where(c => c.IsActive);

    public Task<IEnumerable<CourseInstance>> SearchCourseInstancesAsync(string searchTerm) => _inner.SearchCourseInstancesAsync(searchTerm);

    public Task<(IEnumerable<CourseInstance> CourseInstances, int TotalCount)> GetCourseInstancesPagedAsync(int page, int pageSize, string? searchTerm = null, Guid? groupUid = null, Guid? teacherUid = null, Guid? subjectUid = null)
        => _inner.GetCourseInstancesPagedAsync(page, pageSize, searchTerm, groupUid, teacherUid, subjectUid);

    public Task<bool> StartCourseInstanceAsync(Guid courseInstanceUid) => _inner.StartCourseInstanceAsync(courseInstanceUid);

    public Task<bool> CompleteCourseInstanceAsync(Guid courseInstanceUid) => _inner.CompleteCourseInstanceAsync(courseInstanceUid);

    public Task<bool> SuspendCourseInstanceAsync(Guid courseInstanceUid) => _inner.SuspendCourseInstanceAsync(courseInstanceUid);

    public Task<IEnumerable<Student>> GetEnrolledStudentsAsync(Guid courseInstanceUid) => _inner.GetEnrolledStudentsAsync(courseInstanceUid);

    public Task<bool> EnrollStudentAsync(Guid courseInstanceUid, Guid studentUid) => _inner.EnrollStudentAsync(courseInstanceUid, studentUid);

    public Task<bool> UnenrollStudentAsync(Guid courseInstanceUid, Guid studentUid) => _inner.UnenrollStudentAsync(courseInstanceUid, studentUid);

    public Task<int> GetEnrollmentCountAsync(Guid courseInstanceUid) => _inner.GetEnrollmentCountAsync(courseInstanceUid);

    public Task<CourseAnalytics> GetAnalyticsAsync(Guid courseInstanceUid) => _inner.GetAnalyticsAsync(courseInstanceUid);

    public Task<(int TotalInstances, int ActiveInstances, int TotalStudents, decimal AverageGrade)> GetCourseStatisticsAsync(Guid courseInstanceUid) => _inner.GetCourseStatisticsAsync(courseInstanceUid);

    public Task<(decimal ProgressPercentage, int CompletedLessons, int TotalLessons, int CompletedAssignments, int TotalAssignments)> GetProgressAsync(Guid courseInstanceUid) => _inner.GetProgressAsync(courseInstanceUid);

    public async Task<IEnumerable<CourseInstance>> GetAllCourseInstancesAsync(Guid? subjectFilter = null, Guid? teacherFilter = null, Guid? groupFilter = null, Guid? academicPeriodFilter = null)
    {
        var all = await GetAllAsync();
        return all.Where(c =>
            (subjectFilter is null || c.SubjectUid == subjectFilter) &&
            (teacherFilter is null || c.TeacherUid == teacherFilter) &&
            (groupFilter is null || c.GroupUid == groupFilter) &&
            (academicPeriodFilter is null || c.AcademicPeriodUid == academicPeriodFilter));
    }

    public Task<CourseInstance?> CloneCourseInstanceAsync(Guid courseInstanceUid, Guid newAcademicPeriodUid) => _inner.CloneCourseInstanceAsync(courseInstanceUid, newAcademicPeriodUid);

    public Task<CourseInstance?> CloneCourseAsync(Guid courseInstanceUid, Guid newAcademicPeriodUid) => CloneCourseInstanceAsync(courseInstanceUid, newAcademicPeriodUid);

    public Task<IEnumerable<CourseInstance>> GetAvailableCourseInstancesForStudentAsync(Guid studentUid) => _inner.GetAvailableCourseInstancesForStudentAsync(studentUid);

    public Task<IEnumerable<CourseInstance>> GetRecommendedCourseInstancesAsync(Guid studentUid) => _inner.GetRecommendedCourseInstancesAsync(studentUid);
}
