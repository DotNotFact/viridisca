using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Analytics;

namespace ViridiscaUi.Domain.Services.Education;

/// <summary>
/// Интерфейс сервиса для работы с учебными группами
/// </summary>
public interface IGroupService
{
    // Стандартные CRUD операции (для совместимости с ViewModels)
    Task<IEnumerable<Group>> GetAllAsync();
    Task<Group?> GetByUidAsync(Guid uid);
    Task<Group> CreateAsync(Group group);
    Task<bool> UpdateAsync(Group group);
    Task<bool> DeleteAsync(Guid uid);

    // Базовые CRUD операции (существующие методы)
    Task<Group?> GetGroupAsync(Guid uid);
    Task<IReadOnlyList<Group>> GetAllGroupsAsync();
    Task<IReadOnlyList<Group>> GetGroupsAsync();
    Task<Group> CreateGroupAsync(Group group);
    Task AddGroupAsync(Group group);
    Task<bool> UpdateGroupAsync(Group group);
    Task<bool> DeleteGroupAsync(Guid uid);

    // Missing methods needed by ViewModels
    /// <summary>
    /// Получает группы с пагинацией
    /// </summary>
    Task<(IEnumerable<Group> Groups, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null, Guid? departmentUid = null);

    /// <summary>
    /// Получает записи на курсы группы
    /// </summary>
    Task<IEnumerable<Enrollment>> GetGroupEnrollmentsAsync(Guid groupUid);

    /// <summary>
    /// Получает задания группы
    /// </summary>
    Task<IEnumerable<Assignment>> GetGroupAssignmentsAsync(Guid groupUid);

    /// <summary>
    /// Получает оценки группы
    /// </summary>
    Task<IEnumerable<Grade>> GetGroupGradesAsync(Guid groupUid);

    /// <summary>
    /// Получает посещаемость группы
    /// </summary>
    Task<IEnumerable<Attendance>> GetGroupAttendanceAsync(Guid groupUid);

    // Специфичные методы для групп
    Task<IReadOnlyList<Group>> GetGroupsByCourseAsync(int course);
    Task<IReadOnlyList<Group>> GetGroupsByYearAsync(int year);
    Task<IReadOnlyList<Group>> GetGroupsByCuratorAsync(Guid curatorUid);
    Task<IReadOnlyList<Group>> GetActiveGroupsAsync();
    Task<IReadOnlyList<Group>> GetGroupsByDepartmentAsync(Guid departmentUid);
    Task<IReadOnlyList<Group>> SearchGroupsAsync(string searchTerm);
    Task<(IReadOnlyList<Group> Groups, int TotalCount)> GetGroupsPagedAsync(int page, int pageSize, string? searchTerm = null, Guid? departmentUid = null);
    
    /// <summary>
    /// Получает аналитику группы
    /// </summary>
    /// <param name="groupUid">Идентификатор группы</param>
    Task<GroupAnalytics> GetAnalyticsAsync(Guid groupUid);

    /// <summary>
    /// Получает аналитику группы (альтернативное имя для совместимости)
    /// </summary>
    /// <param name="groupUid">Идентификатор группы</param>
    Task<GroupAnalytics> GetGroupStatisticsAsync(Guid groupUid);

    Task<bool> AssignCuratorAsync(Guid groupUid, Guid? curatorUid);
    Task<bool> RemoveCuratorAsync(Guid groupUid);
    Task<bool> PromoteToNextCourseAsync(Guid groupUid);
    Task<Group?> GetByCodeAsync(string code);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeUid = null);
    Task<int> GetStudentsCountByGroupAsync(Guid groupUid);
    Task<int> GetCourseInstancesCountAsync(Guid groupUid);
    Task<IReadOnlyList<CourseInstance>> GetGroupCourseInstancesAsync(Guid groupUid);
}
