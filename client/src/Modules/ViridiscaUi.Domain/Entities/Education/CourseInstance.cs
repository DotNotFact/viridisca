using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.Analytics;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Экземпляр курса (предмет для конкретной группы в конкретном периоде)
/// </summary>
public class CourseInstance : AuditableEntity
{
    /// <summary>
    /// ID предмета
    /// </summary>
    public Guid SubjectUid { get; set; }

    /// <summary>
    /// ID курса (альтернативное название для SubjectUid)
    /// </summary>
    public Guid CourseUid 
    { 
        get => SubjectUid; 
        set => SubjectUid = value; 
    }

    /// <summary>
    /// ID группы
    /// </summary>
    public Guid GroupUid { get; set; }

    /// <summary>
    /// ID академического периода
    /// </summary>
    public Guid AcademicPeriodUid { get; set; }

    /// <summary>
    /// ID преподавателя
    /// </summary>
    public Guid? TeacherUid { get; set; }

    /// <summary>
    /// Название экземпляра курса
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Код экземпляра курса
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Описание
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Заметки
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Дата начала
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Дата окончания
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Максимальное количество записей
    /// </summary>
    public int MaxEnrollments { get; set; }

    /// <summary>
    /// Статус курса
    /// </summary>
    public CourseStatus Status { get; set; }

    /// <summary>
    /// Активен ли курс
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Предмет
    /// </summary>
    public Subject? Subject { get; set; }

    /// <summary>
    /// Группа
    /// </summary>
    public Group? Group { get; set; }

    /// <summary>
    /// Академический период
    /// </summary>
    public AcademicPeriod? AcademicPeriod { get; set; }

    /// <summary>
    /// Преподаватель
    /// </summary>
    public Teacher? Teacher { get; set; }

    /// <summary>
    /// Записи студентов
    /// </summary>
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    /// <summary>
    /// Задания
    /// </summary>
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    /// <summary>
    /// Занятия
    /// </summary>
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    /// <summary>
    /// Слоты расписания
    /// </summary>
    public ICollection<ScheduleSlot> ScheduleSlots { get; set; } = new List<ScheduleSlot>();
    
    /// <summary>
    /// Экзамены
    /// </summary>
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();

    /// <summary>
    /// Тесты/викторины
    /// </summary>
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    /// <summary>
    /// Обсуждения/форумы
    /// </summary>
    public ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();

    /// <summary>
    /// Аналитические данные экземпляра курса
    /// </summary>
    public ICollection<CourseAnalytics> Analytics { get; set; } = new List<CourseAnalytics>();
} 