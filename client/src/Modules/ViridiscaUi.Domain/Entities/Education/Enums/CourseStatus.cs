using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Статус курса
/// </summary>
public enum CourseStatus
{
    /// <summary>
    /// Черновик
    /// </summary>
    [Description("Черновик")]
    Draft,
    
    /// <summary>
    /// Активный
    /// </summary>
    [Description("Активный")]
    Active,
    
    /// <summary>
    /// В процессе
    /// </summary>
    [Description("В процессе")]
    InProgress,
    
    /// <summary>
    /// Опубликованный
    /// </summary>
    [Description("Опубликованный")]
    Published,
    
    /// <summary>
    /// Завершенный
    /// </summary>
    [Description("Завершенный")]
    Completed,
    
    /// <summary>
    /// Архивированный
    /// </summary>
    [Description("Архивированный")]
    Archived,
    
    /// <summary>
    /// Приостановленный
    /// </summary>
    [Description("Приостановленный")]
    Suspended,
    
    /// <summary>
    /// Отмененный
    /// </summary>
    [Description("Отмененный")]
    Cancelled
}
