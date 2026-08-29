using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education.Enums;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Попытка прохождения теста студентом
/// </summary>
public class QuizAttempt : AuditableEntity
{
    /// <summary>
    /// Идентификатор теста
    /// </summary>
    public Guid QuizUid { get; set; }

    /// <summary>
    /// Идентификатор студента
    /// </summary>
    public Guid StudentUid { get; set; }

    /// <summary>
    /// Номер попытки
    /// </summary>
    public int AttemptNumber { get; set; }

    /// <summary>
    /// Дата начала прохождения
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Дата завершения прохождения
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Статус попытки
    /// </summary>
    public QuizAttemptStatus Status { get; set; } = QuizAttemptStatus.InProgress;

    /// <summary>
    /// Итоговый балл
    /// </summary>
    public double? Score { get; set; }

    /// <summary>
    /// Максимально возможный балл
    /// </summary>
    public double MaxScore { get; set; }

    /// <summary>
    /// Процент правильных ответов
    /// </summary>
    public double? Percentage { get; set; }

    /// <summary>
    /// Пройден ли тест (набран минимальный балл)
    /// </summary>
    public bool? IsPassed { get; set; }

    /// <summary>
    /// Время, потраченное на прохождение (в минутах)
    /// </summary>
    public int? TimeSpent { get; set; }

    /// <summary>
    /// Тест
    /// </summary>
    public Quiz? Quiz { get; set; }

    /// <summary>
    /// Студент
    /// </summary>
    public Student? Student { get; set; }

    /// <summary>
    /// Ответы студента на вопросы
    /// </summary>
    public ICollection<QuizStudentAnswer> StudentAnswers { get; set; } = new List<QuizStudentAnswer>();
} 