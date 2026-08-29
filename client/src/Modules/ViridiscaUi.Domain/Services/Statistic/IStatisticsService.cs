using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Models;

namespace ViridiscaUi.Domain.Services.Statistic;

/// <summary>
/// Сервис для получения аналитики и статистики системы
/// </summary>
public interface IStatisticsService
{ 
    /// <summary>
    /// Получает аналитику студента за указанный академический период
    /// </summary>
    /// <param name="studentId">Идентификатор студента</param>
    /// <param name="academicPeriodId">Идентификатор академического периода (если null, используется текущий)</param>
    Task<StudentAnalytics> GetStudentAnalyticsAsync(Guid studentId, Guid? academicPeriodId = null);
    
    /// <summary>
    /// Получает аналитику преподавателя
    /// </summary>
    /// <param name="teacherId">Идентификатор преподавателя</param>
    Task<TeacherAnalytics> GetTeacherAnalyticsAsync(Guid teacherId);
    
    /// <summary>
    /// Получает аналитику курса
    /// </summary>
    /// <param name="courseInstanceId">Идентификатор экземпляра курса</param>
    Task<CourseAnalytics> GetCourseAnalyticsAsync(Guid courseInstanceId);
    
    /// <summary>
    /// Получает аналитику группы
    /// </summary>
    /// <param name="groupId">Идентификатор группы</param>
    Task<GroupAnalytics> GetGroupAnalyticsAsync(Guid groupId);
    
    /// <summary>
    /// Получает аналитику задания
    /// </summary>
    /// <param name="assignmentId">Идентификатор задания</param>
    Task<AssignmentAnalytics> GetAssignmentAnalyticsAsync(Guid assignmentId);
    
    /// <summary>
    /// Обновляет всю аналитику в системе
    /// </summary>
    Task RefreshAllAnalyticsAsync();

    /// <summary>
    /// Получает системную статистику
    /// </summary>
    Task<SystemStatistics> GetSystemStatisticsAsync();
}