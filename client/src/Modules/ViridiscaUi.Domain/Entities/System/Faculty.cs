using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education;

namespace ViridiscaUi.Domain.Entities.System;

/// <summary>
/// Факультет учебного заведения
/// </summary>
public class Faculty : AuditableEntity
{
    /// <summary>
    /// Название факультета
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание факультета
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Код факультета
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор декана факультета
    /// </summary>
    public Guid? DeanUid { get; set; }

    /// <summary>
    /// Декан факультета
    /// </summary>
    public Teacher? Dean { get; set; }

    /// <summary>
    /// Флаг активности факультета
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Департаменты, входящие в факультет
    /// </summary>
    public ICollection<Department> Departments { get; set; } = new List<Department>();
    
    /// <summary>
    /// Группы, относящиеся к факультету
    /// </summary>
    public ICollection<Group> Groups { get; set; } = new List<Group>();

    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public Faculty()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        IsActive = true;
    }

    /// <summary>
    /// Конструктор с параметрами
    /// </summary>
    public Faculty(string name, string code, string description) : this()
    {
        Name = name;
        Code = code;
        Description = description;
    }
} 