namespace ViridiscaUi.Domain.Entities.System.Enums;

/// <summary>
/// Тип уведомления
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Информационное
    /// </summary>
    Info = 0,

    /// <summary>
    /// Предупреждение
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Ошибка
    /// </summary>
    Error = 2,

    /// <summary>
    /// Успех
    /// </summary>
    Success = 3,

    /// <summary>
    /// Системное
    /// </summary>
    System = 4,

    /// <summary>
    /// Оценка
    /// </summary>
    Grade = 5,

    /// <summary>
    /// Посещаемость
    /// </summary>
    Attendance = 6,

    /// <summary>
    /// Задание
    /// </summary>
    Assignment = 7,

    /// <summary>
    /// Напоминание
    /// </summary>
    Reminder = 8
}
