using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education.Enums;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Слот расписания - конкретное время проведения урока
/// </summary>
public class ScheduleSlot : AuditableEntity
{
    /// <summary>
    /// ID экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid { get; set; }

    /// <summary>
    /// День недели
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// Время начала
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Время окончания
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Аудитория/комната
    /// </summary>
    public string? Room { get; set; }

    /// <summary>
    /// Местоположение (алиас для Room для совместимости)
    /// </summary>
    public string? Location
    {
        get => Room;
        set => Room = value;
    }

    /// <summary>
    /// Дата начала действия расписания
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Дата окончания действия расписания
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Дата начала действия (алиас для StartDate для совместимости)
    /// </summary>
    public DateTime ValidFrom
    {
        get => StartDate;
        set => StartDate = value;
    }

    /// <summary>
    /// Дата окончания действия (алиас для EndDate для совместимости)
    /// </summary>
    public DateTime? ValidTo
    {
        get => EndDate;
        set => EndDate = value;
    }

    /// <summary>
    /// Тип занятия
    /// </summary>
    public ScheduleSlotType Type { get; set; } = ScheduleSlotType.Lecture;

    /// <summary>
    /// Активен ли слот расписания
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Заметки к слоту расписания
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Максимальное количество студентов
    /// </summary>
    public int? MaxStudents { get; set; }

    /// <summary>
    /// Экземпляр курса
    /// </summary>
    public CourseInstance? CourseInstance { get; set; }

    /// <summary>
    /// ID курса (для совместимости с ViewModels)
    /// </summary>
    public Guid? CourseUid { get; set; }

    /// <summary>
    /// ID преподавателя (для совместимости с ViewModels)
    /// </summary>
    public Guid? TeacherUid { get; set; }

    /// <summary>
    /// ID группы (вычисляемое свойство)
    /// </summary>
    public Guid? GroupUid => CourseInstance?.GroupUid;

    /// <summary>
    /// Курс (делегирует к CourseInstance.Subject)
    /// </summary>
    public Subject? Course => CourseInstance?.Subject;

    /// <summary>
    /// Преподаватель (делегирует к CourseInstance.Teacher)
    /// </summary>
    public Teacher? Teacher => CourseInstance?.Teacher;

    /// <summary>
    /// Группа (делегирует к CourseInstance.Group)
    /// </summary>
    public Group? Group => CourseInstance?.Group;
}
