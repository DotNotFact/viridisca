using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Статусы групп
/// </summary>
public enum GroupStatus
{
    /// <summary>
    /// Активная группа
    /// </summary>
    [Description("Активная")]
    Active = 1,

    /// <summary>
    /// Группа в процессе формирования
    /// </summary>
    [Description("Формирование")]
    Forming = 2,

    /// <summary>
    /// Приостановленная группа
    /// </summary>
    [Description("Приостановлена")]
    Suspended = 3,

    /// <summary>
    /// Завершившая обучение группа
    /// </summary>
    [Description("Завершена")]
    Completed = 4,

    /// <summary>
    /// Выпустившаяся группа
    /// </summary>
    [Description("Выпустилась")]
    Graduated = 5,

    /// <summary>
    /// Расформированная группа
    /// </summary>
    [Description("Расформирована")]
    Disbanded = 6,

    /// <summary>
    /// Архивная группа
    /// </summary>
    [Description("Архивная")]
    Archived = 7
} 