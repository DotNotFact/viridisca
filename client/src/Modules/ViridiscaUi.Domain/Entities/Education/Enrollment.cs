using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education.Enums;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Запись студента на курс
/// </summary>
public class Enrollment : AuditableEntity
{
    /// <summary>
    /// ID студента
    /// </summary>
    public Guid StudentUid { get; set; }

    /// <summary>
    /// ID экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid { get; set; }

    /// <summary>
    /// Дата записи
    /// </summary>
    public DateTime EnrollmentDate { get; set; }

    /// <summary>
    /// Дата завершения
    /// </summary>
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Дата завершения (алиас для совместимости)
    /// </summary>
    public DateTime? CompletedAt 
    { 
        get => CompletionDate; 
        set => CompletionDate = value; 
    }
    
    /// <summary>
    /// Дата записи (алиас для совместимости)
    /// </summary>
    public DateTime EnrolledAt 
    { 
        get => EnrollmentDate; 
        set => EnrollmentDate = value; 
    }

    /// <summary>
    /// Статус записи
    /// </summary>
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    /// <summary>
    /// Итоговая оценка
    /// </summary>
    public decimal? FinalGrade { get; set; }

    /// <summary>
    /// Заметки
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Активна ли запись (для совместимости с ViewModels)
    /// </summary>
    public bool IsActive => !IsDeleted && Status != EnrollmentStatus.Cancelled && Status != EnrollmentStatus.Dropped;

    /// <summary>
    /// ID курса (для совместимости с ViewModels)
    /// </summary>
    public Guid? CourseUid => CourseInstance?.SubjectUid;

    /// <summary>
    /// Студент
    /// </summary>
    public Student? Student { get; set; }

    /// <summary>
    /// Экземпляр курса
    /// </summary>
    public CourseInstance? CourseInstance { get; set; }

    /// <summary>
    /// Курс (делегирует к CourseInstance.Subject)
    /// </summary>
    public Subject? Course => CourseInstance?.Subject;
} 