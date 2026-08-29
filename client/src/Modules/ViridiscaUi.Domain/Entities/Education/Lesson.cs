using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.Analytics;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Урок в рамках экземпляра курса
/// </summary>
public class Lesson : AuditableEntity
{
    /// <summary>
    /// Название урока
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание урока
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Содержимое урока
    /// </summary>
    public string? Content { get; set; }
    
    /// <summary>
    /// Идентификатор экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid { get; set; }
    
    /// <summary>
    /// Порядковый номер урока в курсе
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// Продолжительность урока
    /// </summary>
    public TimeSpan? Duration { get; set; }
    
    /// <summary>
    /// Тип урока
    /// </summary>
    public LessonType Type { get; set; } = LessonType.Lecture;
    
    /// <summary>
    /// Статус урока
    /// </summary>
    public LessonStatus Status { get; set; } = LessonStatus.Draft;
    
    /// <summary>
    /// Флаг публикации урока
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Экземпляр курса, к которому принадлежит урок
    /// </summary>
    public CourseInstance? CourseInstance { get; set; }
    
    /// <summary>
    /// Посещаемость по этому уроку
    /// </summary>
    public ICollection<Attendance> Attendances { get; set; } = [];
    
    /// <summary>
    /// Прогресс студентов по этому занятию
    /// </summary>
    public ICollection<LessonProgress> LessonProgresses { get; set; } = [];

    /// <summary>
    /// Прогресс студентов по уроку (алиас для совместимости)
    /// </summary>
    public ICollection<LessonProgress> LessonProgress 
    { 
        get => LessonProgresses; 
        set => LessonProgresses = value; 
    }

    /// <summary>
    /// Тесты, привязанные к уроку
    /// </summary>
    public ICollection<Quiz> Quizzes { get; set; } = [];

    /// <summary>
    /// Аналитические данные урока
    /// </summary>
    public ICollection<LessonAnalytics> Analytics { get; set; } = new List<LessonAnalytics>();
}
