using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.System.Enums;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Посещаемость студентов
/// </summary>
public class Attendance : AuditableEntity
{
    /// <summary>
    /// Идентификатор студента
    /// </summary>
    public Guid StudentUid { get; set; }

    /// <summary>
    /// Идентификатор урока
    /// </summary>
    public Guid LessonUid { get; set; }

    /// <summary>
    /// Статус посещения
    /// </summary>
    public AttendanceStatus Status { get; set; }

    /// <summary>
    /// Заметки о посещении
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Дата и время отметки посещения
    /// </summary>
    public DateTime? CheckedAt { get; set; }

    /// <summary>
    /// Идентификатор преподавателя, отметившего посещение
    /// </summary>
    public Guid? CheckedByUid { get; set; }

    /// <summary>
    /// Студент
    /// </summary>
    public Student? Student { get; set; }

    /// <summary>
    /// Урок
    /// </summary>
    public Lesson? Lesson { get; set; }
} 