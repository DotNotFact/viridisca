namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Статус прогресса по уроку
/// </summary>
public enum LessonProgressStatus
{
    /// <summary>
    /// Не начат
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// В процессе
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Завершен
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Приостановлен
    /// </summary>
    Paused = 3,

    /// <summary>
    /// Пропущен
    /// </summary>
    Skipped = 4
} 