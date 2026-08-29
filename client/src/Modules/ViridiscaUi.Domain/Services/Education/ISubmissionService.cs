using ViridiscaUi.Domain.Entities.Education;

namespace ViridiscaUi.Domain.Services.Education;

/// <summary>
/// Сервис для работы с выполненными заданиями
/// </summary>
public interface ISubmissionService
{
    /// <summary>
    /// Получает выполненное задание по идентификатору
    /// </summary>
    Task<Submission?> GetSubmissionAsync(Guid uid);
    
    /// <summary>
    /// Получает все выполненные задания
    /// </summary>
    Task<IEnumerable<Submission>> GetAllSubmissionsAsync();
    
    /// <summary>
    /// Получает выполненные задания студента
    /// </summary>
    Task<IEnumerable<Submission>> GetSubmissionsByStudentAsync(Guid studentUid);
    
    /// <summary>
    /// Получает выполненные задания по заданию
    /// </summary>
    Task<IEnumerable<Submission>> GetSubmissionsByAssignmentAsync(Guid assignmentUid);
    
    /// <summary>
    /// Получает выполненное задание студента по конкретному заданию
    /// </summary>
    Task<Submission?> GetSubmissionByStudentAndAssignmentAsync(Guid studentUid, Guid assignmentUid);
    
    /// <summary>
    /// Добавляет новое выполненное задание
    /// </summary>
    Task AddSubmissionAsync(Submission submission);
    
    /// <summary>
    /// Обновляет существующее выполненное задание
    /// </summary>
    Task<bool> UpdateSubmissionAsync(Submission submission);
    
    /// <summary>
    /// Удаляет выполненное задание
    /// </summary>
    Task<bool> DeleteSubmissionAsync(Guid uid);
    
    /// <summary>
    /// Оценивает выполненное задание
    /// </summary>
    Task<bool> GradeSubmissionAsync(Guid uid, int grade, string feedback);
}
