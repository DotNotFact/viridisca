using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Тип экзамена
/// </summary>
public enum ExamType
{
    [Description("Письменный")]
    Written = 1,

    [Description("Устный")]
    Oral = 2,

    [Description("Практический")]
    Practical = 3,

    [Description("Онлайн")]
    Online = 4,

    [Description("Комбинированный")]
    Combined = 5,

    [Description("Проект")]
    Project = 6
} 