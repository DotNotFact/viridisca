using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Типы родственных отношений
/// </summary>
public enum ParentRelationType
{
    /// <summary>
    /// Мать
    /// </summary>
    [Description("Мать")]
    Mother,

    /// <summary>
    /// Отец
    /// </summary>
    [Description("Отец")]
    Father,

    /// <summary>
    /// Бабушка
    /// </summary>
    [Description("Бабушка")]
    Grandmother,

    /// <summary>
    /// Дедушка
    /// </summary>
    [Description("Дедушка")]
    Grandfather,

    /// <summary>
    /// Опекун
    /// </summary>
    [Description("Опекун")]
    Guardian,

    /// <summary>
    /// Другое
    /// </summary>
    [Description("Другое")]
    Other
} 