using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Вариант ответа на вопрос теста
/// </summary>
public class QuizAnswer : AuditableEntity
{
    /// <summary>
    /// Текст ответа
    /// </summary>
    public string AnswerText { get; set; } = string.Empty;

    /// <summary>
    /// Правильный ли это ответ
    /// </summary>
    public bool IsCorrect { get; set; } = false;

    /// <summary>
    /// Порядковый номер ответа
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Идентификатор вопроса
    /// </summary>
    public Guid QuestionUid { get; set; }

    /// <summary>
    /// Вопрос, к которому относится ответ
    /// </summary>
    public QuizQuestion? Question { get; set; }
} 