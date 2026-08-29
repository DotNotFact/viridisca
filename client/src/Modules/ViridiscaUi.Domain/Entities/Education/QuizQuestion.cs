using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education.Enums;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Вопрос теста
/// </summary>
public class QuizQuestion : AuditableEntity
{
    /// <summary>
    /// Текст вопроса
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Тип вопроса
    /// </summary>
    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;

    /// <summary>
    /// Баллы за правильный ответ
    /// </summary>
    public double Points { get; set; } = 1.0;

    /// <summary>
    /// Порядковый номер вопроса в тесте
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Обязательный ли вопрос
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Идентификатор теста
    /// </summary>
    public Guid QuizUid { get; set; }

    /// <summary>
    /// Объяснение правильного ответа
    /// </summary>
    public string? Explanation { get; set; }

    /// <summary>
    /// Тест, к которому относится вопрос
    /// </summary>
    public Quiz? Quiz { get; set; }

    /// <summary>
    /// Варианты ответов на вопрос
    /// </summary>
    public ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
} 