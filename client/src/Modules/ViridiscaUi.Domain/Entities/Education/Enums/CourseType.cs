using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Тип курса
/// </summary>
public enum CourseType
{
    /// <summary>
    /// Обязательный курс
    /// </summary>
    [Description("Обязательный")]
    Core = 1,

    /// <summary>
    /// Элективный курс
    /// </summary>
    [Description("Элективный")]
    Elective = 2,

    /// <summary>
    /// Факультативный курс
    /// </summary>
    [Description("Факультативный")]
    Optional = 3,

    /// <summary>
    /// Практический курс
    /// </summary>
    [Description("Практический")]
    Practical = 4,

    /// <summary>
    /// Лабораторный курс
    /// </summary>
    [Description("Лабораторный")]
    Laboratory = 5,

    /// <summary>
    /// Семинар
    /// </summary>
    [Description("Семинар")]
    Seminar = 6
} 