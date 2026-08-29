namespace ViridiscaUi.Domain.Models;

/// <summary>
/// Запрос на выставление оценки
/// </summary>
public class GradingRequest
{
    /// <summary>
    /// Идентификатор студента
    /// </summary>
    public Guid StudentUid { get; set; }

    /// <summary>
    /// Идентификатор задания
    /// </summary>
    public Guid AssignmentUid { get; set; }

    /// <summary>
    /// Идентификатор экзамена (опционально)
    /// </summary>
    public Guid? ExamUid { get; set; }

    /// <summary>
    /// Оценка
    /// </summary>
    public decimal Score { get; set; }

    /// <summary>
    /// Максимальная оценка
    /// </summary>
    public decimal? MaxScore { get; set; }

    /// <summary>
    /// Комментарий к оценке
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Обратная связь
    /// </summary>
    public string? Feedback { get; set; }

    /// <summary>
    /// Опубликована ли оценка для студента
    /// </summary>
    public bool IsPublished { get; set; } = true;

    /// <summary>
    /// Вес оценки
    /// </summary>
    public decimal Weight { get; set; } = 1.0m;

    /// <summary>
    /// Дополнительные метаданные
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Дата выставления оценки
    /// </summary>
    public DateTime? GradedAt { get; set; }

    /// <summary>
    /// Создает простой запрос на оценивание
    /// </summary>
    public static GradingRequest Create(Guid studentUid, Guid assignmentUid, decimal score, string? comment = null)
    {
        return new GradingRequest
        {
            StudentUid = studentUid,
            AssignmentUid = assignmentUid,
            Score = score,
            Comment = comment,
            GradedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Валидирует запрос
    /// </summary>
    public bool IsValid()
    {
        return StudentUid != Guid.Empty 
               && AssignmentUid != Guid.Empty 
               && Score >= 0 
               && (MaxScore == null || Score <= MaxScore);
    }
} 