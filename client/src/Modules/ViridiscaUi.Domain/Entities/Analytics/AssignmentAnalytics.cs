using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ViridiscaUi.Domain.Entities.Analytics;

/// <summary>
/// Аналитика задания
/// </summary>
public class AssignmentAnalytics : AuditableEntity
{
    /// <summary>
    /// Идентификатор задания
    /// </summary>
    public Guid AssignmentUid { get; set; }

    /// <summary>
    /// Время последнего обновления статистики
    /// </summary>
    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Распределение оценок - может кэшироваться для производительности
    /// Не сохраняется в БД, вычисляется динамически
    /// </summary>
    [NotMapped]
    public Dictionary<string, int> ScoreDistribution { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Задание
    /// </summary>
    public Assignment? Assignment { get; set; }

    /// <summary>
    /// Общее количество студентов - вычисляется из enrollments курса
    /// </summary>
    public int TotalStudents => Assignment?.CourseInstance?.Enrollments?
        .Count(e => e.Status == EnrollmentStatus.Enrolled) ?? 0;

    /// <summary>
    /// Количество сданных работ - вычисляется из submissions
    /// </summary>
    public int TotalSubmissions => Assignment?.Submissions?.Count ?? 0;

    /// <summary>
    /// Количество работ на проверке - вычисляется из pending submissions
    /// </summary>
    public int PendingSubmissions => Assignment?.Submissions?
        .Count(s => s.Status == SubmissionStatus.Submitted) ?? 0;

    /// <summary>
    /// Количество оцененных работ - вычисляется из graded submissions
    /// </summary>
    public int GradedSubmissions => Assignment?.Submissions?
        .Count(s => s.Status == SubmissionStatus.Graded) ?? 0;

    /// <summary>
    /// Количество просроченных работ - вычисляется из late submissions
    /// </summary>
    public int LateSubmissions
    {
        get
        {
            if (Assignment?.DueDate == null)
                return 0;

            return Assignment.Submissions?
                .Count(s => s.SubmittedAt > Assignment.DueDate) ?? 0;
        }
    }

    /// <summary>
    /// Количество не сданных работ - вычисляется как разность
    /// </summary>
    public int NotSubmitted => Math.Max(0, TotalStudents - TotalSubmissions);

    /// <summary>
    /// Средняя оценка - вычисляется из оценок по данному заданию (требует доступ к DbContext)
    /// Это свойство должно заполняться в сервисе через отдельный запрос
    /// </summary>
    public decimal AverageGrade { get; set; }

    /// <summary>
    /// Максимальная оценка - вычисляется из оценок по данному заданию
    /// Это свойство должно заполняться в сервисе через отдельный запрос
    /// </summary>
    public decimal MaxGrade { get; set; }

    /// <summary>
    /// Минимальная оценка - вычисляется из оценок по данному заданию
    /// Это свойство должно заполняться в сервисе через отдельный запрос
    /// </summary>
    public decimal MinGrade { get; set; }

    /// <summary>
    /// Процент выполнения - отношение сданных к общему количеству студентов
    /// </summary>
    public decimal CompletionRate
    {
        get
        {
            if (TotalStudents == 0)
                return 0m;
            
            return (decimal)TotalSubmissions / TotalStudents * 100;
        }
    }

    /// <summary>
    /// Процент сдачи заданий - отношение сданных к общему количеству
    /// </summary>
    public decimal SubmissionRate => CompletionRate; // Синоним для CompletionRate

    /// <summary>
    /// Среднее время выполнения задания - вычисляется из submissions
    /// </summary>
    public TimeSpan? AverageSubmissionTime
    {
        get
        {
            if (Assignment?.CreatedAt == null)
                return null;

            var submissionTimes = Assignment.Submissions?
                .Where(s => s.SubmittedAt.HasValue)
                .Select(s => s.SubmittedAt!.Value - Assignment.CreatedAt)
                .ToList();

            if (submissionTimes?.Any() != true)
                return null;

            var averageTicks = (long)submissionTimes.Average(ts => ts.Ticks);
            return new TimeSpan(averageTicks);
        }
    }

    /// <summary>
    /// Дата первой сдачи - вычисляется из submissions
    /// </summary>
    public DateTime? FirstSubmissionDate => Assignment?.Submissions?
        .Where(s => s.SubmittedAt.HasValue)
        .Min(s => s.SubmittedAt);

    /// <summary>
    /// Дата последней сдачи - вычисляется из submissions
    /// </summary>
    public DateTime? LastSubmissionDate => Assignment?.Submissions?
        .Where(s => s.SubmittedAt.HasValue)
        .Max(s => s.SubmittedAt);

    /// <summary>
    /// Статус задания
    /// </summary>
    public AssignmentStatus Status => Assignment?.Status ?? AssignmentStatus.Draft;

    /// <summary>
    /// Проверяет, активно ли задание
    /// </summary>
    public bool IsActive => Status == AssignmentStatus.Published;

    /// <summary>
    /// Проверяет, просрочено ли задание
    /// </summary>
    public bool IsOverdue => Assignment?.DueDate.HasValue == true && Assignment.DueDate < DateTime.UtcNow;

    /// <summary>
    /// Процент просроченных сдач
    /// </summary>
    public decimal LateSubmissionRate
    {
        get
        {
            if (TotalSubmissions == 0)
                return 0m;
            
            return (decimal)LateSubmissions / TotalSubmissions * 100;
        }
    }

    /// <summary>
    /// Процент оцененных работ
    /// </summary>
    public decimal GradingProgress
    {
        get
        {
            if (TotalSubmissions == 0)
                return 0m;
            
            return (decimal)GradedSubmissions / TotalSubmissions * 100;
        }
    }

    /// <summary>
    /// Общий рейтинг задания (комбинированный показатель)
    /// </summary>
    public decimal OverallRating
    {
        get
        {
            // Простая формула рейтинга
            var completionScore = CompletionRate; // 0-100
            var gradeScore = AverageGrade / 5m * 100; // Нормализуем к 100
            var timelinessScore = 100 - LateSubmissionRate; // Инвертируем просрочки

            // Взвешенная средняя
            return completionScore * 0.4m + gradeScore * 0.4m + timelinessScore * 0.2m;
        }
    }

    /// <summary>
    /// Принудительно обновляет кэшированные значения
    /// </summary>
    public void RefreshCalculatedFields()
    {
        LastCalculated = DateTime.UtcNow;
        
        // Обновляем распределение оценок
        RefreshScoreDistribution();
    }

    /// <summary>
    /// Обновляет распределение оценок
    /// </summary>
    private void RefreshScoreDistribution()
    {
        ScoreDistribution.Clear();
        
        // Примечание: Для получения оценок нужен доступ к DbContext
        // Эта логика должна быть реализована в сервисе
    }

    /// <summary>
    /// Получает диапазон оценки для группировки
    /// </summary>
    private string GetGradeRange(double grade)
    {
        return grade switch
        {
            >= 4.5 => "Отлично (4.5-5.0)",
            >= 3.5 => "Хорошо (3.5-4.4)",
            >= 2.5 => "Удовлетворительно (2.5-3.4)",
            _ => "Неудовлетворительно (0-2.4)"
        };
    }

    /// <summary>
    /// Проверяет, нужно ли обновить статистику
    /// </summary>
    public bool ShouldRecalculate(TimeSpan maxAge)
    {
        return DateTime.UtcNow - LastCalculated > maxAge;
    }
} 