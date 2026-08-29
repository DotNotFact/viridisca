using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.System.Enums;

namespace ViridiscaUi.Domain.Entities.Analytics;

/// <summary>
/// Аналитика урока
/// </summary>
public class LessonAnalytics : AuditableEntity
{
    /// <summary>
    /// Идентификатор урока
    /// </summary>
    public Guid LessonUid { get; set; }

    // === STORED FIELDS (обновляются периодически) ===

    /// <summary>
    /// Общее количество студентов на уроке
    /// </summary>
    public int TotalStudents { get; set; }

    /// <summary>
    /// Количество присутствовавших студентов
    /// </summary>
    public int PresentStudents { get; set; }

    /// <summary>
    /// Количество отсутствовавших студентов
    /// </summary>
    public int AbsentStudents { get; set; }

    /// <summary>
    /// Процент посещаемости
    /// </summary>
    public decimal AttendanceRate { get; set; }

    /// <summary>
    /// Количество студентов, завершивших урок
    /// </summary>
    public int CompletedStudents { get; set; }

    /// <summary>
    /// Средний прогресс урока (в процентах)
    /// </summary>
    public decimal AverageProgress { get; set; }

    /// <summary>
    /// Дата последнего обновления аналитики
    /// </summary>
    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;

    // === NAVIGATION PROPERTIES ===

    /// <summary>
    /// Урок
    /// </summary>
    public Lesson? Lesson { get; set; }

    // === COMPUTED PROPERTIES (автоматически вычисляются через связи) ===

    /// <summary>
    /// Общее количество записей посещаемости
    /// </summary>
    public int AttendanceRecordsCount => Lesson?.Attendances?.Count ?? 0;

    /// <summary>
    /// Количество присутствующих - вычисляется из записей посещаемости
    /// </summary>
    public int CalculatedPresentCount => Lesson?.Attendances?
        .Count(a => a.Status == AttendanceStatus.Present) ?? 0;

    /// <summary>
    /// Количество отсутствующих - вычисляется из записей посещаемости
    /// </summary>
    public int CalculatedAbsentCount => Lesson?.Attendances?
        .Count(a => a.Status == AttendanceStatus.Absent) ?? 0;

    /// <summary>
    /// Процент посещаемости - вычисляется из записей
    /// </summary>
    public decimal CalculatedAttendanceRate
    {
        get
        {
            var totalRecords = AttendanceRecordsCount;
            if (totalRecords == 0) return 0m;

            var presentCount = CalculatedPresentCount;
            return (decimal)presentCount / totalRecords * 100;
        }
    }

    /// <summary>
    /// Средний прогресс - вычисляется из прогресса студентов
    /// </summary>
    public decimal CalculatedAverageProgress
    {
        get
        {
            var progressRecords = Lesson?.LessonProgresses?.ToList();
            if (progressRecords?.Any() != true) return 0m;

            return progressRecords.Average(p => p.CompletionPercentage);
        }
    }

    /// <summary>
    /// Обновляет кэшированные поля аналитики
    /// </summary>
    public void RefreshCalculatedFields()
    {
        TotalStudents = AttendanceRecordsCount;
        PresentStudents = CalculatedPresentCount;
        AbsentStudents = CalculatedAbsentCount;
        AttendanceRate = CalculatedAttendanceRate;
        
        CompletedStudents = Lesson?.LessonProgresses?
            .Count(p => p.IsCompleted) ?? 0;
        
        AverageProgress = CalculatedAverageProgress;

        LastCalculated = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
} 