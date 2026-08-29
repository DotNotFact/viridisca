using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Типы слотов расписания
/// </summary>
public enum ScheduleSlotType
{
    /// <summary>
    /// Лекция
    /// </summary>
    [Description("Лекция")]
    Lecture,
    
    /// <summary>
    /// Семинар
    /// </summary>
    [Description("Семинар")]
    Seminar,
    
    /// <summary>
    /// Лабораторная работа
    /// </summary>
    [Description("Лабораторная работа")]
    Laboratory,
    
    /// <summary>
    /// Практическое занятие
    /// </summary>
    [Description("Практическое занятие")]
    Practice,
    
    /// <summary>
    /// Консультация
    /// </summary>
    [Description("Консультация")]
    Consultation,
    
    /// <summary>
    /// Экзамен
    /// </summary>
    [Description("Экзамен")]
    Exam,
    
    /// <summary>
    /// Зачет
    /// </summary>
    [Description("Зачет")]
    Test,
    
    /// <summary>
    /// Самостоятельная работа
    /// </summary>
    [Description("Самостоятельная работа")]
    SelfStudy
} 