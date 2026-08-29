namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Статусы академического периода
/// </summary>
public enum AcademicPeriodStatus
{
    /// <summary>
    /// Планируется
    /// </summary>
    Planned = 0,

    /// <summary>
    /// Активный
    /// </summary>
    Active = 1,

    /// <summary>
    /// Завершенный
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Отмененный
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Приостановленный
    /// </summary>
    Suspended = 4
} 