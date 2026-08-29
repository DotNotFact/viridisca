using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education;

namespace ViridiscaUi.Domain.Entities.Analytics;

/// <summary>
/// Аналитика теста/викторины
/// </summary>
public class QuizAnalytics : AuditableEntity
{
    /// <summary>
    /// Идентификатор теста
    /// </summary>
    public Guid QuizUid { get; set; }

    // === STORED FIELDS (обновляются периодически) ===

    /// <summary>
    /// Общее количество попыток прохождения теста
    /// </summary>
    public int TotalAttempts { get; set; }

    /// <summary>
    /// Количество завершенных попыток
    /// </summary>
    public int CompletedAttempts { get; set; }

    /// <summary>
    /// Количество успешных попыток (прошли минимальный балл)
    /// </summary>
    public int PassedAttempts { get; set; }

    /// <summary>
    /// Процент успешного прохождения
    /// </summary>
    public decimal PassRate { get; set; }

    /// <summary>
    /// Средний балл по тесту
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
    /// Среднее время прохождения теста (в минутах)
    /// </summary>
    public decimal AverageCompletionTime { get; set; }

    /// <summary>
    /// Количество уникальных участников
    /// </summary>
    public int UniqueParticipants { get; set; }

    /// <summary>
    /// Дата последнего обновления аналитики
    /// </summary>
    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;

    // === NAVIGATION PROPERTIES ===

    /// <summary>
    /// Тест
    /// </summary>
    public Quiz? Quiz { get; set; }

    // === COMPUTED PROPERTIES (автоматически вычисляются через связи) ===

    /// <summary>
    /// Общее количество попыток - вычисляется из связей
    /// </summary>
    public int AttemptsCount => Quiz?.Attempts?.Count ?? 0;

    /// <summary>
    /// Количество завершенных попыток - вычисляется из попыток
    /// </summary>
    public int CalculatedCompletedAttempts => Quiz?.Attempts?
        .Count(a => a.Status == Education.Enums.QuizAttemptStatus.Completed) ?? 0;

    /// <summary>
    /// Количество успешных попыток - вычисляется из попыток
    /// </summary>
    public int CalculatedPassedAttempts => Quiz?.Attempts?
        .Count(a => a.Status == Education.Enums.QuizAttemptStatus.Completed && a.IsPassed == true) ?? 0;

    /// <summary>
    /// Процент успешного прохождения - вычисляется из попыток
    /// </summary>
    public decimal CalculatedPassRate
    {
        get
        {
            var completedCount = CalculatedCompletedAttempts;
            if (completedCount == 0) return 0m;

            var passedCount = CalculatedPassedAttempts;
            return (decimal)passedCount / completedCount * 100;
        }
    }

    /// <summary>
    /// Средний балл - вычисляется из завершенных попыток
    /// </summary>
    public decimal CalculatedAverageScore
    {
        get
        {
            var completedAttempts = Quiz?.Attempts?
                .Where(a => a.Status == Education.Enums.QuizAttemptStatus.Completed && a.Percentage.HasValue)
                .ToList();

            if (completedAttempts?.Any() != true) return 0m;

            return (decimal)completedAttempts.Average(a => a.Percentage!.Value);
        }
    }

    /// <summary>
    /// Максимальный балл - вычисляется из попыток
    /// </summary>
    public decimal CalculatedHighestScore => Quiz?.Attempts?
        .Where(a => a.Status == Education.Enums.QuizAttemptStatus.Completed && a.Percentage.HasValue)
        .Max(a => (decimal)a.Percentage!.Value) ?? 0m;

    /// <summary>
    /// Минимальный балл - вычисляется из попыток
    /// </summary>
    public decimal CalculatedLowestScore => Quiz?.Attempts?
        .Where(a => a.Status == Education.Enums.QuizAttemptStatus.Completed && a.Percentage.HasValue)
        .Min(a => (decimal)a.Percentage!.Value) ?? 0m;

    /// <summary>
    /// Среднее время прохождения - вычисляется из завершенных попыток
    /// </summary>
    public decimal CalculatedAverageCompletionTime
    {
        get
        {
            var completedAttempts = Quiz?.Attempts?
                .Where(a => a.Status == Education.Enums.QuizAttemptStatus.Completed && a.TimeSpent.HasValue)
                .ToList();

            if (completedAttempts?.Any() != true) return 0m;

            return (decimal)completedAttempts.Average(a => a.TimeSpent!.Value);
        }
    }

    /// <summary>
    /// Количество уникальных участников - вычисляется из попыток
    /// </summary>
    public int CalculatedUniqueParticipants => Quiz?.Attempts?
        .Select(a => a.StudentUid)
        .Distinct()
        .Count() ?? 0;

    /// <summary>
    /// Обновляет кэшированные поля аналитики
    /// </summary>
    public void RefreshCalculatedFields()
    {
        TotalAttempts = AttemptsCount;
        CompletedAttempts = CalculatedCompletedAttempts;
        PassedAttempts = CalculatedPassedAttempts;
        PassRate = CalculatedPassRate;
        AverageScore = CalculatedAverageScore;
        HighestScore = CalculatedHighestScore;
        LowestScore = CalculatedLowestScore;
        AverageCompletionTime = CalculatedAverageCompletionTime;
        UniqueParticipants = CalculatedUniqueParticipants;

        LastCalculated = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
} 