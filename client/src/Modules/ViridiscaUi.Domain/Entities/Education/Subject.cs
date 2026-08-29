using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.Analytics;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Учебный предмет
/// </summary>
public class Subject : AuditableEntity
{
    /// <summary>
    /// Код предмета
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Название предмета
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание предмета
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Категория предмета
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Пререквизиты (предварительные требования)
    /// </summary>
    public string? Prerequisites { get; set; }

    /// <summary>
    /// Результаты обучения
    /// </summary>
    public string? LearningOutcomes { get; set; }

    /// <summary>
    /// Количество кредитов
    /// </summary>
    public int Credits { get; set; }

    /// <summary>
    /// Количество занятий в неделю
    /// </summary>
    public int LessonsPerWeek { get; set; } = 1;
    
    /// <summary>
    /// Количество часов в неделю
    /// </summary>
    public int Hours { get; set; } = 1;
    
    /// <summary>
    /// Тип предмета
    /// </summary>
    public SubjectType Type { get; set; }

    /// <summary>
    /// Статус предмета (для совместимости с ViewModels)
    /// </summary>
    public CourseStatus Status => IsActive && !IsDeleted ? CourseStatus.Active : CourseStatus.Suspended;

    /// <summary>
    /// Идентификатор кафедры/отдела
    /// </summary>
    public Guid? DepartmentUid { get; set; }

    /// <summary>
    /// Кафедра/отдел
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// Флаг активности предмета
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Экземпляры курса по этому предмету
    /// </summary>
    public ICollection<CourseInstance> CourseInstances { get; set; } = new List<CourseInstance>();
    
    /// <summary>
    /// Предметы в учебных планах
    /// </summary>
    public ICollection<CurriculumSubject> CurriculumSubjects { get; set; } = new List<CurriculumSubject>();

    /// <summary>
    /// Аналитические данные предмета
    /// </summary>
    public ICollection<SubjectAnalytics> Analytics { get; set; } = new List<SubjectAnalytics>();

    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public Subject()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        IsActive = true;
    }

    /// <summary>
    /// Конструктор с параметрами
    /// </summary>
    public Subject(string code, string name, string description, int credits, SubjectType type, string category, Guid? departmentUid) : this()
    {
        Code = code;
        Name = name;
        Description = description;
        Credits = credits;
        Type = type;
        Category = category;
        DepartmentUid = departmentUid;
    }
} 