using ViridiscaUi.Domain.Entities.Education;

namespace ViridiscaUi.Domain.Services.Education;

/// <summary>
/// Интерфейс сервиса для работы с базовыми курсами (шаблонами)
/// </summary>
public interface ICourseService
{
    // Базовые CRUD операции
    Task<Course?> GetByUidAsync(Guid uid);
    Task<IEnumerable<Course>> GetAllAsync();
    Task<IEnumerable<Course>> GetCoursesAsync(); // Алиас для GetAllAsync
    Task<Course> CreateAsync(Course course);
    Task<bool> UpdateAsync(Course course);
    Task<bool> DeleteAsync(Guid uid);

    // Специфичные методы для курсов
    Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(Guid departmentUid);
    Task<IEnumerable<Course>> GetCoursesBySubjectAsync(Guid subjectUid);
    Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm);
    Task<(IEnumerable<Course> Courses, int TotalCount)> GetCoursesPagedAsync(
        int page, int pageSize, string? searchTerm = null, Guid? departmentUid = null);

    /// <summary>
    /// Получает статистику курса (используйте CourseAnalytics для детальной аналитики экземпляров курса)
    /// </summary>
    Task<(int TotalInstances, int ActiveInstances, int TotalEnrollments, int CompletedEnrollments, double CompletionRate, double AverageGrade)> GetCourseStatisticsAsync(Guid courseUid);
} 