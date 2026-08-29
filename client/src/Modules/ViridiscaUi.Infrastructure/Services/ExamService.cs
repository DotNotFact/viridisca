using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.System.Enums;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Infrastructure.Data;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для работы с экзаменами
/// EF-backed сервис, следует архитектурному паттерну AssignmentService
/// </summary>
public class ExamService : IExamService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ExamService> _logger;

    public ExamService(
        ApplicationDbContext dbContext,
        INotificationService notificationService,
        ILogger<ExamService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Базовые CRUD операции

    /// <summary>
    /// Получает все экзамены
    /// </summary>
    public async Task<IEnumerable<Exam>> GetAllAsync()
    {
        try
        {
            return await _dbContext.Exams
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Subject)
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Group)
                .Include(e => e.AcademicPeriod)
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.ExamDate)
                .ThenBy(e => e.Title)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all exams");
            throw;
        }
    }

    /// <summary>
    /// Получает экзамен по идентификатору
    /// </summary>
    public async Task<Exam?> GetByUidAsync(Guid uid)
    {
        try
        {
            return await _dbContext.Exams
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Subject)
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Teacher)
                        .ThenInclude(t => t!.Person)
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Group)
                .Include(e => e.AcademicPeriod)
                .Include(e => e.Results)
                .FirstOrDefaultAsync(e => e.Uid == uid && !e.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exam {ExamUid}", uid);
            throw;
        }
    }

    /// <summary>
    /// Получает экзамен по идентификатору (алиас для GetByUidAsync)
    /// </summary>
    public Task<Exam?> GetByIdAsync(Guid uid) => GetByUidAsync(uid);

    /// <summary>
    /// Получает экзамены для экземпляра курса
    /// </summary>
    public async Task<IEnumerable<Exam>> GetByCourseInstanceAsync(Guid courseInstanceUid)
    {
        try
        {
            return await _dbContext.Exams
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Subject)
                .Include(e => e.Results)
                .Where(e => e.CourseInstanceUid == courseInstanceUid && !e.IsDeleted)
                .OrderBy(e => e.ExamDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exams for course instance {CourseInstanceUid}", courseInstanceUid);
            throw;
        }
    }

    /// <summary>
    /// Получает экзамены для академического периода
    /// </summary>
    public async Task<IEnumerable<Exam>> GetByAcademicPeriodAsync(Guid academicPeriodUid)
    {
        try
        {
            return await _dbContext.Exams
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Subject)
                .Where(e => e.AcademicPeriodUid == academicPeriodUid && !e.IsDeleted)
                .OrderBy(e => e.ExamDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exams for academic period {AcademicPeriodUid}", academicPeriodUid);
            throw;
        }
    }

    /// <summary>
    /// Создает новый экзамен
    /// </summary>
    public async Task<Exam> CreateAsync(Exam exam)
    {
        ArgumentNullException.ThrowIfNull(exam);

        try
        {
            await ValidateExamAsync(exam);

            exam.Uid = Guid.NewGuid();
            exam.CreatedAt = DateTime.UtcNow;
            exam.LastModifiedAt = DateTime.UtcNow;

            _dbContext.Exams.Add(exam);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created exam {ExamTitle} for course instance {CourseInstanceUid}",
                exam.Title, exam.CourseInstanceUid);
            return exam;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exam {ExamTitle}", exam.Title);
            throw;
        }
    }

    /// <summary>
    /// Обновляет экзамен
    /// </summary>
    public async Task<Exam> UpdateAsync(Exam exam)
    {
        ArgumentNullException.ThrowIfNull(exam);

        try
        {
            exam.LastModifiedAt = DateTime.UtcNow;

            _dbContext.Exams.Update(exam);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated exam {ExamTitle} with ID {ExamUid}", exam.Title, exam.Uid);
            return exam;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating exam {ExamUid}", exam.Uid);
            throw;
        }
    }

    /// <summary>
    /// Удаляет экзамен (мягкое удаление)
    /// </summary>
    public async Task<bool> DeleteAsync(Guid uid)
    {
        try
        {
            var exam = await _dbContext.Exams.FindAsync(uid);
            if (exam == null || exam.IsDeleted)
                return false;

            exam.IsDeleted = true;
            exam.DeletedAt = DateTime.UtcNow;
            exam.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted exam {ExamUid}", uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting exam {ExamUid}", uid);
            throw;
        }
    }

    /// <summary>
    /// Проверяет существование экзамена
    /// </summary>
    public async Task<bool> ExistsAsync(Guid uid)
    {
        try
        {
            return await _dbContext.Exams.AnyAsync(e => e.Uid == uid && !e.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if exam exists {ExamUid}", uid);
            throw;
        }
    }

    /// <summary>
    /// Получает количество экзаменов
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        try
        {
            return await _dbContext.Exams.CountAsync(e => !e.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exams count");
            throw;
        }
    }

    #endregion

    #region Специфичные методы для экзаменов

    /// <summary>
    /// Получает предстоящие экзамены
    /// </summary>
    public async Task<IEnumerable<Exam>> GetUpcomingAsync()
    {
        try
        {
            return await _dbContext.Exams
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Subject)
                .Where(e => !e.IsDeleted && e.ExamDate > DateTime.UtcNow)
                .OrderBy(e => e.ExamDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming exams");
            throw;
        }
    }

    /// <summary>
    /// Получает завершенные экзамены
    /// </summary>
    public async Task<IEnumerable<Exam>> GetCompletedAsync()
    {
        try
        {
            return await _dbContext.Exams
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Subject)
                .Where(e => !e.IsDeleted && e.ExamDate <= DateTime.UtcNow)
                .OrderByDescending(e => e.ExamDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completed exams");
            throw;
        }
    }

    /// <summary>
    /// Получает экзамены, конфликтующие по времени/группе/месту с заданным экзаменом
    /// </summary>
    public async Task<IEnumerable<Exam>> GetConflictingExamsAsync(Exam exam)
    {
        ArgumentNullException.ThrowIfNull(exam);

        return await GetConflictingExamsAsync(exam.ExamDate, exam.ExamDate.Add(exam.Duration), exam.Uid);
    }

    /// <summary>
    /// Получает экзамены, конфликтующие с заданным временным интервалом
    /// (по совпадению группы курса или места проведения)
    /// </summary>
    public async Task<IEnumerable<Exam>> GetConflictingExamsAsync(DateTime startTime, DateTime endTime, Guid? excludeExamUid = null)
    {
        try
        {
            var query = _dbContext.Exams
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Subject)
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Group)
                .Where(e => !e.IsDeleted &&
                            e.ExamDate < endTime &&
                            e.ExamDate.Add(e.Duration) > startTime);

            if (excludeExamUid.HasValue)
            {
                query = query.Where(e => e.Uid != excludeExamUid.Value);
            }

            var overlapping = await query.ToListAsync();

            if (!excludeExamUid.HasValue)
            {
                return overlapping;
            }

            // Ограничиваем конфликты теми, что затрагивают ту же группу или то же место проведения,
            // что и исключаемый (проверяемый) экзамен
            var referenceExam = await _dbContext.Exams
                .Include(e => e.CourseInstance)
                .FirstOrDefaultAsync(e => e.Uid == excludeExamUid.Value);

            if (referenceExam?.CourseInstance == null)
            {
                return overlapping;
            }

            return overlapping.Where(e =>
                (e.CourseInstance != null && e.CourseInstance.GroupUid == referenceExam.CourseInstance.GroupUid) ||
                (!string.IsNullOrWhiteSpace(e.Location) && e.Location == referenceExam.Location));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conflicting exams for {StartTime} - {EndTime}", startTime, endTime);
            throw;
        }
    }

    /// <summary>
    /// Получает количество результатов экзамена
    /// </summary>
    public async Task<int> GetResultsCountAsync(Guid examUid)
    {
        try
        {
            return await _dbContext.ExamResults
                .CountAsync(r => r.ExamUid == examUid && !r.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting results count for exam {ExamUid}", examUid);
            throw;
        }
    }

    /// <summary>
    /// Публикует экзамен
    /// </summary>
    public async Task<Exam> PublishExamAsync(Guid examUid)
    {
        try
        {
            var exam = await _dbContext.Exams.FindAsync(examUid);
            if (exam == null || exam.IsDeleted)
                throw new ArgumentException($"Exam with UID {examUid} not found");

            exam.IsPublished = true;
            exam.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Published exam {ExamUid}", examUid);
            return exam;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing exam {ExamUid}", examUid);
            throw;
        }
    }

    /// <summary>
    /// Получает результаты экзамена
    /// </summary>
    public async Task<IEnumerable<object>> GetExamResultsAsync(Guid examUid)
    {
        try
        {
            var results = await _dbContext.ExamResults
                .Include(r => r.Student)
                    .ThenInclude(s => s!.Person)
                .Include(r => r.Exam)
                .Where(r => r.ExamUid == examUid && !r.IsDeleted)
                .OrderByDescending(r => r.Score)
                .ToListAsync();

            return results.Select(r => new
            {
                r.Uid,
                r.ExamUid,
                r.StudentUid,
                StudentName = r.Student?.Person != null
                    ? $"{r.Student.Person.LastName} {r.Student.Person.FirstName}".Trim()
                    : string.Empty,
                r.Score,
                MaxScore = r.Exam?.MaxScore ?? 0,
                r.Percentage,
                r.IsAbsent,
                r.Feedback,
                r.SubmittedAt,
                r.GradedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exam results for exam {ExamUid}", examUid);
            throw;
        }
    }

    /// <summary>
    /// Отправляет уведомления об экзамене студентам, записанным на соответствующий курс
    /// </summary>
    public async Task SendExamNotificationAsync(Guid examUid, string message)
    {
        try
        {
            var exam = await _dbContext.Exams
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Enrollments)
                        .ThenInclude(en => en.Student)
                .FirstOrDefaultAsync(e => e.Uid == examUid && !e.IsDeleted);

            if (exam?.CourseInstance == null)
            {
                _logger.LogWarning("SendExamNotificationAsync: exam {ExamUid} not found or has no course instance", examUid);
                return;
            }

            var students = exam.CourseInstance.Enrollments
                .Where(en => !en.IsDeleted)
                .Select(en => en.Student)
                .Where(s => s != null)
                .ToList();

            foreach (var student in students)
            {
                await _notificationService.CreateNotificationAsync(
                    student!.PersonUid,
                    $"Экзамен: {exam.Title}",
                    message,
                    NotificationType.Info,
                    NotificationPriority.Normal);
            }

            _logger.LogInformation("Sent exam notification for {ExamUid} to {StudentCount} students", examUid, students.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending exam notification for exam {ExamUid}", examUid);
            throw;
        }
    }

    /// <summary>
    /// Получает общую статистику по всем экзаменам
    /// </summary>
    public async Task<object> GetExamStatisticsAsync()
    {
        try
        {
            var exams = await _dbContext.Exams
                .Where(e => !e.IsDeleted)
                .ToListAsync();

            var totalExams = exams.Count;
            var publishedExams = exams.Count(e => e.IsPublished);
            var upcomingExams = exams.Count(e => e.ExamDate > DateTime.UtcNow);
            var completedExams = exams.Count(e => e.ExamDate <= DateTime.UtcNow);

            var results = await _dbContext.ExamResults
                .Where(r => !r.IsDeleted && !r.IsAbsent)
                .ToListAsync();

            var averageScore = results.Count > 0 ? (double)results.Average(r => r.Percentage) : 0;
            var passRate = results.Count > 0 ? (double)results.Count(r => r.Percentage >= 60) / results.Count * 100 : 0;

            return new
            {
                TotalExams = totalExams,
                PublishedExams = publishedExams,
                UpcomingExams = upcomingExams,
                CompletedExams = completedExams,
                AverageScore = averageScore,
                PassRate = passRate,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exam statistics");
            throw;
        }
    }

    /// <summary>
    /// Получает статистику конкретного экзамена
    /// </summary>
    public async Task<object> GetExamStatisticsAsync(Guid examUid)
    {
        try
        {
            var exam = await _dbContext.Exams
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Enrollments)
                .FirstOrDefaultAsync(e => e.Uid == examUid && !e.IsDeleted);

            if (exam == null)
                throw new ArgumentException($"Exam with UID {examUid} not found");

            var results = await _dbContext.ExamResults
                .Where(r => r.ExamUid == examUid && !r.IsDeleted)
                .ToListAsync();

            var enrolledCount = exam.CourseInstance?.Enrollments.Count(en => !en.IsDeleted) ?? 0;
            var presentResults = results.Where(r => !r.IsAbsent).ToList();
            var absentCount = results.Count(r => r.IsAbsent);

            var averageScore = presentResults.Count > 0 ? (double)presentResults.Average(r => r.Score) : 0;
            var averagePercentage = presentResults.Count > 0 ? (double)presentResults.Average(r => r.Percentage) : 0;
            var passRate = presentResults.Count > 0
                ? (double)presentResults.Count(r => r.Percentage >= 60) / presentResults.Count * 100
                : 0;

            return new
            {
                ExamUid = examUid,
                exam.Title,
                EnrolledCount = enrolledCount,
                ResultsCount = results.Count,
                AbsentCount = absentCount,
                AverageScore = averageScore,
                AveragePercentage = averagePercentage,
                PassRate = passRate,
                HighestScore = presentResults.Count > 0 ? presentResults.Max(r => r.Score) : 0,
                LowestScore = presentResults.Count > 0 ? presentResults.Min(r => r.Score) : 0,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exam statistics for {ExamUid}", examUid);
            throw;
        }
    }

    /// <summary>
    /// Получает экзамены с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Exam> exams, int totalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null)
    {
        try
        {
            var query = _dbContext.Exams
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Subject)
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci!.Group)
                .Include(e => e.AcademicPeriod)
                .Where(e => !e.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(e =>
                    e.Title.ToLower().Contains(lowerSearchTerm) ||
                    (e.Description != null && e.Description.ToLower().Contains(lowerSearchTerm)) ||
                    (e.Location != null && e.Location.ToLower().Contains(lowerSearchTerm)));
            }

            var totalCount = await query.CountAsync();

            var exams = await query
                .OrderBy(e => e.ExamDate)
                .ThenBy(e => e.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (exams, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged exams");
            throw;
        }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Валидация экзамена
    /// </summary>
    private async Task ValidateExamAsync(Exam exam)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(exam.Title))
            errors.Add("Название экзамена обязательно для заполнения");

        if (exam.CourseInstanceUid == Guid.Empty)
            errors.Add("Экземпляр курса обязателен для экзамена");

        if (exam.AcademicPeriodUid == Guid.Empty)
            errors.Add("Академический период обязателен для экзамена");

        if (exam.MaxScore <= 0)
            errors.Add("Максимальный балл должен быть больше нуля");

        var courseInstanceExists = await _dbContext.CourseInstances
            .AnyAsync(ci => ci.Uid == exam.CourseInstanceUid && !ci.IsDeleted);

        if (!courseInstanceExists)
            errors.Add($"Экземпляр курса с Uid {exam.CourseInstanceUid} не найден");

        if (errors.Count > 0)
        {
            throw new ArgumentException($"Validation failed: {string.Join("; ", errors)}");
        }
    }

    #endregion
}
