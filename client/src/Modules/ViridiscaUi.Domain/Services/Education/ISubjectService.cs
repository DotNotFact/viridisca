using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education;

namespace ViridiscaUi.Domain.Services.Education;

/// <summary>
/// Интерфейс сервиса для работы с предметами
/// </summary>
public interface ISubjectService
{
    /// <summary>
    /// Получает все предметы
    /// </summary>
    Task<IEnumerable<Subject>> GetAllAsync();

    /// <summary>
    /// Получает предмет по ID
    /// </summary>
    Task<Subject?> GetByIdAsync(Guid subjectUid);

    /// <summary>
    /// Получает предмет по UID (псевдоним для GetByIdAsync)
    /// </summary>
    Task<Subject?> GetByUidAsync(Guid subjectUid);

    /// <summary>
    /// Создает новый предмет
    /// </summary>
    Task<Subject> CreateAsync(Subject subject);

    /// <summary>
    /// Обновляет предмет
    /// </summary>
    Task<Subject> UpdateAsync(Subject subject);

    /// <summary>
    /// Удаляет предмет
    /// </summary>
    Task<bool> DeleteAsync(Guid subjectUid);

    /// <summary>
    /// Получает предметы по департаменту
    /// </summary>
    Task<IEnumerable<Subject>> GetByDepartmentAsync(Guid departmentUid);

    /// <summary>
    /// Поиск предметов
    /// </summary>
    Task<IEnumerable<Subject>> SearchAsync(string searchTerm);

    /// <summary>
    /// Получает предметы с пагинацией
    /// </summary>
    Task<(IEnumerable<Subject> Subjects, int TotalCount)> GetPagedAsync(
        int page = 1, 
        int pageSize = 20, 
        string? searchTerm = null, 
        Guid? departmentUid = null);

    /// <summary>
    /// Получает предмет по коду
    /// </summary>
    Task<Subject?> GetByCodeAsync(string code);

    /// <summary>
    /// Получает предмет по названию
    /// </summary>
    Task<Subject?> GetByNameAsync(string name);

    /// <summary>
    /// Получает общее количество предметов
    /// </summary>
    Task<int> GetTotalCountAsync();

    /// <summary>
    /// Получает курсы предмета
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetSubjectCoursesAsync(Guid subjectUid);

    /// <summary>
    /// Получает учебные планы предмета
    /// </summary>
    Task<IEnumerable<Curriculum>> GetSubjectCurriculaAsync(Guid subjectUid);

    /// <summary>
    /// Получает задания предмета
    /// </summary>
    Task<IEnumerable<Assignment>> GetSubjectAssignmentsAsync(Guid subjectUid);
}