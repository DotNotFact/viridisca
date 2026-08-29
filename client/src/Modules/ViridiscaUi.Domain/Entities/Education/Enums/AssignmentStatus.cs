using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Статус задания
/// </summary>
public enum AssignmentStatus
{
    /// <summary>
    /// Черновик
    /// </summary>
    [Description("Черновик")]
    Draft,
    
    /// <summary>
    /// Опубликовано
    /// </summary>
    [Description("Опубликовано")]
    Published,
    
    /// <summary>
    /// Активно (доступно для выполнения)
    /// </summary>
    [Description("Активно")]
    Active,
    
    /// <summary>
    /// В процессе выполнения
    /// </summary>
    [Description("В процессе")]
    InProgress,
    
    /// <summary>
    /// Сдано
    /// </summary>
    [Description("Сдано")]
    Submitted,
    
    /// <summary>
    /// Оценено
    /// </summary>
    [Description("Оценено")]
    Graded,
    
    /// <summary>
    /// Завершено
    /// </summary>
    [Description("Завершено")]
    Completed,
    
    /// <summary>
    /// Просрочено
    /// </summary>
    [Description("Просрочено")]
    Overdue,
    
    /// <summary>
    /// Закрыто для сдачи
    /// </summary>
    [Description("Закрыто")]
    Closed,
    
    /// <summary>
    /// Отменено
    /// </summary>
    [Description("Отменено")]
    Cancelled,
    
    /// <summary>
    /// Архивировано
    /// </summary>
    [Description("Архивировано")]
    Archived
} 