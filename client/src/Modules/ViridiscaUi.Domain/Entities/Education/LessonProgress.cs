using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education.Enums;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Прогресс студента по уроку
/// </summary>
public class LessonProgress : AuditableEntity
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
    /// Процент завершения урока (0-100)
    /// </summary>
    public decimal CompletionPercentage { get; set; }

    /// <summary>
    /// Время начала изучения урока
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Время завершения урока
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Время, потраченное на урок (в минутах)
    /// </summary>
    public int TimeSpentMinutes { get; set; }

    /// <summary>
    /// Завершен ли урок
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Заметки студента по уроку
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Статус прогресса
    /// </summary>
    public LessonProgressStatus Status { get; set; } = LessonProgressStatus.NotStarted;

    /// <summary>
    /// Оценка занятия студентом (1-5)
    /// </summary>
    public int? Rating { get; set; }

    /// <summary>
    /// Отзыв студента о занятии
    /// </summary>
    public string? Feedback { get; set; }

    // Navigation properties
    /// <summary>
    /// Студент
    /// </summary>
    public Student? Student { get; set; }

    /// <summary>
    /// Урок
    /// </summary>
    public Lesson? Lesson { get; set; }

    /// <summary>
    /// Конструктор по умолчанию
    /// </summary>
    public LessonProgress()
    {
        Uid = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Конструктор с параметрами
    /// </summary>
    public LessonProgress(Guid studentUid, Guid lessonUid) : this()
    {
        StudentUid = studentUid;
        LessonUid = lessonUid;
    }

    /// <summary>
    /// Начать изучение занятия
    /// </summary>
    public void StartLesson()
    {
        if (Status == LessonProgressStatus.NotStarted)
        {
            Status = LessonProgressStatus.InProgress;
            StartedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Завершить изучение занятия
    /// </summary>
    public void CompleteLesson()
    {
        Status = LessonProgressStatus.Completed;
        CompletionPercentage = 100;
        CompletedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Обновить прогресс
    /// </summary>
    public void UpdateProgress(int percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage), "Процент должен быть от 0 до 100");

        CompletionPercentage = percentage;
        
        if (Status == LessonProgressStatus.NotStarted && percentage > 0)
        {
            StartLesson();
        }
        else if (percentage == 100)
        {
            CompleteLesson();
        }
        else
        {
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Добавить время изучения
    /// </summary>
    public void AddStudyTime(int minutes)
    {
        if (minutes > 0)
        {
            TimeSpentMinutes += minutes;
            LastModifiedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Установить оценку и отзыв
    /// </summary>
    public void SetRating(int rating, string? feedback = null)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Оценка должна быть от 1 до 5");

        Rating = rating;
        Feedback = feedback;
        LastModifiedAt = DateTime.UtcNow;
    }
} 