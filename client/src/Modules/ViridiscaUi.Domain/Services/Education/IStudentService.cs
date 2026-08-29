using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.Analytics;

namespace ViridiscaUi.Domain.Services.Education;

/// <summary>
/// Интерфейс сервиса для работы со студентами
/// </summary>
public interface IStudentService
{
    // Базовые CRUD операции
    Task<Student?> GetByUidAsync(Guid uid);
    Task<IEnumerable<Student>> GetAllAsync();
    Task<IEnumerable<Student>> GetStudentsAsync(); // Алиас для GetAllAsync
    Task<IEnumerable<Student>> GetAllStudentsAsync(); // Еще один алиас для GetAllAsync
    Task<Student> CreateAsync(Student student);
    Task<bool> UpdateAsync(Student student);
    Task<bool> DeleteAsync(Guid uid);

    // Дополнительные методы для совместимости с ViewModels
    Task<(IEnumerable<Student> Students, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null);
    Task<int> CountAsync();
    Task<bool> UpdateStudentAsync(Student student);
    Task<bool> DeleteStudentAsync(Guid uid);

    // Missing methods needed by ViewModels
    /// <summary>
    /// Получает студента по Uid (алиас для GetByUidAsync)
    /// </summary>
    Task<Student?> GetStudentAsync(Guid uid);

    /// <summary>
    /// Создает нового студента (алиас для CreateAsync)
    /// </summary>
    Task<Student> CreateStudentAsync(Student student);

    /// <summary>
    /// Получает количество студентов в группе
    /// </summary>
    Task<int> GetStudentsCountByGroupAsync(Guid groupUid);

    /// <summary>
    /// Получает курсы студента
    /// </summary>
    Task<IEnumerable<CourseInstance>> GetStudentCoursesAsync(Guid studentUid);

    /// <summary>
    /// Получает посещаемость студента
    /// </summary>
    Task<IEnumerable<Attendance>> GetStudentAttendanceAsync(Guid studentUid);

    // Специфичные методы для студентов
    Task<IEnumerable<Student>> GetStudentsByGroupAsync(Guid groupUid);
    Task<IEnumerable<Student>> GetStudentsByStatusAsync(StudentStatus status);
    Task<IEnumerable<Student>> GetActiveStudentsAsync();
    Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm);
    Task<(IEnumerable<Student> Students, int TotalCount)> GetStudentsPagedAsync(
        int page, int pageSize, string? searchTerm = null, 
        Guid? groupUid = null, StudentStatus? status = null);

    Task<Student?> GetByStudentCodeAsync(string studentCode);
    Task<bool> ExistsByStudentCodeAsync(string studentCode, Guid? excludeUid = null);
    Task<bool> TransferStudentAsync(Guid studentUid, Guid newGroupUid);
    Task<bool> ChangeStudentStatusAsync(Guid studentUid, StudentStatus newStatus);
    Task<IEnumerable<Grade>> GetStudentGradesAsync(Guid studentUid);
    Task<double> GetStudentGPAAsync(Guid studentUid);
    Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(Guid studentUid);

    /// <summary>
    /// Получает аналитику студента за указанный академический период
    /// </summary>
    /// <param name="studentUid">Идентификатор студента</param>
    /// <param name="academicPeriodUid">Идентификатор академического периода (если null, используется текущий)</param>
    Task<StudentAnalytics> GetAnalyticsAsync(Guid studentUid, Guid? academicPeriodUid = null);

    /// <summary>
    /// Получает аналитику студента (альтернативное имя для совместимости)
    /// </summary>
    /// <param name="studentUid">Идентификатор студента</param>
    /// <param name="academicPeriodUid">Идентификатор академического периода</param>
    Task<StudentAnalytics> GetStudentStatisticsAsync(Guid studentUid, Guid? academicPeriodUid = null);
}
