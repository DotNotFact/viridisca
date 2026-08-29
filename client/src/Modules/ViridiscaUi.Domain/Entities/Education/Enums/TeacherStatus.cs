using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Статусы преподавателей
/// </summary>
public enum TeacherStatus
{
    /// <summary>
    /// Активный преподаватель
    /// </summary>
    [Description("Активный")]
    Active = 1,

    /// <summary>
    /// Преподаватель в отпуске
    /// </summary>
    [Description("В отпуске")]
    OnLeave = 2,

    /// <summary>
    /// Уволенный преподаватель
    /// </summary>
    [Description("Уволен")]
    Terminated = 3,

    /// <summary>
    /// Временно неактивный преподаватель
    /// </summary>
    [Description("Временно неактивен")]
    Inactive = 4,

    /// <summary>
    /// Преподаватель на пенсии
    /// </summary>
    [Description("На пенсии")]
    Retired = 5,

    /// <summary>
    /// Отстраненный преподаватель
    /// </summary>
    [Description("Отстранен")]
    Suspended = 6
} 