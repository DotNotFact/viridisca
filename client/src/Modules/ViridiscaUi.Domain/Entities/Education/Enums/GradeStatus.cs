using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Статус оценки
/// </summary>
public enum GradeStatus
{
    /// <summary>
    /// Черновик
    /// </summary>
    [Description("Черновик")]
    Draft,

    /// <summary>
    /// Выставлена
    /// </summary>
    [Description("Выставлена")]
    Graded,

    /// <summary>
    /// Опубликована
    /// </summary>
    [Description("Опубликована")]
    Published,

    /// <summary>
    /// На рассмотрении
    /// </summary>
    [Description("На рассмотрении")]
    UnderReview,

    /// <summary>
    /// Отклонена
    /// </summary>
    [Description("Отклонена")]
    Rejected,

    /// <summary>
    /// Архивирована
    /// </summary>
    [Description("Архивирована")]
    Archived
}
