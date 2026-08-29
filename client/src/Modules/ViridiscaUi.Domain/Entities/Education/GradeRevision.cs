using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// История изменений оценки
/// </summary>
public class GradeRevision : AuditableEntity
{
    /// <summary>
    /// Идентификатор оценки
    /// </summary>
    public Guid GradeUid { get; set; }

    /// <summary>
    /// Идентификатор преподавателя, изменившего оценку
    /// </summary>
    public Guid ChangedByUid { get; set; }

    /// <summary>
    /// Предыдущее значение оценки
    /// </summary>
    public decimal PreviousValue { get; set; }

    /// <summary>
    /// Новое значение оценки
    /// </summary>
    public decimal NewValue { get; set; }

    /// <summary>
    /// Причина изменения оценки
    /// </summary>
    public string RevisionReason { get; set; } = string.Empty;

    /// <summary>
    /// Оценка
    /// </summary>
    public Grade? Grade { get; set; }
    
    /// <summary>
    /// Преподаватель
    /// </summary>
    public Teacher? ChangedBy { get; set; }
} 