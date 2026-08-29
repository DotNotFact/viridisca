using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education;

namespace ViridiscaUi.Domain.Entities.Analytics;

/// <summary>
/// Аналитика предмета
/// </summary>
public class SubjectAnalytics : AuditableEntity
{
    /// <summary>
    /// Идентификатор предмета
    /// </summary>
    public Guid SubjectUid { get; set; }

    // === STORED FIELDS (обновляются периодически) ===

    /// <summary>
    /// Общее количество экземпляров курсов по предмету
    /// </summary>
    public int TotalCourseInstances { get; set; }

    /// <summary>
    /// Количество активных экземпляров курсов
    /// </summary>
    public int ActiveCourseInstances { get; set; }

    /// <summary>
    /// Общее количество студентов, изучавших предмет
    /// </summary>
    public int TotalStudentsEnrolled { get; set; }

    /// <summary>
    /// Средняя оценка по предмету
    /// </summary>
    public decimal AverageGrade { get; set; }

    /// <summary>
    /// Процент успешного завершения предмета
    /// </summary>
    public decimal CompletionRate { get; set; }

    /// <summary>
    /// Количество заданий по предмету
    /// </summary>
    public int TotalAssignments { get; set; }

    /// <summary>
    /// Количество уроков по предмету
    /// </summary>
    public int TotalLessons { get; set; }

    /// <summary>
    /// Дата последнего обновления аналитики
    /// </summary>
    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;

    // === NAVIGATION PROPERTIES ===

    /// <summary>
    /// Предмет
    /// </summary>
    public Subject? Subject { get; set; }

    // === COMPUTED PROPERTIES (автоматически вычисляются через связи) ===

    /// <summary>
    /// Общее количество экземпляров курсов - вычисляется из связей
    /// </summary>
    public int CourseInstancesCount => Subject?.CourseInstances?.Count ?? 0;

    /// <summary>
    /// Количество активных экземпляров курсов - вычисляется из активных курсов
    /// </summary>
    public int ActiveCourseInstancesCount => Subject?.CourseInstances?
        .Count(ci => ci.IsActive && !ci.IsDeleted) ?? 0;

    /// <summary>
    /// Общее количество записей студентов - вычисляется из enrollments
    /// </summary>
    public int EnrollmentsCount => Subject?.CourseInstances?
        .SelectMany(ci => ci.Enrollments)
        .Count() ?? 0;

    /// <summary>
    /// Количество активных записей студентов
    /// </summary>
    public int ActiveEnrollmentsCount => Subject?.CourseInstances?
        .SelectMany(ci => ci.Enrollments)
        .Count(e => e.IsActive) ?? 0;

    /// <summary>
    /// Средняя оценка по предмету - вычисляется из всех оценок
    /// </summary>
    public decimal CalculatedAverageGrade
    {
        get
        {
            var allGrades = Subject?.CourseInstances?
                .SelectMany(ci => ci.Enrollments)
                .Where(e => e.FinalGrade.HasValue)
                .Select(e => e.FinalGrade!.Value)
                .ToList();

            if (allGrades?.Any() != true)
                return 0m;

            return (decimal)allGrades.Average();
        }
    }

    /// <summary>
    /// Обновляет кэшированные поля аналитики
    /// </summary>
    public void RefreshCalculatedFields()
    {
        TotalCourseInstances = CourseInstancesCount;
        ActiveCourseInstances = ActiveCourseInstancesCount;
        TotalStudentsEnrolled = EnrollmentsCount;
        AverageGrade = CalculatedAverageGrade;
        
        // Расчет процента завершения
        var completedEnrollments = Subject?.CourseInstances?
            .SelectMany(ci => ci.Enrollments)
            .Count(e => e.Status == Education.Enums.EnrollmentStatus.Completed) ?? 0;
        
        var totalEnrollments = EnrollmentsCount;
        CompletionRate = totalEnrollments > 0 ? (decimal)completedEnrollments / totalEnrollments * 100 : 0m;

        // Подсчет заданий и уроков
        TotalAssignments = Subject?.CourseInstances?
            .SelectMany(ci => ci.Assignments)
            .Count() ?? 0;

        TotalLessons = Subject?.CourseInstances?
            .SelectMany(ci => ci.Lessons)
            .Count() ?? 0;

        LastCalculated = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
} 