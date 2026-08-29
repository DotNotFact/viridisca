using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Analytics;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Тест/викторина
/// </summary>
public class Quiz : AuditableEntity
{
    /// <summary>
    /// Название теста
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание теста
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Инструкции для прохождения теста
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Идентификатор экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid { get; set; }

    /// <summary>
    /// Идентификатор урока (опционально)
    /// </summary>
    public Guid? LessonUid { get; set; }

    /// <summary>
    /// Время на прохождение теста (в минутах)
    /// </summary>
    public int? TimeLimit { get; set; }

    /// <summary>
    /// Максимальное количество попыток
    /// </summary>
    public int MaxAttempts { get; set; } = 1;

    /// <summary>
    /// Минимальный проходной балл (в процентах)
    /// </summary>
    public double PassingScore { get; set; } = 60.0;

    /// <summary>
    /// Перемешивать ли вопросы
    /// </summary>
    public bool ShuffleQuestions { get; set; } = false;

    /// <summary>
    /// Перемешивать ли ответы
    /// </summary>
    public bool ShuffleAnswers { get; set; } = false;

    /// <summary>
    /// Показывать ли результаты сразу после прохождения
    /// </summary>
    public bool ShowResultsImmediately { get; set; } = true;

    /// <summary>
    /// Показывать ли правильные ответы после прохождения
    /// </summary>
    public bool ShowCorrectAnswers { get; set; } = true;

    /// <summary>
    /// Дата начала доступности теста
    /// </summary>
    public DateTime? AvailableFrom { get; set; }

    /// <summary>
    /// Дата окончания доступности теста
    /// </summary>
    public DateTime? AvailableUntil { get; set; }

    /// <summary>
    /// Активен ли тест
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Опубликован ли тест
    /// </summary>
    public bool IsPublished { get; set; } = false;

    /// <summary>
    /// Экземпляр курса
    /// </summary>
    public CourseInstance? CourseInstance { get; set; }

    /// <summary>
    /// Урок (если тест привязан к уроку)
    /// </summary>
    public Lesson? Lesson { get; set; }

    /// <summary>
    /// Вопросы теста
    /// </summary>
    public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();

    /// <summary>
    /// Попытки прохождения теста
    /// </summary>
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();

    /// <summary>
    /// Аналитические данные теста
    /// </summary>
    public ICollection<QuizAnalytics> Analytics { get; set; } = new List<QuizAnalytics>();
} 