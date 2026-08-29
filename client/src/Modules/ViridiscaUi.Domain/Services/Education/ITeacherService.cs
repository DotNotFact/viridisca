using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Entities.Education;

namespace ViridiscaUi.Domain.Services.Education;

/// <summary>
/// Интерфейс сервиса для работы с преподавателями
/// </summary>
public interface ITeacherService
{
    // Базовые CRUD операции
    Task<Teacher?> GetByUidAsync(Guid uid);
    Task<Teacher?> GetTeacherAsync(Guid uid);
    Task<IEnumerable<Teacher>> GetAllAsync();
    Task<(IEnumerable<Teacher> teachers, int totalCount)> GetPagedAsync(int page, int pageSize, string? searchQuery = null);
    Task<Teacher> CreateAsync(Teacher teacher);
    Task<bool> UpdateAsync(Teacher teacher);
    Task<bool> DeleteAsync(Guid uid);

    // Дополнительные методы для совместимости с ViewModels
    Task<Teacher?> GetByPersonEmailAsync(string email);
    Task<bool> UpdateTeacherAsync(Teacher teacher);
    Task<Teacher> CreateTeacherAsync(Teacher teacher);

    // Специфичные методы для преподавателей
    Task<IEnumerable<Teacher>> GetTeachersByDepartmentAsync(Guid departmentUid);
    Task<IEnumerable<Teacher>> GetActiveTeachersAsync();
    Task<IEnumerable<Teacher>> SearchTeachersAsync(string searchTerm);
    Task<(IEnumerable<Teacher> Teachers, int TotalCount)> GetTeachersPagedAsync(
        int page, int pageSize, string? searchTerm = null, 
        Guid? departmentUid = null);

    Task<Teacher?> GetByEmployeeCodeAsync(string employeeCode);
    Task<bool> ExistsByEmployeeCodeAsync(string employeeCode, Guid? excludeUid = null);
    Task<TeacherAnalytics> GetAnalyticsAsync(Guid teacherUid);
    Task<TeacherAnalytics> GetTeacherStatisticsAsync(Guid teacherUid);
    Task<IEnumerable<CourseInstance>> GetTeacherCourseInstancesAsync(Guid teacherUid);
    Task<IEnumerable<Group>> GetTeacherGroupsAsync(Guid teacherUid);
    Task<IEnumerable<Student>> GetTeacherStudentsAsync(Guid teacherUid);
    Task<double> GetTeacherWorkloadAsync(Guid teacherUid);

    /// <summary>
    /// Получает всех преподавателей
    /// </summary>
    Task<IEnumerable<Teacher>> GetAllTeachersAsync();

    /// <summary>
    /// Получает всех преподавателей (алиас для GetAllTeachersAsync)
    /// </summary>
    Task<IEnumerable<Teacher>> GetTeachersAsync();

    /// <summary>
    /// Добавляет нового преподавателя
    /// </summary>
    Task AddTeacherAsync(Teacher teacher);

    /// <summary>
    /// Назначает преподавателя на курс
    /// </summary>
    Task<bool> AssignToCourseAsync(Guid teacherUid, Guid courseUid);

    /// <summary>
    /// Получает группы, которые курирует преподаватель
    /// </summary>
    Task<IEnumerable<Group>> GetCuratedGroupsAsync(Guid teacherUid);

    /// <summary>
    /// Экспортирует данные преподавателей
    /// </summary>
    Task<string> ExportTeachersAsync(IEnumerable<Teacher> teachers, string format = "xlsx");

    /// <summary>
    /// Назначает преподавателя на группу
    /// </summary>
    Task<bool> AssignToGroupAsync(Guid teacherUid, Guid groupUid);

    /// <summary>
    /// Отменяет назначение преподавателя на группу
    /// </summary>
    Task<bool> UnassignFromGroupAsync(Guid teacherUid, Guid groupUid);

    /// <summary>
    /// Отменяет назначение преподавателя на курс
    /// </summary>
    Task<bool> UnassignFromCourseAsync(Guid teacherUid, Guid courseUid);

    /// <summary>
    /// Получает доступных кураторов
    /// </summary>
    Task<IEnumerable<Teacher>> GetAvailableCuratorsAsync();

    /// <summary>
    /// Получает доступных кураторов для назначения группе
    /// </summary>
    Task<IEnumerable<Teacher>> GetAvailableCuratorsForGroupAsync(Guid groupUid);

    /// <summary>
    /// Проверяет существование преподавателя по email
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email);

    /// <summary>
    /// Проверяет существование преподавателя по email (с исключением определенного UID)
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, Guid excludeUid);

    // Missing methods needed by ViewModels
    /// <summary>
    /// Удаляет преподавателя (алиас для DeleteAsync)
    /// </summary>
    Task<bool> DeleteTeacherAsync(Guid uid);
}