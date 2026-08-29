using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Models;

namespace ViridiscaUi.Domain.Services.Education;

/// <summary>
/// Интерфейс сервиса для работы с заданиями
/// </summary>
public interface IAssignmentService
{
    /// <summary>
    /// Получает все задания
    /// </summary>
    Task<IEnumerable<Assignment>> GetAllAsync();

    /// <summary>
    /// Получает задание по ID
    /// </summary>
    /// <param name="assignmentUid">Идентификатор задания</param>
    Task<Assignment?> GetByIdAsync(Guid assignmentUid);

    /// <summary>
    /// Создает новое задание
    /// </summary>
    Task<Assignment> CreateAsync(Assignment assignment);

    /// <summary>
    /// Обновляет задание
    /// </summary>
    /// <param name="assignment">Задание для обновления</param>
    Task<Assignment> UpdateAsync(Assignment assignment);

    /// <summary>
    /// Удаляет задание
    /// </summary>
    Task<bool> DeleteAsync(Guid assignmentUid);

    /// <summary>
    /// Получает задания для курса
    /// </summary>
    /// <param name="courseInstanceUid">Идентификатор экземпляра курса</param>
    Task<IEnumerable<Assignment>> GetByCourseInstanceAsync(Guid courseInstanceUid);

    /// <summary>
    /// Получает задания для студента
    /// </summary>
    /// <param name="studentUid">Идентификатор студента</param>
    Task<IEnumerable<Assignment>> GetByStudentAsync(Guid studentUid);

    /// <summary>
    /// Получает просроченные задания
    /// </summary>
    Task<IEnumerable<Assignment>> GetOverdueAsync();

    /// <summary>
    /// Получает задания с приближающимися дедлайнами
    /// </summary>
    /// <param name="days">Количество дней</param>
    Task<IEnumerable<Assignment>> GetUpcomingDeadlinesAsync(int days);

    /// <summary>
    /// Валидирует задание
    /// </summary>
    /// <param name="assignment">Задание для валидации</param>
    Task<ValidationResult> ValidateAsync(Assignment assignment);

    // Специфичные методы для заданий
    Task<IEnumerable<Assignment>> GetAssignmentsByCourseInstanceAsync(Guid courseInstanceUid);
    Task<IEnumerable<Assignment>> GetAssignmentsByStatusAsync(AssignmentStatus status);
    Task<IEnumerable<Assignment>> GetAssignmentsByTypeAsync(AssignmentType type);
    Task<IEnumerable<Assignment>> GetAssignmentsByDifficultyAsync(AssignmentDifficulty difficulty);
    Task<IEnumerable<Assignment>> GetUpcomingAssignmentsAsync(DateTime? beforeDate = null);
    Task<IEnumerable<Assignment>> GetOverdueAssignmentsAsync();
    Task<IEnumerable<Assignment>> SearchAssignmentsAsync(string searchTerm);
    Task<(IEnumerable<Assignment> Assignments, int TotalCount)> GetAssignmentsPagedAsync(
        int page, int pageSize, string? searchTerm = null, 
        Guid? courseInstanceUid = null, AssignmentStatus? status = null);

    /// <summary>
    /// Получает задания с пагинацией (алиас для GetAssignmentsPagedAsync)
    /// </summary>
    Task<(IEnumerable<Assignment> Assignments, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? searchTerm = null, 
        Guid? courseInstanceUid = null, AssignmentStatus? status = null);

    /// <summary>
    /// Получает аналитику задания
    /// </summary>
    /// <param name="assignmentUid">Идентификатор задания</param>
    Task<AssignmentAnalytics> GetAnalyticsAsync(Guid assignmentUid);

    // Missing methods needed by ViewModels
    /// <summary>
    /// Получает задание по названию и курсу
    /// </summary>
    Task<Assignment?> GetByTitleAndCourseAsync(string title, Guid courseInstanceUid);

    /// <summary>
    /// Получает количество оценок для задания
    /// </summary>
    Task<int> GetGradesCountAsync(Guid assignmentUid);

    /// <summary>
    /// Получает аналитику задания (альтернативное имя для совместимости)
    /// </summary>
    /// <param name="assignmentUid">Идентификатор задания</param>
    Task<AssignmentAnalytics> GetAssignmentAnalyticsAsync(Guid assignmentUid);

    /// <summary>
    /// Получает сдачи по заданию
    /// </summary>
    Task<IEnumerable<Submission>> GetSubmissionsByAssignmentAsync(Guid assignmentUid);

    /// <summary>
    /// Отправляет напоминание о дедлайне
    /// </summary>
    Task SendDueDateReminderAsync(Guid assignmentUid);

    /// <summary>
    /// Получает задания ожидающие оценки
    /// </summary>
    Task<IEnumerable<Assignment>> GetAssignmentsPendingGradingAsync();

    Task<bool> PublishAssignmentAsync(Guid assignmentUid);
    Task<bool> UnpublishAssignmentAsync(Guid assignmentUid);
    Task<bool> ArchiveAssignmentAsync(Guid assignmentUid);
    Task<bool> DuplicateAssignmentAsync(Guid assignmentUid, string newTitle);
    Task<int> GetSubmissionsCountAsync(Guid assignmentUid);
    Task<IEnumerable<Submission>> GetSubmissionsAsync(Guid assignmentUid);
    Task<double> GetAverageGradeAsync(Guid assignmentUid);
    Task<bool> SetMaxPointsAsync(Guid assignmentUid, int maxPoints);
    Task<bool> UpdateDueDateAsync(Guid assignmentUid, DateTime? dueDate);
    Task<IEnumerable<Assignment>> GetAssignmentsByTeacherAsync(Guid teacherUid);
    Task<IEnumerable<Assignment>> GetAssignmentsByStudentAsync(Guid studentUid);
    Task<bool> CanStudentSubmitAsync(Guid assignmentUid, Guid studentUid);
    Task<Assignment?> GetAssignmentWithSubmissionsAsync(Guid assignmentUid);
    Task<bool> IsAssignmentOverdueAsync(Guid assignmentUid);
    Task<TimeSpan?> GetTimeUntilDueAsync(Guid assignmentUid);
}
