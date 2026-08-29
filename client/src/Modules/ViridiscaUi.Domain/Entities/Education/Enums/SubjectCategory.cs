using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Категории предметов
/// </summary>
public enum SubjectCategory
{
    /// <summary>
    /// Математика
    /// </summary>
    [Description("Математика")]
    Mathematics = 1,

    /// <summary>
    /// Естественные науки
    /// </summary>
    [Description("Естественные науки")]
    NaturalSciences = 2,

    /// <summary>
    /// Гуманитарные науки
    /// </summary>
    [Description("Гуманитарные науки")]
    Humanities = 3,

    /// <summary>
    /// Информатика
    /// </summary>
    [Description("Информатика")]
    ComputerScience = 4,

    /// <summary>
    /// Языки
    /// </summary>
    [Description("Языки")]
    Languages = 5,

    /// <summary>
    /// Искусство
    /// </summary>
    [Description("Искусство")]
    Arts = 6,

    /// <summary>
    /// Спорт
    /// </summary>
    [Description("Спорт")]
    Sports = 7,

    /// <summary>
    /// Экономика
    /// </summary>
    [Description("Экономика")]
    Economics = 8,

    /// <summary>
    /// Инженерия
    /// </summary>
    [Description("Инженерия")]
    Engineering = 9,

    /// <summary>
    /// Другое
    /// </summary>
    [Description("Другое")]
    Other = 10
}
