using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Infrastructure.Data;
using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Models;
using ViridiscaUi.Infrastructure.Logger;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для работы с заданиями
/// Независимый сервис без наследования от GenericCrudService
/// </summary>
public class AssignmentService : IAssignmentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AssignmentService> _logger;

    public AssignmentService(
        ApplicationDbContext dbContext, 
        INotificationService notificationService, 
        ILogger<AssignmentService> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    #region IAssignmentService Implementation

    /// <summary>
    /// Получает задание по ID
    /// </summary>
    public async Task<Assignment?> GetByIdAsync(Guid assignmentUid)
    {
        return await GetByUidAsync(assignmentUid);
    }

    /// <summary>
    /// Обновляет задание
    /// </summary>
    public async Task<Assignment> UpdateAsync(Assignment assignment)
    {
        try
        {
            _dbContext.Assignments.Update(assignment);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Обновлено задание {AssignmentTitle} с ID {AssignmentUid}", assignment.Title, assignment.Uid);
            return assignment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении задания {AssignmentUid}", assignment.Uid);
            throw;
        }
    }

    /// <summary>
    /// Получает задания для курса
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetByCourseInstanceAsync(Guid courseInstanceUid)
    {
        return await GetAssignmentsByCourseInstanceAsync(courseInstanceUid);
    }

    /// <summary>
    /// Получает задания для студента
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetByStudentAsync(Guid studentUid)
    {
        return await GetAssignmentsByStudentAsync(studentUid);
    }

    /// <summary>
    /// Получает просроченные задания
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetOverdueAsync()
    {
        return await GetOverdueAssignmentsAsync();
    }

    /// <summary>
    /// Получает задания с приближающимися дедлайнами
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetUpcomingDeadlinesAsync(int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(days);
        return await GetUpcomingAssignmentsAsync(cutoffDate);
    }

    /// <summary>
    /// Валидирует задание
    /// </summary>
    public async Task<ValidationResult> ValidateAsync(Assignment assignment)
    {
        try
        {
            await ValidateAssignmentAsync(assignment, false);
            return ValidationResult.Success();
        }
        catch (ArgumentException ex)
        {
            return ValidationResult.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return ValidationResult.Failure($"Ошибка валидации: {ex.Message}");
        }
    }

    /// <summary>
    /// Получает аналитику задания
    /// </summary>
    public async Task<AssignmentAnalytics> GetAnalyticsAsync(Guid assignmentUid)
    {
        return await GetAssignmentAnalyticsAsync(assignmentUid);
    }

    /// <summary>
    /// Получает аналитику задания (альтернативное имя для совместимости)
    /// </summary>
    public async Task<AssignmentAnalytics> GetAssignmentAnalyticsAsync(Guid assignmentUid)
    {
        try
        {
            var analytics = await _dbContext.AssignmentAnalytics
                .Include(aa => aa.Assignment)
                    .ThenInclude(a => a!.CourseInstance)
                        .ThenInclude(ci => ci!.Enrollments)
                .Include(aa => aa.Assignment)
                    .ThenInclude(a => a!.Submissions)
                .FirstOrDefaultAsync(aa => aa.AssignmentUid == assignmentUid);

            if (analytics == null)
            {
                // Создаем новую аналитику если не существует
                analytics = new AssignmentAnalytics
                {
                    AssignmentUid = assignmentUid,
                    LastCalculated = DateTime.UtcNow
                };

                _dbContext.AssignmentAnalytics.Add(analytics);
                await _dbContext.SaveChangesAsync();

                // Перезагружаем с навигационными свойствами
                analytics = await _dbContext.AssignmentAnalytics
                    .Include(aa => aa.Assignment)
                        .ThenInclude(a => a!.CourseInstance)
                            .ThenInclude(ci => ci!.Enrollments)
                    .Include(aa => aa.Assignment)
                        .ThenInclude(a => a!.Submissions)
                    .FirstAsync(aa => aa.Uid == analytics.Uid);
            }

            // Обновляем вычисляемые поля если нужно
            if (analytics.ShouldRecalculate(TimeSpan.FromHours(1)))
            {
                analytics.RefreshCalculatedFields();
                await _dbContext.SaveChangesAsync();
            }

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении аналитики задания {AssignmentUid}", assignmentUid);
            throw;
        }
    }

    #endregion

    #region Базовые CRUD операции

    /// <summary>
    /// Получает задание по идентификатору
    /// </summary>
    public async Task<Assignment?> GetByUidAsync(Guid uid)
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Teacher)
                        .ThenInclude(t => t.Person)
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Group)
                .Include(a => a.Submissions)
                .FirstOrDefaultAsync(a => a.Uid == uid && !a.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignment {AssignmentUid}", uid);
            throw;
        }
    }

    /// <summary>
    /// Получает все задания
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetAllAsync()
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Teacher)
                        .ThenInclude(t => t.Person)
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Group)
                .Where(a => !a.IsDeleted)
                .OrderBy(a => a.DueDate)
                .ThenBy(a => a.Title)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all assignments");
            throw;
        }
    }

    /// <summary>
    /// Создает новое задание
    /// </summary>
    public async Task<Assignment> CreateAsync(Assignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        try
        {
            // Валидация
            await ValidateAssignmentAsync(assignment, true);

            assignment.Uid = Guid.NewGuid();
            assignment.CreatedAt = DateTime.UtcNow;
            assignment.LastModifiedAt = DateTime.UtcNow;

            _dbContext.Assignments.Add(assignment);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created assignment {AssignmentTitle} for course instance {CourseInstanceUid}", 
                assignment.Title, assignment.CourseInstanceUid);
            return assignment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating assignment {AssignmentTitle}", assignment.Title);
            throw;
        }
    }

    /// <summary>
    /// Удаляет задание
    /// </summary>
    public async Task<bool> DeleteAsync(Guid uid)
    {
        try
        {
            var assignment = await _dbContext.Assignments.FindAsync(uid);
            if (assignment == null || assignment.IsDeleted)
                return false;

            // Мягкое удаление
            assignment.IsDeleted = true;
            assignment.DeletedAt = DateTime.UtcNow;
            assignment.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted assignment {AssignmentUid}", uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting assignment {AssignmentUid}", uid);
            throw;
        }
    }

    #endregion

    #region Специфичные методы для заданий

    /// <summary>
    /// Получает задания по экземпляру курса
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetAssignmentsByCourseInstanceAsync(Guid courseInstanceUid)
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Include(a => a.Submissions)
                .Where(a => a.CourseInstanceUid == courseInstanceUid && !a.IsDeleted)
                .OrderBy(a => a.DueDate)
                .ThenBy(a => a.Title)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignments for course instance {CourseInstanceUid}", courseInstanceUid);
            throw;
        }
    }

    /// <summary>
    /// Получает задания по статусу
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetAssignmentsByStatusAsync(AssignmentStatus status)
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Where(a => a.Status == status && !a.IsDeleted)
                .OrderBy(a => a.DueDate)
                .ThenBy(a => a.Title)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignments by status {Status}", status);
            throw;
        }
    }

    /// <summary>
    /// Получает задания по типу
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetAssignmentsByTypeAsync(AssignmentType type)
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Where(a => a.Type == type && !a.IsDeleted)
                .OrderBy(a => a.DueDate)
                .ThenBy(a => a.Title)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignments by type {Type}", type);
            throw;
        }
    }

    /// <summary>
    /// Получает задания по сложности
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetAssignmentsByDifficultyAsync(AssignmentDifficulty difficulty)
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Where(a => a.Difficulty == difficulty && !a.IsDeleted)
                .OrderBy(a => a.DueDate)
                .ThenBy(a => a.Title)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignments by difficulty {Difficulty}", difficulty);
            throw;
        }
    }

    /// <summary>
    /// Получает предстоящие задания
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetUpcomingAssignmentsAsync(DateTime? beforeDate = null)
    {
        try
        {
            var cutoffDate = beforeDate ?? DateTime.UtcNow.AddDays(7);

            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Where(a => !a.IsDeleted && 
                           a.Status == AssignmentStatus.Published &&
                           a.DueDate.HasValue && 
                           a.DueDate.Value > DateTime.UtcNow && 
                           a.DueDate.Value <= cutoffDate)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming assignments");
            throw;
        }
    }

    /// <summary>
    /// Получает просроченные задания
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetOverdueAssignmentsAsync()
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Where(a => !a.IsDeleted && 
                           a.Status == AssignmentStatus.Published &&
                           a.DueDate.HasValue && 
                           a.DueDate.Value < DateTime.UtcNow)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue assignments");
            throw;
        }
    }

    /// <summary>
    /// Поиск заданий по тексту
    /// </summary>
    public async Task<IEnumerable<Assignment>> SearchAssignmentsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var lowerSearchTerm = searchTerm.ToLower();

            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Where(a => !a.IsDeleted && (
                    a.Title.ToLower().Contains(lowerSearchTerm) ||
                    a.Description.ToLower().Contains(lowerSearchTerm) ||
                    a.Instructions.ToLower().Contains(lowerSearchTerm) ||
                    a.CourseInstance != null && a.CourseInstance.Subject != null && 
                     a.CourseInstance.Subject.Name.ToLower().Contains(lowerSearchTerm)
                ))
                .OrderBy(a => a.DueDate)
                .ThenBy(a => a.Title)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching assignments with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Получает задания с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Assignment> Assignments, int TotalCount)> GetAssignmentsPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        Guid? courseInstanceUid = null, 
        AssignmentStatus? status = null)
    {
        try
        {
            var query = _dbContext.Assignments
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Where(a => !a.IsDeleted);

            // Применяем фильтры
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(a => 
                    a.Title.ToLower().Contains(lowerSearchTerm) ||
                    a.Description.ToLower().Contains(lowerSearchTerm) ||
                    a.Instructions.ToLower().Contains(lowerSearchTerm)
                );
            }

            if (courseInstanceUid.HasValue)
            {
                query = query.Where(a => a.CourseInstanceUid == courseInstanceUid.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            var assignments = await query
                .OrderBy(a => a.DueDate)
                .ThenBy(a => a.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (assignments, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged assignments");
            throw;
        }
    }

    /// <summary>
    /// Получает задания с пагинацией (алиас для GetAssignmentsPagedAsync)
    /// </summary>
    public async Task<(IEnumerable<Assignment> Assignments, int TotalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        Guid? courseInstanceUid = null, 
        AssignmentStatus? status = null)
    {
        return await GetAssignmentsPagedAsync(page, pageSize, searchTerm, courseInstanceUid, status);
    }

    /// <summary>
    /// Получает статистику задания
    /// </summary>
    public async Task<AssignmentAnalytics> GetStatisticsAsync(Guid assignmentUid)
    {
        try
        {
            var statisticsService = new StatisticsService(_dbContext);
            return await statisticsService.GetAssignmentAnalyticsAsync(assignmentUid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignment statistics for {AssignmentUid}", assignmentUid);
            return new AssignmentAnalytics
            {
                AssignmentUid = assignmentUid,
                LastCalculated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Публикует задание
    /// </summary>
    public async Task<bool> PublishAssignmentAsync(Guid assignmentUid)
    {
        try
        {
            var assignment = await _dbContext.Assignments.FindAsync(assignmentUid);
            if (assignment == null || assignment.IsDeleted)
                return false;

            assignment.Status = AssignmentStatus.Published;
            assignment.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Published assignment {AssignmentUid}", assignmentUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing assignment {AssignmentUid}", assignmentUid);
            return false;
        }
    }

    /// <summary>
    /// Отменяет публикацию задания
    /// </summary>
    public async Task<bool> UnpublishAssignmentAsync(Guid assignmentUid)
    {
        try
        {
            var assignment = await _dbContext.Assignments.FindAsync(assignmentUid);
            if (assignment == null || assignment.IsDeleted)
                return false;

            assignment.Status = AssignmentStatus.Draft;
            assignment.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Unpublished assignment {AssignmentUid}", assignmentUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpublishing assignment {AssignmentUid}", assignmentUid);
            return false;
        }
    }

    /// <summary>
    /// Архивирует задание
    /// </summary>
    public async Task<bool> ArchiveAssignmentAsync(Guid assignmentUid)
    {
        try
        {
            var assignment = await _dbContext.Assignments.FindAsync(assignmentUid);
            if (assignment == null || assignment.IsDeleted)
                return false;

            assignment.Status = AssignmentStatus.Archived;
            assignment.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Archived assignment {AssignmentUid}", assignmentUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving assignment {AssignmentUid}", assignmentUid);
            return false;
        }
    }

    /// <summary>
    /// Дублирует задание
    /// </summary>
    public async Task<bool> DuplicateAssignmentAsync(Guid assignmentUid, string newTitle)
    {
        try
        {
            var originalAssignment = await GetByUidAsync(assignmentUid);
            if (originalAssignment == null)
                return false;

            var duplicatedAssignment = new Assignment
            {
                Title = newTitle,
                Description = originalAssignment.Description,
                Instructions = originalAssignment.Instructions,
                DueDate = originalAssignment.DueDate?.AddDays(7), // Сдвигаем на неделю
                MaxScore = originalAssignment.MaxScore,
                Type = originalAssignment.Type,
                Difficulty = originalAssignment.Difficulty,
                Status = AssignmentStatus.Draft,
                CourseInstanceUid = originalAssignment.CourseInstanceUid
            };

            await CreateAsync(duplicatedAssignment);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating assignment {AssignmentUid}", assignmentUid);
            return false;
        }
    }

    /// <summary>
    /// Получает количество сдач задания
    /// </summary>
    public async Task<int> GetSubmissionsCountAsync(Guid assignmentUid)
    {
        try
        {
            return await _dbContext.Submissions
                .CountAsync(s => s.AssignmentUid == assignmentUid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submissions count for assignment {AssignmentUid}", assignmentUid);
            return 0;
        }
    }

    /// <summary>
    /// Получает сдачи задания
    /// </summary>
    public async Task<IEnumerable<Submission>> GetSubmissionsAsync(Guid assignmentUid)
    {
        try
        {
            return await _dbContext.Submissions
                .Include(s => s.Student)
                    .ThenInclude(st => st.Person)
                .Where(s => s.AssignmentUid == assignmentUid)
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submissions for assignment {AssignmentUid}", assignmentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает средний балл по заданию
    /// </summary>
    public async Task<double> GetAverageGradeAsync(Guid assignmentUid)
    {
        try
        {
            return await _dbContext.Submissions
                .Where(s => s.AssignmentUid == assignmentUid && s.Score.HasValue)
                .AverageAsync(s => s.Score) ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average grade for assignment {AssignmentUid}", assignmentUid);
            return 0;
        }
    }

    /// <summary>
    /// Устанавливает максимальные баллы для задания
    /// </summary>
    public async Task<bool> SetMaxPointsAsync(Guid assignmentUid, int maxPoints)
    {
        try
        {
            var assignment = await _dbContext.Assignments.FindAsync(assignmentUid);
            if (assignment == null || assignment.IsDeleted)
                return false;

            assignment.MaxScore = maxPoints;
            assignment.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting max points for assignment {AssignmentUid}", assignmentUid);
            return false;
        }
    }

    /// <summary>
    /// Обновляет дату сдачи задания
    /// </summary>
    public async Task<bool> UpdateDueDateAsync(Guid assignmentUid, DateTime? dueDate)
    {
        try
        {
            var assignment = await _dbContext.Assignments.FindAsync(assignmentUid);
            if (assignment == null || assignment.IsDeleted)
                return false;

            assignment.DueDate = dueDate;
            assignment.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating due date for assignment {AssignmentUid}", assignmentUid);
            return false;
        }
    }

    /// <summary>
    /// Получает задания преподавателя
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetAssignmentsByTeacherAsync(Guid teacherUid)
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Where(a => !a.IsDeleted && a.CourseInstance.TeacherUid == teacherUid)
                .OrderBy(a => a.DueDate)
                .ThenBy(a => a.Title)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignments for teacher {TeacherUid}", teacherUid);
            throw;
        }
    }

    /// <summary>
    /// Получает задания студента
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetAssignmentsByStudentAsync(Guid studentUid)
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Where(a => !a.IsDeleted && 
                           a.Status == AssignmentStatus.Published &&
                           a.CourseInstance.Enrollments.Any(e => e.StudentUid == studentUid))
                .OrderBy(a => a.DueDate)
                .ThenBy(a => a.Title)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignments for student {StudentUid}", studentUid);
            throw;
        }
    }

    /// <summary>
    /// Проверяет, может ли студент сдать задание
    /// </summary>
    public async Task<bool> CanStudentSubmitAsync(Guid assignmentUid, Guid studentUid)
    {
        try
        {
            var assignment = await GetByUidAsync(assignmentUid);
            if (assignment == null || assignment.Status != AssignmentStatus.Published)
                return false;

            // Проверяем, записан ли студент на курс
            var isEnrolled = await _dbContext.Enrollments
                .AnyAsync(e => e.CourseInstanceUid == assignment.CourseInstanceUid && e.StudentUid == studentUid);

            if (!isEnrolled)
                return false;

            // Проверяем дедлайн
            if (assignment.DueDate.HasValue && DateTime.UtcNow > assignment.DueDate.Value)
                return false;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if student {StudentUid} can submit assignment {AssignmentUid}", studentUid, assignmentUid);
            return false;
        }
    }

    /// <summary>
    /// Получает задание с сдачами
    /// </summary>
    public async Task<Assignment?> GetAssignmentWithSubmissionsAsync(Guid assignmentUid)
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Student)
                        .ThenInclude(st => st.Person)
                .FirstOrDefaultAsync(a => a.Uid == assignmentUid && !a.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignment with submissions {AssignmentUid}", assignmentUid);
            throw;
        }
    }

    /// <summary>
    /// Проверяет, просрочено ли задание
    /// </summary>
    public async Task<bool> IsAssignmentOverdueAsync(Guid assignmentUid)
    {
        try
        {
            var assignment = await _dbContext.Assignments
                .FirstOrDefaultAsync(a => a.Uid == assignmentUid && !a.IsDeleted);

            if (assignment?.DueDate == null)
                return false;

            return DateTime.UtcNow > assignment.DueDate.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if assignment is overdue {AssignmentUid}", assignmentUid);
            return false;
        }
    }

    /// <summary>
    /// Получает время до дедлайна
    /// </summary>
    public async Task<TimeSpan?> GetTimeUntilDueAsync(Guid assignmentUid)
    {
        try
        {
            var assignment = await _dbContext.Assignments
                .FirstOrDefaultAsync(a => a.Uid == assignmentUid && !a.IsDeleted);

            if (assignment?.DueDate == null)
                return null;

            var timeUntilDue = assignment.DueDate.Value - DateTime.UtcNow;
            return timeUntilDue.TotalMilliseconds > 0 ? timeUntilDue : TimeSpan.Zero;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting time until due for assignment {AssignmentUid}", assignmentUid);
            return null;
        }
    }

    /// <summary>
    /// Получает все задания (алиас)
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetAllAssignmentsAsync()
    {
        return await GetAllAsync();
    }

    /// <summary>
    /// Получает задание по названию и курсу
    /// </summary>
    public async Task<Assignment?> GetByTitleAndCourseAsync(string title, Guid courseInstanceUid)
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                .ThenInclude(ci => ci.Subject)
                .Include(a => a.CourseInstance)
                .ThenInclude(ci => ci.Group)
                .FirstOrDefaultAsync(a => 
                    a.Title == title && 
                    a.CourseInstanceUid == courseInstanceUid && 
                    !a.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignment by title {Title} and course {CourseInstanceUid}", title, courseInstanceUid);
            throw;
        }
    }

    /// <summary>
    /// Получает количество оценок для задания
    /// </summary>
    public async Task<int> GetGradesCountAsync(Guid assignmentUid)
    {
        try
        {
            return await _dbContext.Grades
                .CountAsync(g => g.AssignmentUid == assignmentUid && !g.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grades count for assignment {AssignmentUid}", assignmentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику задания
    /// </summary>
    public async Task<AssignmentAnalytics> GetAssignmentStatisticsAsync(Guid assignmentUid)
    {
        try
        {
            var assignment = await GetByUidAsync(assignmentUid);
            if (assignment == null)
                throw new ArgumentException($"Assignment with UID {assignmentUid} not found");

            var submissionsCount = await GetSubmissionsCountAsync(assignmentUid);
            var gradesCount = await GetGradesCountAsync(assignmentUid);
            var enrolledStudentsCount = await _dbContext.Enrollments
                .CountAsync(e => e.CourseInstanceUid == assignment.CourseInstanceUid && !e.IsDeleted);

            var averageScore = await GetAverageGradeAsync(assignmentUid);
            var submissionRate = enrolledStudentsCount > 0 ? (double)submissionsCount / enrolledStudentsCount : 0;

            return new AssignmentAnalytics
            {
                AssignmentUid = assignmentUid,
                LastCalculated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignment statistics for {AssignmentUid}", assignmentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает сдачи задания
    /// </summary>
    public async Task<IEnumerable<Submission>> GetSubmissionsByAssignmentAsync(Guid assignmentUid)
    {
        try
        {
            return await _dbContext.Submissions
                .Include(s => s.Student)
                .ThenInclude(st => st.Person)
                .Include(s => s.Assignment)
                .Where(s => s.AssignmentUid == assignmentUid && !s.IsDeleted)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submissions for assignment {AssignmentUid}", assignmentUid);
            throw;
        }
    }

    /// <summary>
    /// Отправляет напоминание о дедлайне
    /// </summary>
    public async Task SendDueDateReminderAsync(Guid assignmentUid)
    {
        try
        {
            var assignment = await GetByUidAsync(assignmentUid);
            if (assignment == null)
                return;

            // Получаем студентов, которые еще не сдали задание
            var studentsWithoutSubmissions = await _dbContext.Enrollments
                .Include(e => e.Student)
                .ThenInclude(s => s.Person)
                .Where(e => e.CourseInstanceUid == assignment.CourseInstanceUid && !e.IsDeleted)
                .Where(e => !_dbContext.Submissions.Any(sub => 
                    sub.AssignmentUid == assignmentUid && 
                    sub.StudentUid == e.StudentUid && 
                    !sub.IsDeleted))
                .Select(e => e.Student)
                .ToListAsync();

            // Отправляем уведомления (заглушка - в реальной системе здесь будет отправка email/push)
            foreach (var student in studentsWithoutSubmissions)
            {
                _logger.LogInformation("Sending due date reminder to student {StudentUid} for assignment {AssignmentUid}", 
                    student.Uid, assignmentUid);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending due date reminder for assignment {AssignmentUid}", assignmentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает задания, ожидающие оценивания
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetAssignmentsPendingGradingAsync()
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                .ThenInclude(ci => ci.Subject)
                .Include(a => a.CourseInstance)
                .ThenInclude(ci => ci.Group)
                .Where(a => !a.IsDeleted && 
                           a.Status == AssignmentStatus.Published &&
                           _dbContext.Submissions.Any(s => 
                               s.AssignmentUid == a.Uid && 
                               !s.IsDeleted &&
                               !_dbContext.Grades.Any(g => 
                                   g.AssignmentUid == a.Uid && 
                                   g.StudentUid == s.StudentUid && 
                                   !g.IsDeleted)))
                .OrderBy(a => a.DueDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignments pending grading");
            throw;
        }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Валидация задания
    /// </summary>
    private async Task ValidateAssignmentAsync(Assignment assignment, bool isCreate)
    {
        var errors = new List<string>();

        // Проверка обязательных полей
        if (string.IsNullOrWhiteSpace(assignment.Title))
            errors.Add("Название задания обязательно для заполнения");

        if (assignment.CourseInstanceUid == Guid.Empty)
            errors.Add("Экземпляр курса обязателен для задания");

        // Проверка максимального балла
        if (assignment.MaxScore <= 0)
            errors.Add("Максимальный балл должен быть больше нуля");

        // Проверка существования экземпляра курса
        var courseInstanceExists = await _dbContext.CourseInstances
            .AnyAsync(ci => ci.Uid == assignment.CourseInstanceUid && !ci.IsDeleted);

        if (!courseInstanceExists)
            errors.Add($"Экземпляр курса с Uid {assignment.CourseInstanceUid} не найден");

        // Проверка уникальности названия в рамках курса
        if (!string.IsNullOrWhiteSpace(assignment.Title))
        {
            var titleExists = await _dbContext.Assignments
                .Where(a => a.Uid != assignment.Uid && 
                           a.CourseInstanceUid == assignment.CourseInstanceUid && 
                           a.Title.ToLower() == assignment.Title.ToLower() && 
                           !a.IsDeleted)
                .AnyAsync();

            if (titleExists)
                errors.Add($"Задание с названием '{assignment.Title}' уже существует в этом курсе");
        }

        if (errors.Any())
        {
            throw new ArgumentException($"Validation failed: {string.Join("; ", errors)}");
        }
    }

    #endregion
}