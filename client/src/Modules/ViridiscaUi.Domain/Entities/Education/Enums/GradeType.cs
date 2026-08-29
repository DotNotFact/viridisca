using System.ComponentModel;

namespace ViridiscaUi.Domain.Entities.Education.Enums;

/// <summary>
/// Типы оценок
/// </summary>
public enum GradeType
{
    /// <summary>
    /// Домашнее задание
    /// </summary>
    [Description("Домашнее задание")]
    Homework,

    /// <summary>
    /// Задание
    /// </summary>
    [Description("Задание")]
    Assignment,

    /// <summary>
    /// Тест/Опрос
    /// </summary>
    [Description("Тест/Опрос")]
    Quiz,

    /// <summary>
    /// Контрольная работа
    /// </summary>
    [Description("Контрольная работа")]
    Test,

    /// <summary>
    /// Экзамен
    /// </summary>
    [Description("Экзамен")]
    Exam,

    /// <summary>
    /// Проект
    /// </summary>
    [Description("Проект")]
    Project,

    /// <summary>
    /// Участие
    /// </summary>
    [Description("Участие")]
    Participation,

    /// <summary>
    /// Итоговая оценка
    /// </summary>
    [Description("Итоговая оценка")]
    FinalGrade,

    /// <summary>
    /// Другое
    /// </summary>
    [Description("Другое")]
    Other
}

/// <summary>
/// Расширения для типов оценок
/// </summary>
public static class GradeTypeExtensions
{
    /// <summary>
    /// Получает отображаемое имя типа оценки
    /// </summary>
    public static string GetDisplayName(this GradeType gradeType)
    {
        var fieldInfo = gradeType.GetType().GetField(gradeType.ToString());
        if (fieldInfo == null) return gradeType.ToString();

        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : gradeType.ToString();
    }
} 