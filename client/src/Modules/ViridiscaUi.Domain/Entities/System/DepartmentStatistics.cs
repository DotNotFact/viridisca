namespace ViridiscaUi.Domain.Entities.System;

/// <summary>
/// Статистика департамента
/// </summary>
public class DepartmentStatistics
{
    /// <summary>
    /// Идентификатор департамента
    /// </summary>
    public Guid DepartmentUid { get; set; }

    /// <summary>
    /// Общее количество предметов
    /// </summary>
    public int TotalSubjects { get; set; }

    /// <summary>
    /// Общее количество преподавателей
    /// </summary>
    public int TotalTeachers { get; set; }

    /// <summary>
    /// Общее количество экземпляров курсов
    /// </summary>
    public int TotalCourseInstances { get; set; }

    /// <summary>
    /// Общее количество студентов
    /// </summary>
    public int TotalStudents { get; set; }

    /// <summary>
    /// Количество активных преподавателей
    /// </summary>
    public int ActiveTeachers { get; set; }

    /// <summary>
    /// Количество активных предметов
    /// </summary>
    public int ActiveSubjects { get; set; }

    /// <summary>
    /// Дата последней активности
    /// </summary>
    public DateTime? LastActivityDate { get; set; }
} 