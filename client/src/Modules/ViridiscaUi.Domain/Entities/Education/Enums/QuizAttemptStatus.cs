namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Статусы попыток прохождения теста
/// </summary>
public enum QuizAttemptStatus
{
    /// <summary>
    /// В процессе прохождения
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Завершено
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Заброшено/не завершено
    /// </summary>
    Abandoned = 3,

    /// <summary>
    /// Время истекло
    /// </summary>
    TimeExpired = 4,

    /// <summary>
    /// Отменено администратором
    /// </summary>
    Cancelled = 5
} 