using ViridiscaUi.Domain.Entities.Base;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Предмет в учебном плане
/// </summary>
public class CurriculumSubject : AuditableEntity
{
    /// <summary>
    /// ID учебного плана
    /// </summary>
    public Guid CurriculumUid { get; set; }

    /// <summary>
    /// ID предмета
    /// </summary>
    public Guid SubjectUid { get; set; }

    /// <summary>
    /// Семестр изучения
    /// </summary>
    public int Semester { get; set; }

    /// <summary>
    /// Количество кредитов
    /// </summary>
    public int Credits { get; set; }

    /// <summary>
    /// Обязательный ли предмет
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Обязательный ли предмет (алиас для совместимости)
    /// </summary>
    public bool IsMandatory 
    { 
        get => IsRequired; 
        set => IsRequired = value; 
    }
    
    /// <summary>
    /// Учебный план
    /// </summary>
    public Curriculum? Curriculum { get; set; }

    /// <summary>
    /// Предмет
    /// </summary>
    public Subject? Subject { get; set; }
} 