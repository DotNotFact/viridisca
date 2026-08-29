using ViridiscaUi.Domain.Entities.Education;

namespace ViridiscaUi.Domain.Services.Education;

/// <summary>
/// Сервис для управления экзаменами
/// </summary>
public interface IExamService
{
    Task<IEnumerable<Exam>> GetAllAsync();
    Task<Exam?> GetByIdAsync(Guid uid);
    Task<IEnumerable<Exam>> GetByCourseInstanceAsync(Guid courseInstanceUid);
    Task<IEnumerable<Exam>> GetByAcademicPeriodAsync(Guid academicPeriodUid);
    Task<Exam> CreateAsync(Exam exam);
    Task<Exam> UpdateAsync(Exam exam);
    Task<bool> DeleteAsync(Guid uid);
    Task<bool> ExistsAsync(Guid uid);
    Task<int> GetCountAsync();
    Task<IEnumerable<Exam>> GetUpcomingAsync();
    Task<IEnumerable<Exam>> GetCompletedAsync();
    Task<IEnumerable<Exam>> GetConflictingExamsAsync(Exam exam);
    Task<int> GetResultsCountAsync(Guid examUid);
    Task<Exam> PublishExamAsync(Guid examUid);
    Task<IEnumerable<object>> GetExamResultsAsync(Guid examUid);
    Task SendExamNotificationAsync(Guid examUid, string message);
    Task<object> GetExamStatisticsAsync();

    /// <summary>
    /// Получает экзамены с пагинацией
    /// </summary>
    Task<(IEnumerable<Exam> exams, int totalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null);

    /// <summary>
    /// Получает экзамен по идентификатору
    /// </summary>
    Task<Exam?> GetByUidAsync(Guid uid);

    /// <summary>
    /// Получает конфликтующие экзамены
    /// </summary>
    Task<IEnumerable<Exam>> GetConflictingExamsAsync(DateTime startTime, DateTime endTime, Guid? excludeExamUid = null);

    /// <summary>
    /// Получает статистику экзамена
    /// </summary>
    Task<object> GetExamStatisticsAsync(Guid examUid);
} 