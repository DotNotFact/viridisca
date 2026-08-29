namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Типы вопросов в тестах
/// </summary>
public enum QuestionType
{
    /// <summary>
    /// Множественный выбор (один правильный ответ)
    /// </summary>
    MultipleChoice = 1,

    /// <summary>
    /// Множественный выбор (несколько правильных ответов)
    /// </summary>
    MultipleAnswer = 2,

    /// <summary>
    /// Верно/неверно
    /// </summary>
    TrueFalse = 3,

    /// <summary>
    /// Текстовый ответ
    /// </summary>
    TextAnswer = 4,

    /// <summary>
    /// Числовой ответ
    /// </summary>
    NumericAnswer = 5,

    /// <summary>
    /// Эссе (развернутый ответ)
    /// </summary>
    Essay = 6,

    /// <summary>
    /// Соответствие (сопоставление)
    /// </summary>
    Matching = 7,

    /// <summary>
    /// Упорядочивание
    /// </summary>
    Ordering = 8
} 