using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Entities.Analytics;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Учебная группа
/// </summary>
public class Group : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int MaxStudents { get; set; } = 30;
    public Guid DepartmentUid { get; set; }
    public Guid? CuratorUid { get; set; }
    public GroupStatus Status { get; set; } = GroupStatus.Active;
    
    /// <summary>
    /// Идентификатор учебного плана
    /// </summary>
    public Guid? CurriculumUid { get; set; }
    
    /// <summary>
    /// Идентификатор академического периода
    /// </summary>
    public Guid? AcademicPeriodUid { get; set; }
    
    /// <summary>
    /// Активна ли группа
    /// </summary>
    public bool IsActive { get; set; } = true;
        
    // Navigation properties
    public Department? Department { get; set; }
    public Teacher? Curator { get; set; }
    public ICollection<Student> Students { get; set; } = new List<Student>();
    
    /// <summary>
    /// Учебный план группы
    /// </summary>
    public Curriculum? Curriculum { get; set; }
    
    /// <summary>
    /// Академический период группы
    /// </summary>
    public AcademicPeriod? AcademicPeriod { get; set; }

    /// <summary>
    /// Аналитические данные группы
    /// </summary>
    public ICollection<GroupAnalytics> Analytics { get; set; } = new List<GroupAnalytics>();
} 