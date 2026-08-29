using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education;

namespace ViridiscaUi.Domain.Entities.System;

/// <summary>
/// Департамент/Кафедра учебного заведения
/// </summary>
public class Department : AuditableEntity
{
    /// <summary>
    /// Название департамента
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание департамента
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Код департамента
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор заведующего кафедрой
    /// </summary>
    public Guid? HeadOfDepartmentUid { get; set; }

    /// <summary>
    /// Заведующий кафедрой
    /// </summary>
    public Teacher? HeadOfDepartment { get; set; }

    /// <summary>
    /// Идентификатор родительского департамента
    /// </summary>
    public Guid? ParentDepartmentUid { get; set; }

    /// <summary>
    /// Идентификатор факультета, к которому принадлежит департамент
    /// </summary>
    public Guid? FacultyUid { get; set; }

    /// <summary>
    /// Родительский департамент
    /// </summary>
    public Department? ParentDepartment { get; set; }

    /// <summary>
    /// Дочерние департаменты
    /// </summary>
    public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();

    /// <summary>
    /// Флаг активности департамента
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Преподаватели, приписанные к департаменту
    /// </summary>
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    
    /// <summary>
    /// Предметы, закрепленные за департаментом
    /// </summary>
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();

    /// <summary>
    /// Группы, относящиеся к департаменту
    /// </summary>
    public ICollection<Group> Groups { get; set; } = new List<Group>();

    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public Department()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        IsActive = true;
    }

    /// <summary>
    /// Конструктор с параметрами
    /// </summary>
    public Department(string name, string code, string description) : this()
    {
        Name = name;
        Code = code;
        Description = description;
    }
} 