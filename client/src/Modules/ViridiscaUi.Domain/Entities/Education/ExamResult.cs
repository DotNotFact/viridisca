using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Результат экзамена студента
/// </summary>
public class ExamResult : AuditableEntity
{
    /// <summary>
    /// ID экзамена
    /// </summary>
    public Guid ExamUid { get; set; }

    /// <summary>
    /// ID студента
    /// </summary>
    public Guid StudentUid { get; set; }

    /// <summary>
    /// Полученный балл
    /// </summary>
    public decimal Score { get; set; }

    /// <summary>
    /// Обратная связь от преподавателя
    /// </summary>
    public string? Feedback { get; set; }

    /// <summary>
    /// Время сдачи экзамена
    /// </summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// Время выставления оценки
    /// </summary>
    public DateTime? GradedAt { get; set; }

    /// <summary>
    /// Отсутствовал ли студент
    /// </summary>
    public bool IsAbsent { get; set; }

    /// <summary>
    /// Дополнительные заметки
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Экзамен
    /// </summary>
    public Exam? Exam { get; set; }

    /// <summary>
    /// Студент
    /// </summary>
    public Student? Student { get; set; }

    /// <summary>
    /// Процент от максимального балла
    /// </summary>
    public decimal Percentage => Exam?.MaxScore > 0 ? (Score / Exam.MaxScore) * 100 : 0;
} 