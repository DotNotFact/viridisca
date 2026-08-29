using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Ответ студента на вопрос теста
/// </summary>
public class QuizStudentAnswer : AuditableEntity
{
    /// <summary>
    /// Идентификатор попытки прохождения теста
    /// </summary>
    public Guid AttemptUid { get; set; }

    /// <summary>
    /// Идентификатор вопроса
    /// </summary>
    public Guid QuestionUid { get; set; }

    /// <summary>
    /// Идентификатор выбранного ответа (для множественного выбора)
    /// </summary>
    public Guid? SelectedAnswerUid { get; set; }

    /// <summary>
    /// Текстовый ответ (для открытых вопросов)
    /// </summary>
    public string? TextAnswer { get; set; }

    /// <summary>
    /// Правильный ли ответ
    /// </summary>
    public bool? IsCorrect { get; set; }

    /// <summary>
    /// Баллы, полученные за ответ
    /// </summary>
    public double? PointsEarned { get; set; }

    /// <summary>
    /// Попытка прохождения теста
    /// </summary>
    public QuizAttempt? Attempt { get; set; }

    /// <summary>
    /// Вопрос
    /// </summary>
    public QuizQuestion? Question { get; set; }

    /// <summary>
    /// Выбранный ответ
    /// </summary>
    public QuizAnswer? SelectedAnswer { get; set; }
} 