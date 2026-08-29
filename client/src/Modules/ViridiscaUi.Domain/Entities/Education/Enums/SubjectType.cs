using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Типы предметов
/// </summary>
public enum SubjectType
{
    /// <summary>
    /// Обязательный предмет
    /// </summary>
    [Description("Обязательный")]
    Required = 1,

    /// <summary>
    /// Факультативный предмет
    /// </summary>
    [Description("Факультативный")]
    Elective = 2,

    /// <summary>
    /// Специализированный предмет
    /// </summary>
    [Description("Специализированный")]
    Specialized = 3,

    /// <summary>
    /// Практикум
    /// </summary>
    [Description("Практикум")]
    Practicum = 4,

    /// <summary>
    /// Семинар
    /// </summary>
    [Description("Семинар")]
    Seminar = 5,
    Laboratory = 6,
    Lecture = 7
} 