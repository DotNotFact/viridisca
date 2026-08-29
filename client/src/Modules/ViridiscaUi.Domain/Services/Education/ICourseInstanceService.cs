using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Analytics;

namespace ViridiscaUi.Domain.Services.Education;

/// <summary>
/// Интерфейс сервиса для работы с экземплярами курсов
/// </summary>
public interface ICourseInstanceService
{
    // Базовые CRUD операции
    Task<CourseInstance?> GetByUidAsync(Guid uid);
    Task<CourseInstance?> GetCourseInstanceAsync(Guid uid); // Алиас для GetByUidAsync
    Task<IEnumerable<CourseInstance>> GetAllAsync();
    Task<CourseInstance> CreateAsync(CourseInstance courseInstance);
    Task<bool> UpdateAsync(CourseInstance courseInstance);
    Task<bool> DeleteAsync(Guid uid);
    Task<bool> DeleteCourseInstanceAsync(Guid uid); // Алиас для DeleteAsync

    // Missing methods needed by ViewModels
    /// <summary>
    /// Получает экземпляры курсов по преподавателю (алиас для GetCourseInstancesByTeacherAsync)
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetByTeacherUidAsync(Guid teacherUid);

    /// <summary>
    /// Получает неназначенные экземпляры курсов
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetUnassignedAsync();

    // Специфичные методы для экземпляров курсов
    Task<IEnumerable<CourseInstance>> GetCourseInstancesByGroupAsync(Guid groupUid);
    Task<IEnumerable<CourseInstance>> GetCourseInstancesByTeacherAsync(Guid teacherUid);
    Task<IEnumerable<CourseInstance>> GetCourseInstancesBySubjectAsync(Guid subjectUid);
    Task<IEnumerable<CourseInstance>> GetCourseInstancesByPeriodAsync(Guid academicPeriodUid);
    Task<IEnumerable<CourseInstance>> GetCourseInstancesByStudentAsync(Guid studentUid);
    Task<IEnumerable<CourseInstance>> GetCoursesByStudentAsync(Guid studentUid); // Алиас
    Task<IEnumerable<CourseInstance>> GetActiveCourseInstancesAsync();
    Task<IEnumerable<CourseInstance>> SearchCourseInstancesAsync(string searchTerm);
    Task<(IEnumerable<CourseInstance> CourseInstances, int TotalCount)> GetCourseInstancesPagedAsync(
        int page, int pageSize, string? searchTerm = null, 
        Guid? groupUid = null, Guid? teacherUid = null, Guid? subjectUid = null);

    // Управление состоянием курсов
    Task<bool> StartCourseInstanceAsync(Guid courseInstanceUid);
    Task<bool> CompleteCourseInstanceAsync(Guid courseInstanceUid);
    Task<bool> SuspendCourseInstanceAsync(Guid courseInstanceUid);

    // Управление записями студентов
    Task<IEnumerable<Student>> GetEnrolledStudentsAsync(Guid courseInstanceUid);
    Task<bool> EnrollStudentAsync(Guid courseInstanceUid, Guid studentUid);
    Task<bool> UnenrollStudentAsync(Guid courseInstanceUid, Guid studentUid);
    Task<int> GetEnrollmentCountAsync(Guid courseInstanceUid);

    // Статистика и аналитика
    /// <summary>
    /// Получает аналитику экземпляра курса
    /// </summary>
    /// <param name="courseInstanceUid">Идентификатор экземпляра курса</param>
    Task<CourseAnalytics> GetAnalyticsAsync(Guid courseInstanceUid);
    Task<(int TotalInstances, int ActiveInstances, int TotalStudents, decimal AverageGrade)> GetCourseStatisticsAsync(Guid courseInstanceUid);
    /// <summary>
    /// Получает прогресс курса
    /// </summary>
    /// <param name="courseInstanceUid">Идентификатор экземпляра курса</param>
    Task<(decimal ProgressPercentage, int CompletedLessons, int TotalLessons, int CompletedAssignments, int TotalAssignments)> GetProgressAsync(Guid courseInstanceUid);

    // Дополнительные методы
    Task<IEnumerable<CourseInstance>> GetAllCourseInstancesAsync(
        Guid? subjectFilter = null,
        Guid? teacherFilter = null,
        Guid? groupFilter = null,
        Guid? academicPeriodFilter = null);

    Task<CourseInstance?> GetCourseAsync(Guid uid); // Алиас для GetByUidAsync
    Task<IEnumerable<CourseInstance>> GetAllCoursesAsync(); // Алиас для GetAllAsync
    Task<CourseInstance?> CloneCourseInstanceAsync(Guid courseInstanceUid, Guid newAcademicPeriodUid);
    Task<CourseInstance?> CloneCourseAsync(Guid courseInstanceUid, Guid newAcademicPeriodUid); // Алиас
    Task<IEnumerable<CourseInstance>> GetAvailableCourseInstancesForStudentAsync(Guid studentUid); 
    Task<IEnumerable<CourseInstance>> GetRecommendedCourseInstancesAsync(Guid studentUid);
} 