using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// HTTP-backed IGroupService — CRUD, listing, and curator assignment go through the
/// backend Academic API; everything involving Enrollments/Assignments/Grades/Attendance/
/// analytics/paging falls through to <paramref name="inner"/>, the EF-backed GroupService,
/// since those modules don't exist server-side yet.
/// </summary>
public class HttpGroupService(GroupApiClient apiClient, GroupService inner, ILogger<HttpGroupService> logger) : IGroupService
{
    private readonly GroupApiClient _apiClient = apiClient;
    private readonly GroupService _inner = inner;
    private readonly ILogger<HttpGroupService> _logger = logger;

    public async Task<IEnumerable<Group>> GetAllAsync()
    {
        var (success, data, error) = await _apiClient.GetAllAsync();
        if (!success || data is null)
        {
            _logger.LogWarning("GetAllAsync (groups) failed: {Error}", error);
            return [];
        }

        return data.Select(AcademicMappers.ToGroup);
    }

    public async Task<Group?> GetByUidAsync(Guid uid)
    {
        var (success, data, error) = await _apiClient.GetAsync(uid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetByUidAsync({GroupUid}) failed: {Error}", uid, error);
            return null;
        }

        return AcademicMappers.ToGroup(data);
    }

    public async Task<Group> CreateAsync(Group group)
    {
        var request = new CreateGroupRequestDto(group.Code, group.Name, group.Description, group.Year, group.StartDate, group.MaxStudents, group.DepartmentUid, group.CuratorUid);
        var (success, groupUid, error) = await _apiClient.CreateAsync(request);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to create group: {error}");
        }

        group.Uid = groupUid;
        return group;
    }

    public async Task<bool> UpdateAsync(Group group)
    {
        var (success, error) = await _apiClient.UpdateAsync(group.Uid, new UpdateGroupRequestDto(group.Name, group.Description, group.MaxStudents));
        if (!success)
        {
            _logger.LogWarning("UpdateAsync({GroupUid}) failed: {Error}", group.Uid, error);
        }

        return success;
    }

    public Task<bool> DeleteAsync(Guid uid)
    {
        _logger.LogWarning("DeleteAsync is not supported by the backend yet (no DELETE endpoint for groups)");
        return Task.FromResult(false);
    }

    public Task<Group?> GetGroupAsync(Guid uid) => GetByUidAsync(uid);

    public async Task<IReadOnlyList<Group>> GetAllGroupsAsync() => (await GetAllAsync()).ToList();

    public Task<IReadOnlyList<Group>> GetGroupsAsync() => GetAllGroupsAsync();

    public Task<Group> CreateGroupAsync(Group group) => CreateAsync(group);

    public async Task AddGroupAsync(Group group) => await CreateAsync(group);

    public Task<bool> UpdateGroupAsync(Group group) => UpdateAsync(group);

    public Task<bool> DeleteGroupAsync(Guid uid) => DeleteAsync(uid);

    public async Task<(IEnumerable<Group> Groups, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null, Guid? departmentUid = null)
    {
        var all = await GetAllAsync();
        var filtered = all.Where(g =>
            (departmentUid is null || g.DepartmentUid == departmentUid) &&
            (string.IsNullOrWhiteSpace(searchTerm) || g.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || g.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return (filtered.Skip((page - 1) * pageSize).Take(pageSize), filtered.Count);
    }

    public Task<IEnumerable<Enrollment>> GetGroupEnrollmentsAsync(Guid groupUid) => _inner.GetGroupEnrollmentsAsync(groupUid);

    public Task<IEnumerable<Assignment>> GetGroupAssignmentsAsync(Guid groupUid) => _inner.GetGroupAssignmentsAsync(groupUid);

    public Task<IEnumerable<Grade>> GetGroupGradesAsync(Guid groupUid) => _inner.GetGroupGradesAsync(groupUid);

    public Task<IEnumerable<Attendance>> GetGroupAttendanceAsync(Guid groupUid) => _inner.GetGroupAttendanceAsync(groupUid);

    public async Task<IReadOnlyList<Group>> GetGroupsByCourseAsync(int course) => (await GetAllAsync()).Where(g => g.Year == course).ToList();

    public async Task<IReadOnlyList<Group>> GetGroupsByYearAsync(int year) => (await GetAllAsync()).Where(g => g.Year == year).ToList();

    public async Task<IReadOnlyList<Group>> GetGroupsByCuratorAsync(Guid curatorUid) => (await GetAllAsync()).Where(g => g.CuratorUid == curatorUid).ToList();

    public async Task<IReadOnlyList<Group>> GetActiveGroupsAsync() => (await GetAllAsync()).Where(g => g.IsActive).ToList();

    public async Task<IReadOnlyList<Group>> GetGroupsByDepartmentAsync(Guid departmentUid) => (await GetAllAsync()).Where(g => g.DepartmentUid == departmentUid).ToList();

    public async Task<IReadOnlyList<Group>> SearchGroupsAsync(string searchTerm)
        => (await GetAllAsync()).Where(g => g.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || g.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();

    public async Task<(IReadOnlyList<Group> Groups, int TotalCount)> GetGroupsPagedAsync(int page, int pageSize, string? searchTerm = null, Guid? departmentUid = null)
    {
        var (groups, totalCount) = await GetPagedAsync(page, pageSize, searchTerm, departmentUid);
        return (groups.ToList(), totalCount);
    }

    public Task<GroupAnalytics> GetAnalyticsAsync(Guid groupUid) => _inner.GetAnalyticsAsync(groupUid);

    public Task<GroupAnalytics> GetGroupStatisticsAsync(Guid groupUid) => _inner.GetGroupStatisticsAsync(groupUid);

    public async Task<bool> AssignCuratorAsync(Guid groupUid, Guid? curatorUid)
    {
        var (success, error) = await _apiClient.SetCuratorAsync(groupUid, curatorUid);
        if (!success)
        {
            _logger.LogWarning("AssignCuratorAsync({GroupUid}) failed: {Error}", groupUid, error);
        }

        return success;
    }

    public Task<bool> RemoveCuratorAsync(Guid groupUid) => AssignCuratorAsync(groupUid, null);

    public Task<bool> PromoteToNextCourseAsync(Guid groupUid)
    {
        _logger.LogWarning("PromoteToNextCourseAsync is not supported by the backend yet");
        return Task.FromResult(false);
    }

    public async Task<Group?> GetByCodeAsync(string code) => (await GetAllAsync()).FirstOrDefault(g => g.Code == code);

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeUid = null)
        => (await GetAllAsync()).Any(g => g.Name == name && g.Uid != excludeUid);

    public async Task<int> GetStudentsCountByGroupAsync(Guid groupUid)
    {
        var (success, data, _) = await _apiClient.GetAsync(groupUid);
        return success && data is not null ? data.CurrentStudentsCount : 0;
    }

    public Task<int> GetCourseInstancesCountAsync(Guid groupUid) => _inner.GetCourseInstancesCountAsync(groupUid);

    public Task<IReadOnlyList<CourseInstance>> GetGroupCourseInstancesAsync(Guid groupUid) => _inner.GetGroupCourseInstancesAsync(groupUid);
}
