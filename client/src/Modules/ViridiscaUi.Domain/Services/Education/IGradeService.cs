using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;

namespace ViridiscaUi.Domain.Services.Education;

/// <summary>
/// Интерфейс сервиса для работы с оценками
/// </summary>
public interface IGradeService
{
    // Базовые CRUD операции
    Task<Grade?> GetByUidAsync(Guid uid);
    Task<IEnumerable<Grade>> GetAllAsync();
    Task<Grade> CreateAsync(Grade grade);
    Task<bool> UpdateAsync(Grade grade);
    Task<bool> DeleteAsync(Guid uid);

    // Missing methods needed by ViewModels
    /// <summary>
    /// Получает оценки с пагинацией (алиас для GetGradesPagedAsync)
    /// </summary>
    Task<(IEnumerable<Grade> Grades, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? searchTerm = null,
        Guid? studentUid = null, Guid? courseInstanceUid = null);

    /// <summary>
    /// Получает оценку по студенту и заданию
    /// </summary>
    Task<Grade?> GetByStudentAndAssignmentAsync(Guid studentUid, Guid assignmentUid);

    // Специфичные методы для оценок
    Task<IEnumerable<Grade>> GetGradesByStudentAsync(Guid studentUid);
    Task<IEnumerable<Grade>> GetGradesByTeacherAsync(Guid teacherUid);
    Task<IEnumerable<Grade>> GetGradesByCourseInstanceAsync(Guid courseInstanceUid);
    Task<IEnumerable<Grade>> GetGradesByAssignmentAsync(Guid assignmentUid);
    Task<IEnumerable<Grade>> GetGradesByTypeAsync(GradeType gradeType);
    Task<double> GetStudentAverageGradeAsync(Guid studentUid);
    Task<double> GetCourseInstanceAverageGradeAsync(Guid courseInstanceUid);
    Task<(IEnumerable<Grade> Grades, int TotalCount)> GetGradesPagedAsync(
        int page, int pageSize, string? searchTerm = null,
        Guid? studentUid = null, Guid? courseInstanceUid = null);

    Task<bool> PublishGradeAsync(Guid gradeUid);
    Task<bool> UnpublishGradeAsync(Guid gradeUid);
    /// <summary>
    /// Получает статистику оценок для курса или студента
    /// </summary>
    /// <param name="courseInstanceUid">Идентификатор экземпляра курса (опционально)</param>
    /// <param name="studentUid">Идентификатор студента (опционально)</param>
    Task<(decimal AverageGrade, decimal MaxGrade, decimal MinGrade, int TotalGrades, Dictionary<string, int> GradeDistribution)> GetGradeStatisticsAsync(Guid? courseInstanceUid = null, Guid? studentUid = null);
    Task<IEnumerable<Grade>> GetRecentGradesAsync(int count = 10);
    Task<IEnumerable<Grade>> GetUnpublishedGradesAsync();
    Task<bool> BulkPublishGradesAsync(IEnumerable<Guid> gradeUids);
}
