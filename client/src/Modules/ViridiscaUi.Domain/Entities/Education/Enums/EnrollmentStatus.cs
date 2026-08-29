using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Статус записи на курс
/// </summary>
public enum EnrollmentStatus
{
    [Description("Активная")]
    Active = 1,

    [Description("Записан")]
    Enrolled = 2,

    [Description("Завершена")]
    Completed = 3,

    [Description("Отменена")]
    Cancelled = 4,

    [Description("Приостановлена")]
    Suspended = 5,

    [Description("Провалена")]
    Failed = 6,

    [Description("Отчислена")]
    Dropped = 7,

    [Description("В ожидании")]
    Pending = 8,

    [Description("В отпуске")]
    OnLeave = 9
} 