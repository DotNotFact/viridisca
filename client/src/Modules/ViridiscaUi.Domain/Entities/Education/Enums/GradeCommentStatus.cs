using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Статусы комментариев к оценкам
/// </summary>
public enum GradeCommentStatus
{
    /// <summary>
    /// Ожидает проверки
    /// </summary>
    [Description("Ожидает проверки")]
    PendingReview = 0,
    
    /// <summary>
    /// Одобрен
    /// </summary>
    [Description("Одобрен")]
    Approved = 1,
    
    /// <summary>
    /// Отклонен
    /// </summary>
    [Description("Отклонен")]
    Rejected = 2,
    
    /// <summary>
    /// Архивирован
    /// </summary>
    [Description("Архивирован")]
    Archived = 3
} 