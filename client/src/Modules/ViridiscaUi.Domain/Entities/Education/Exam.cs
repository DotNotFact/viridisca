using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.Analytics;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Экзамен
/// </summary>
public class Exam : AuditableEntity
{
    /// <summary>
    /// ID экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid { get; set; }

    /// <summary>
    /// ID академического периода
    /// </summary>
    public Guid AcademicPeriodUid { get; set; }

    /// <summary>
    /// Название экзамена
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Дата и время экзамена
    /// </summary>
    public DateTime ExamDate { get; set; }

    /// <summary>
    /// Продолжительность экзамена
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Место проведения
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Тип экзамена
    /// </summary>
    public ExamType Type { get; set; } = ExamType.Written;

    /// <summary>
    /// Максимальный балл
    /// </summary>
    public decimal MaxScore { get; set; } = 100;

    /// <summary>
    /// Опубликован ли экзамен
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Инструкции для экзамена
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Экземпляр курса
    /// </summary>
    public CourseInstance? CourseInstance { get; set; }

    /// <summary>
    /// Академический период
    /// </summary>
    public AcademicPeriod? AcademicPeriod { get; set; }

    /// <summary>
    /// Результаты экзамена
    /// </summary>
    public ICollection<ExamResult> Results { get; set; } = new List<ExamResult>();

    /// <summary>
    /// Аналитические данные экзамена
    /// </summary>
    public ICollection<ExamAnalytics> Analytics { get; set; } = new List<ExamAnalytics>();
} 