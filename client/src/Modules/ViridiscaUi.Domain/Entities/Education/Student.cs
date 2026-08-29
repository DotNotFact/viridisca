using System.ComponentModel.DataAnnotations.Schema;
using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Студент
/// </summary>
public class Student : AuditableEntity
{
    /// <summary>
    /// ID связанного человека
    /// </summary>
    public Guid PersonUid { get; set; }

    /// <summary>
    /// Студенческий код
    /// </summary>
    public string StudentCode { get; set; } = string.Empty;

    /// <summary>
    /// Дата поступления
    /// </summary>
    public DateTime EnrollmentDate { get; set; }

    /// <summary>
    /// Дата выпуска
    /// </summary>
    public DateTime? GraduationDate { get; set; }

    /// <summary>
    /// Статус студента
    /// </summary>
    public StudentStatus Status { get; set; } = StudentStatus.Active;

    /// <summary>
    /// Средний балл студента
    /// </summary>
    public double GPA { get; set; } = 0.0;

    /// <summary>
    /// Академический год
    /// </summary>
    public int AcademicYear { get; set; } = DateTime.Now.Year;

    /// <summary>
    /// ID группы
    /// </summary>
    public Guid? GroupUid { get; set; }

    /// <summary>
    /// ID учебного плана
    /// </summary>
    public Guid? CurriculumUid { get; set; }

    /// <summary>
    /// Связанный человек
    /// </summary>
    public Person? Person { get; set; }

    /// <summary>
    /// Группа
    /// </summary>
    public Group? Group { get; set; }

    /// <summary>
    /// Учебный план
    /// </summary>
    public Curriculum? Curriculum { get; set; }

    /// <summary>
    /// Записи на курсы
    /// </summary>
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    /// <summary>
    /// Оценки
    /// </summary>
    public ICollection<Grade> Grades { get; set; } = new List<Grade>();

    /// <summary>
    /// Посещаемость
    /// </summary>
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    
    /// <summary>
    /// Результаты экзаменов
    /// </summary>
    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    
    /// <summary>
    /// Сданные работы
    /// </summary>
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    
    /// <summary>
    /// Прогресс по занятиям
    /// </summary>
    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();

    /// <summary>
    /// Попытки прохождения тестов
    /// </summary>
    public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();

    /// <summary>
    /// Аналитика студента
    /// </summary>
    public ICollection<StudentAnalytics> Analytics { get; set; } = new List<StudentAnalytics>();
    
    /// <summary>
    /// Является ли студент активным (не отчислен, не в академ. отпуске)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Полное имя студента (делегирует к Person.FullName)
    /// </summary>
    [NotMapped]
    public string FullName => Person?.FullName ?? "Неизвестный студент";
}