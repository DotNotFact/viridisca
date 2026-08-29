using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Статусы студентов
/// </summary>
public enum StudentStatus
{
    /// <summary>
    /// Активный студент
    /// </summary>
    [Description("Активный")]
    Active = 1,

    /// <summary>
    /// Неактивный студент
    /// </summary>
    [Description("Неактивный")]
    Inactive = 2,

    /// <summary>
    /// Студент в академическом отпуске
    /// </summary>
    [Description("Академический отпуск")]
    AcademicLeave = 3,

    /// <summary>
    /// Отчисленный студент
    /// </summary>
    [Description("Отчислен")]
    Expelled = 4,

    /// <summary>
    /// Выпущенный студент
    /// </summary>
    [Description("Выпущен")]
    Graduated = 5,

    /// <summary>
    /// Переведенный студент
    /// </summary>
    [Description("Переведен")]
    Transferred = 6,

    /// <summary>
    /// Студент с приостановленным обучением
    /// </summary>
    [Description("Приостановлен")]
    Suspended = 7
} 