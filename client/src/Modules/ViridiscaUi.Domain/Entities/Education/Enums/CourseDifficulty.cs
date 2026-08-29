using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Уровень сложности курса
/// </summary>
public enum CourseDifficulty
{
    /// <summary>
    /// Начальный уровень
    /// </summary>
    [Description("Начальный")]
    Beginner = 1,

    /// <summary>
    /// Базовый уровень
    /// </summary>
    [Description("Базовый")]
    Basic = 2,

    /// <summary>
    /// Средний уровень
    /// </summary>
    [Description("Средний")]
    Intermediate = 3,

    /// <summary>
    /// Продвинутый уровень
    /// </summary>
    [Description("Продвинутый")]
    Advanced = 4,

    /// <summary>
    /// Экспертный уровень
    /// </summary>
    [Description("Экспертный")]
    Expert = 5
} 