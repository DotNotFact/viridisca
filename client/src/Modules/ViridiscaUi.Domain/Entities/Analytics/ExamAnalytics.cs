using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education;

namespace ViridiscaUi.Domain.Entities.Analytics;

/// <summary>
/// Аналитика экзамена
/// </summary>
public class ExamAnalytics : AuditableEntity
{
    /// <summary>
    /// Идентификатор экзамена
    /// </summary>
    public Guid ExamUid { get; set; }

    // === STORED FIELDS (обновляются периодически) ===

    /// <summary>
    /// Общее количество участников экзамена
    /// </summary>
    public int TotalParticipants { get; set; }

    /// <summary>
    /// Количество сдавших экзамен
    /// </summary>
    public int PassedCount { get; set; }

    /// <summary>
    /// Количество не сдавших экзамен
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Процент успешной сдачи
    /// </summary>
    public decimal PassRate { get; set; }

    /// <summary>
    /// Средний балл по экзамену
    /// </summary>
    public decimal AverageScore { get; set; }

    /// <summary>
    /// Максимальный полученный балл
    /// </summary>
    public decimal HighestScore { get; set; }

    /// <summary>
    /// Минимальный полученный балл
    /// </summary>
    public decimal LowestScore { get; set; }

    /// <summary>
    /// Медианный балл
    /// </summary>
    public decimal MedianScore { get; set; }

    /// <summary>
    /// Дата последнего обновления аналитики
    /// </summary>
    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;

    // === NAVIGATION PROPERTIES ===

    /// <summary>
    /// Экзамен
    /// </summary>
    public Exam? Exam { get; set; }

    // === COMPUTED PROPERTIES (автоматически вычисляются через связи) ===

    /// <summary>
    /// Общее количество результатов экзамена
    /// </summary>
    public int ResultsCount => Exam?.Results?.Count ?? 0;

    /// <summary>
    /// Количество сдавших - вычисляется из результатов
    /// </summary>
    public int CalculatedPassedCount => Exam?.Results?
        .Count(r => !r.IsAbsent && r.Percentage >= 60) ?? 0; // Assuming 60% is passing

    /// <summary>
    /// Количество не сдавших - вычисляется из результатов
    /// </summary>
    public int CalculatedFailedCount => Exam?.Results?
        .Count(r => r.IsAbsent || r.Percentage < 60) ?? 0; // Assuming 60% is passing

    /// <summary>
    /// Процент успешной сдачи - вычисляется из результатов
    /// </summary>
    public decimal CalculatedPassRate
    {
        get
        {
            var totalResults = ResultsCount;
            if (totalResults == 0) return 0m;

            var passedCount = CalculatedPassedCount;
            return (decimal)passedCount / totalResults * 100;
        }
    }

    /// <summary>
    /// Средний балл - вычисляется из результатов
    /// </summary>
    public decimal CalculatedAverageScore
    {
        get
        {
            var scores = Exam?.Results?.Select(r => r.Score).ToList();
            if (scores?.Any() != true) return 0m;

            return scores.Average();
        }
    }

    /// <summary>
    /// Максимальный балл - вычисляется из результатов
    /// </summary>
    public decimal CalculatedHighestScore => Exam?.Results?.Max(r => r.Score) ?? 0m;

    /// <summary>
    /// Минимальный балл - вычисляется из результатов
    /// </summary>
    public decimal CalculatedLowestScore => Exam?.Results?.Min(r => r.Score) ?? 0m;

    /// <summary>
    /// Медианный балл - вычисляется из результатов
    /// </summary>
    public decimal CalculatedMedianScore
    {
        get
        {
            var scores = Exam?.Results?.Select(r => r.Score).OrderBy(s => s).ToList();
            if (scores?.Any() != true) return 0m;

            var count = scores.Count;
            if (count % 2 == 0)
            {
                return (scores[count / 2 - 1] + scores[count / 2]) / 2;
            }
            else
            {
                return scores[count / 2];
            }
        }
    }

    /// <summary>
    /// Обновляет кэшированные поля аналитики
    /// </summary>
    public void RefreshCalculatedFields()
    {
        TotalParticipants = ResultsCount;
        PassedCount = CalculatedPassedCount;
        FailedCount = CalculatedFailedCount;
        PassRate = CalculatedPassRate;
        AverageScore = CalculatedAverageScore;
        HighestScore = CalculatedHighestScore;
        LowestScore = CalculatedLowestScore;
        MedianScore = CalculatedMedianScore;

        LastCalculated = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
} 