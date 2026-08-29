using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.System;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Учебный план
/// </summary>
public class Curriculum : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Code { get; set; }
    public int TotalCredits { get; set; }
    public int DurationSemesters { get; set; }
    
    /// <summary>
    /// Продолжительность в месяцах
    /// </summary>
    public int DurationMonths { get; set; }
    
    /// <summary>
    /// Академический год
    /// </summary>
    public int AcademicYear { get; set; }

    /// <summary>
    /// Год начала действия учебного плана
    /// </summary>
    public int StartYear { get; set; }

    /// <summary>
    /// Год окончания действия учебного плана
    /// </summary>
    public int? EndYear { get; set; }

    /// <summary>
    /// Продолжительность в семестрах
    /// </summary>
    public int DurationInSemesters { get; set; } = 8;
    
    public bool IsActive { get; set; } = true;

    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public Guid? DepartmentUid { get; set; }
    
    public Department? Department { get; set; }
    public ICollection<CurriculumSubject> CurriculumSubjects { get; set; } = new List<CurriculumSubject>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Group> Groups { get; set; } = new List<Group>();
    
    /// <summary>
    /// Предметы в учебном плане (вычисляемое свойство)
    /// </summary>
    public IEnumerable<Subject> Subjects => CurriculumSubjects.Select(cs => cs.Subject).Where(s => s != null)!;
} 