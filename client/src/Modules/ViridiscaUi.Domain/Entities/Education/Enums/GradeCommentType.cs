using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Типы комментариев к оценкам
/// </summary>
public enum GradeCommentType
{
    /// <summary>
    /// Комментарий
    /// </summary>
    Comment = 0,
    
    /// <summary>
    /// Причина изменения оценки
    /// </summary>
    ChangeReason = 1
} 