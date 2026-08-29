using System;
using System.Collections.Generic;
using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.Analytics;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Задание для студентов
/// </summary>
public class Assignment : AuditableEntity
{
    /// <summary>
    /// Название задания
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание задания
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Срок сдачи задания
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Максимальный балл за задание
    /// </summary>
    public double MaxScore { get; set; } = 100.0;

    /// <summary>
    /// Максимальные баллы за задание (альтернативное название)
    /// </summary>
    public double MaxPoints 
    { 
        get => MaxScore; 
        set => MaxScore = value; 
    }
    
    /// <summary>
    /// Тип задания
    /// </summary>
    public AssignmentType Type { get; set; } = AssignmentType.Homework;
     
    /// <summary>
    /// Идентификатор экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid { get; set; }

    /// <summary>
    /// Идентификатор урока (опционально)
    /// </summary>
    public Guid? LessonUid { get; set; }
    
    /// <summary>
    /// Экземпляр курса, к которому принадлежит задание
    /// </summary>
    public CourseInstance? CourseInstance { get; set; }

    /// <summary>
    /// Активно ли задание (для совместимости с ViewModels)
    /// </summary>
    public bool IsActive => Status != AssignmentStatus.Cancelled && Status != AssignmentStatus.Archived && !IsDeleted;

    /// <summary>
    /// ID курса (для совместимости с ViewModels)
    /// </summary>
    public Guid? CourseUid => CourseInstance?.SubjectUid;

    /// <summary>
    /// Курс (алиас для совместимости с ViewModels)
    /// </summary>
    public Subject? Course => CourseInstance?.Subject;

    /// <summary>
    /// Предмет (алиас для совместимости)
    /// </summary>
    public Subject? Subject => CourseInstance?.Subject;

    /// <summary>
    /// Преподаватель (алиас для совместимости)
    /// </summary>
    public Teacher? Teacher => CourseInstance?.Teacher;

    /// <summary>
    /// Создатель задания (алиас для совместимости)
    /// </summary>
    public Teacher? CreatedBy => CourseInstance?.Teacher;

    /// <summary>
    /// Урок, к которому принадлежит задание (опционально)
    /// </summary>
    public Lesson? Lesson { get; set; }
    
    /// <summary>
    /// Инструкции к заданию
    /// </summary>
    public string Instructions { get; set; } = string.Empty;
    
    /// <summary>
    /// Сложность задания
    /// </summary>
    public AssignmentDifficulty Difficulty { get; set; } = AssignmentDifficulty.Medium;

    /// <summary>
    /// Статус задания
    /// </summary>
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;
    
    /// <summary>
    /// Опубликовано ли задание
    /// </summary>
    public bool IsPublished { get; set; } = false;
    
    /// <summary>
    /// Путь к прикрепленным файлам
    /// </summary>
    public string AttachmentsPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Сданные работы по заданию
    /// </summary>
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    /// <summary>
    /// Аналитические данные задания
    /// </summary>
    public ICollection<AssignmentAnalytics> Analytics { get; set; } = new List<AssignmentAnalytics>();

    /// <summary>
    /// Создает новый экземпляр задания
    /// </summary>
    public Assignment()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Создает новый экземпляр задания с указанными параметрами
    /// </summary>
    public Assignment(string title, string description, Guid courseInstanceUid, AssignmentType type = AssignmentType.Homework)
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        Title = title.Trim();
        Description = description;
        CourseInstanceUid = courseInstanceUid;
        Type = type;
    }

    /// <summary>
    /// Проверяет, просрочено ли задание
    /// </summary>
    public bool IsOverdue => DueDate.HasValue && DateTime.UtcNow > DueDate.Value;

    /// <summary>
    /// Отображаемый тип задания
    /// </summary>
    public string TypeDisplayName => Type switch
    {
        AssignmentType.Homework => "Домашнее задание",
        AssignmentType.Quiz => "Тест",
        AssignmentType.Exam => "Экзамен",
        AssignmentType.Project => "Проект",
        AssignmentType.LabWork => "Лабораторная работа",
        _ => "Неизвестный тип"
    };
}