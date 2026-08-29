namespace ViridiscaUi.Domain.Entities.System.Enums;

/// <summary>
/// Статус посещения
/// </summary>
public enum AttendanceStatus
{
    /// <summary>
    /// Присутствовал
    /// </summary>
    Present = 0,

    /// <summary>
    /// Отсутствовал
    /// </summary>
    Absent = 1,

    /// <summary>
    /// Опоздал
    /// </summary>
    Late = 2,

    /// <summary>
    /// Ушел раньше
    /// </summary>
    LeftEarly = 3,

    /// <summary>
    /// Уважительная причина
    /// </summary>
    Excused = 4
} 