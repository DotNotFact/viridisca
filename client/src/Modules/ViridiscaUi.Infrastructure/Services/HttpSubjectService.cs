using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// HTTP-backed ISubjectService for the operations the backend Academic API supports
/// (CRUD, list, department filter — see PROGRESS.md Phase 2). Everything else — courses,
/// curricula, assignments, full-text search, paging — belongs to modules that don't exist
/// server-side yet, so those calls fall through to <paramref name="inner"/>, the original
/// EF-backed SubjectService, unchanged.
/// </summary>
public class HttpSubjectService(SubjectApiClient apiClient, SubjectService inner, ILogger<HttpSubjectService> logger) : ISubjectService
{
    private readonly SubjectApiClient _apiClient = apiClient;
    private readonly SubjectService _inner = inner;
    private readonly ILogger<HttpSubjectService> _logger = logger;

    public async Task<IEnumerable<Subject>> GetAllAsync()
    {
        var (success, data, error) = await _apiClient.GetAllAsync();
        if (!success || data is null)
        {
            _logger.LogWarning("GetAllAsync (subjects) failed: {Error}", error);
            return [];
        }

        return data.Select(AcademicMappers.ToSubject);
    }

    public async Task<Subject?> GetByIdAsync(Guid subjectUid) => await GetByUidAsync(subjectUid);

    public async Task<Subject?> GetByUidAsync(Guid subjectUid)
    {
        var (success, data, error) = await _apiClient.GetAsync(subjectUid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetByUidAsync({SubjectUid}) failed: {Error}", subjectUid, error);
            return null;
        }

        return AcademicMappers.ToSubject(data);
    }

    public async Task<Subject> CreateAsync(Subject subject)
    {
        // The client's Subject entity has no Difficulty concept at all (backend added it
        // later); "Intermediate" is a placeholder until the client model catches up.
        var request = new CreateSubjectRequestDto(
            subject.Name, subject.Code, subject.Description, subject.Credits,
            ToBackendSubjectType(subject.Type), "Intermediate", subject.DepartmentUid);

        var (success, subjectUid, error) = await _apiClient.CreateAsync(request);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to create subject: {error}");
        }

        subject.Uid = subjectUid;
        return subject;
    }

    public async Task<Subject> UpdateAsync(Subject subject)
    {
        var (success, error) = await _apiClient.UpdateAsync(subject.Uid, new UpdateSubjectRequestDto(subject.Name, subject.Description, subject.Credits));
        if (!success)
        {
            throw new InvalidOperationException($"Failed to update subject: {error}");
        }

        return subject;
    }

    public Task<bool> DeleteAsync(Guid subjectUid)
    {
        _logger.LogWarning("DeleteAsync is not supported by the backend yet (no DELETE endpoint for subjects)");
        return Task.FromResult(false);
    }

    public async Task<IEnumerable<Subject>> GetByDepartmentAsync(Guid departmentUid)
        => (await GetAllAsync()).Where(s => s.DepartmentUid == departmentUid);

    public Task<IEnumerable<Subject>> SearchAsync(string searchTerm) => _inner.SearchAsync(searchTerm);

    public Task<(IEnumerable<Subject> Subjects, int TotalCount)> GetPagedAsync(int page = 1, int pageSize = 20, string? searchTerm = null, Guid? departmentUid = null)
        => _inner.GetPagedAsync(page, pageSize, searchTerm, departmentUid);

    public async Task<Subject?> GetByCodeAsync(string code)
        => (await GetAllAsync()).FirstOrDefault(s => s.Code == code);

    public async Task<Subject?> GetByNameAsync(string name)
        => (await GetAllAsync()).FirstOrDefault(s => s.Name == name);

    public async Task<int> GetTotalCountAsync() => (await GetAllAsync()).Count();

    public Task<IEnumerable<CourseInstance>> GetSubjectCoursesAsync(Guid subjectUid) => _inner.GetSubjectCoursesAsync(subjectUid);

    public Task<IEnumerable<Curriculum>> GetSubjectCurriculaAsync(Guid subjectUid) => _inner.GetSubjectCurriculaAsync(subjectUid);

    public Task<IEnumerable<Assignment>> GetSubjectAssignmentsAsync(Guid subjectUid) => _inner.GetSubjectAssignmentsAsync(subjectUid);

    // Backend's SubjectType (Core/Elective/Laboratory/Practical/Seminar/Research/Workshop) and
    // the client's own (Required/Elective/Specialized/Practicum/Seminar/Laboratory/Lecture) only
    // partially overlap by name (see AcademicMappers.ParseSubjectType for the read direction).
    // CreateAsync was previously sending subject.Type.ToString() verbatim, which the backend's
    // JsonStringEnumConverter can't parse for the 4 non-overlapping client values (Required/
    // Specialized/Practicum/Lecture) — every subject created through the real UI with one of
    // those types silently failed with a 500. Fixed with an explicit reverse mapping.
    private static string ToBackendSubjectType(Domain.Entities.Education.Enums.SubjectType clientType) => clientType switch
    {
        Domain.Entities.Education.Enums.SubjectType.Elective => "Elective",
        Domain.Entities.Education.Enums.SubjectType.Seminar => "Seminar",
        Domain.Entities.Education.Enums.SubjectType.Laboratory => "Laboratory",
        Domain.Entities.Education.Enums.SubjectType.Practicum => "Practical",
        Domain.Entities.Education.Enums.SubjectType.Specialized => "Research",
        Domain.Entities.Education.Enums.SubjectType.Lecture => "Workshop",
        _ => "Core",
    };
}
