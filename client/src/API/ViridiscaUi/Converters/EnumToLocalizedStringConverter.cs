using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.System.Enums;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для преобразования enum значений в локализованные строки
/// </summary>
public class EnumToLocalizedStringConverter : IValueConverter
{
    /// <summary>
    /// Статический экземпляр конвертера для использования в XAML
    /// </summary>
    public static EnumToLocalizedStringConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;

        return value switch
        {
            // NotificationPriority
            NotificationPriority.Low => "Низкий",
            NotificationPriority.Normal => "Обычный", 
            NotificationPriority.High => "Высокий",
            NotificationPriority.Critical => "Критический",
            
            // NotificationType
            NotificationType.Info => "Информация",
            NotificationType.Warning => "Предупреждение",
            NotificationType.Error => "Ошибка",
            NotificationType.Success => "Успех",
            
            // RoleType (instead of UserRole)
            RoleType.Student => "Студент",
            RoleType.Teacher => "Преподаватель",
            RoleType.SystemAdmin => "Администратор",
            RoleType.SchoolDirector => "Супер-администратор",
            
            // StudentStatus
            StudentStatus.Active => "Активный",
            StudentStatus.Inactive => "Неактивный",
            StudentStatus.AcademicLeave => "Академический отпуск",
            StudentStatus.Expelled => "Отчислен",
            StudentStatus.Graduated => "Выпущен",
            StudentStatus.Transferred => "Переведен",
            StudentStatus.Suspended => "Приостановлен",
            
            // TeacherStatus
            TeacherStatus.Active => "Активный",
            TeacherStatus.Inactive => "Неактивный",
            TeacherStatus.OnLeave => "В отпуске",
            TeacherStatus.Terminated => "Уволен",
            TeacherStatus.Retired => "На пенсии",
            TeacherStatus.Suspended => "Отстранен",
            
            // CourseStatus
            CourseStatus.Draft => "Черновик",
            CourseStatus.Active => "Активный",
            CourseStatus.Published => "Опубликован",
            CourseStatus.Completed => "Завершен",
            CourseStatus.Archived => "Архивный",
            CourseStatus.Suspended => "Приостановлен",
            
            // GroupStatus
            GroupStatus.Active => "Активная",
            GroupStatus.Forming => "Формирование",
            GroupStatus.Suspended => "Приостановлена",
            GroupStatus.Completed => "Завершена",
            GroupStatus.Archived => "Архивная",
            
            // SubjectType
            SubjectType.Required => "Обязательный",
            SubjectType.Elective => "Факультативный", 
            SubjectType.Specialized => "Специализированный",
            SubjectType.Practicum => "Практикум",
            SubjectType.Seminar => "Семинар",
            SubjectType.Laboratory => "Лабораторный",
            SubjectType.Lecture => "Лекционный",
            
            // AssignmentType
            AssignmentType.Homework => "Домашнее задание",
            AssignmentType.Test => "Тест",
            AssignmentType.Exam => "Экзамен",
            AssignmentType.Project => "Проект",
            AssignmentType.Essay => "Эссе",
            
            // GradeType
            GradeType.Homework => "Домашнее задание",
            GradeType.Quiz => "Тест/Опрос",
            GradeType.Test => "Контрольная работа",
            GradeType.Exam => "Экзамен",
            GradeType.Project => "Проект",
            GradeType.FinalGrade => "Итоговая оценка",
            
            _ => value.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Обратное преобразование не поддерживается
        return AvaloniaProperty.UnsetValue;
    }
} 