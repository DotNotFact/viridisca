using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Entities.Analytics;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Курс/Предмет в системе
/// </summary>
public class Course : AuditableEntity
{
    /// <summary>
    /// Название курса
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Код курса
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Описание курса
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Тип курса
    /// </summary>
    public CourseType Type { get; set; }

    /// <summary>
    /// Сложность курса
    /// </summary>
    public CourseDifficulty Difficulty { get; set; }

    /// <summary>
    /// Количество кредитов
    /// </summary>
    public int Credits { get; set; }

    /// <summary>
    /// Продолжительность в часах
    /// </summary>
    public int DurationHours { get; set; }

    /// <summary>
    /// ID департамента
    /// </summary>
    public Guid? DepartmentUid { get; set; }

    /// <summary>
    /// ID предмета (связь с Subject)
    /// </summary>
    public Guid? SubjectUid { get; set; }

    /// <summary>
    /// Департамент
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// Предмет
    /// </summary>
    public Subject? Subject { get; set; }

    /// <summary>
    /// Активен ли курс
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Экземпляры курса
    /// </summary>
    public ICollection<CourseInstance> CourseInstances { get; set; } = new List<CourseInstance>();

    /// <summary>
    /// Аналитические данные курса
    /// </summary>
    public ICollection<CourseAnalytics> Analytics { get; set; } = new List<CourseAnalytics>();

    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public Course()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        IsActive = true;
    }

    /// <summary>
    /// Конструктор с параметрами
    /// </summary>
    public Course(string name, string code, string description) : this()
    {
        Name = name;
        Code = code;
        Description = description;
    }
} 