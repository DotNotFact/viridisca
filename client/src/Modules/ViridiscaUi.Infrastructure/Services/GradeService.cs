using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Models.Common;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Data;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для работы с оценками
/// Независимый сервис без наследования от GenericCrudService
/// </summary>
public class GradeService : IGradeService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GradeService> _logger;

    public GradeService(ApplicationDbContext dbContext, ILogger<GradeService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Базовые CRUD операции

    /// <summary>
    /// Получает оценку по идентификатору
    /// </summary>
    public async Task<Grade?> GetByUidAsync(Guid uid)
    {
        try
        {
            return await _dbContext.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.Person)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                        .ThenInclude(ci => ci.Subject)
                .Include(g => g.Teacher)
                    .ThenInclude(t => t.Person)
                .FirstOrDefaultAsync(g => g.Uid == uid && !g.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grade {GradeUid}", uid);
            throw;
        }
    }

    /// <summary>
    /// Получает все оценки
    /// </summary>
    public async Task<IEnumerable<Grade>> GetAllAsync()
    {
        try
        {
            return await _dbContext.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.Person)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                        .ThenInclude(ci => ci.Subject)
                .Include(g => g.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(g => !g.IsDeleted)
                .OrderByDescending(g => g.IssuedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all grades");
            throw;
        }
    }

    /// <summary>
    /// Создает новую оценку
    /// </summary>
    public async Task<Grade> CreateAsync(Grade grade)
    {
        ArgumentNullException.ThrowIfNull(grade);

        try
        {
            // Валидация
            await ValidateGradeAsync(grade, true);

            grade.Uid = Guid.NewGuid();
            grade.IssuedAt = DateTime.UtcNow;
            grade.CreatedAt = DateTime.UtcNow;
            grade.LastModifiedAt = DateTime.UtcNow;

            _dbContext.Grades.Add(grade);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created grade {GradeValue} for student {StudentUid} and assignment {AssignmentUid}", 
                grade.Value, grade.StudentUid, grade.AssignmentUid);
            return grade;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating grade for student {StudentUid}", grade.StudentUid);
            throw;
        }
    }

    /// <summary>
    /// Обновляет существующую оценку
    /// </summary>
    public async Task<bool> UpdateAsync(Grade grade)
    {
        ArgumentNullException.ThrowIfNull(grade);

        try
        {
            var existingGrade = await _dbContext.Grades.FindAsync(grade.Uid);
            if (existingGrade == null || existingGrade.IsDeleted)
                return false;

            // Валидация
            await ValidateGradeAsync(grade, false);

            // Обновляем поля
            existingGrade.Value = grade.Value;
            existingGrade.Comment = grade.Comment;
            existingGrade.Type = grade.Type;
            existingGrade.IsPublished = grade.IsPublished;
            existingGrade.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated grade {GradeUid}", grade.Uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating grade {GradeUid}", grade.Uid);
            throw;
        }
    }

    /// <summary>
    /// Удаляет оценку
    /// </summary>
    public async Task<bool> DeleteAsync(Guid uid)
    {
        try
        {
            var grade = await _dbContext.Grades.FindAsync(uid);
            if (grade == null || grade.IsDeleted)
                return false;

            // Мягкое удаление
            grade.IsDeleted = true;
            grade.DeletedAt = DateTime.UtcNow;
            grade.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted grade {GradeUid}", uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting grade {GradeUid}", uid);
            throw;
        }
    }

    #endregion

    #region Специфичные методы для оценок

    /// <summary>
    /// Получает оценки студента
    /// </summary>
    public async Task<IEnumerable<Grade>> GetGradesByStudentAsync(Guid studentUid)
    {
        try
        {
            return await _dbContext.Grades
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                        .ThenInclude(ci => ci.Subject)
                .Include(g => g.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(g => g.StudentUid == studentUid && !g.IsDeleted)
                .OrderByDescending(g => g.IssuedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grades for student {StudentUid}", studentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает оценки преподавателя
    /// </summary>
    public async Task<IEnumerable<Grade>> GetGradesByTeacherAsync(Guid teacherUid)
    {
        try
        {
            return await _dbContext.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.Person)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                        .ThenInclude(ci => ci.Subject)
                .Where(g => g.TeacherUid == teacherUid && !g.IsDeleted)
                .OrderByDescending(g => g.IssuedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grades for teacher {TeacherUid}", teacherUid);
            throw;
        }
    }

    /// <summary>
    /// Получает оценки по экземпляру курса
    /// </summary>
    public async Task<IEnumerable<Grade>> GetGradesByCourseInstanceAsync(Guid courseInstanceUid)
    {
        try
        {
            return await _dbContext.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.Person)
                .Include(g => g.Assignment)
                .Include(g => g.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(g => g.Assignment.CourseInstanceUid == courseInstanceUid && !g.IsDeleted)
                .OrderByDescending(g => g.IssuedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grades for course instance {CourseInstanceUid}", courseInstanceUid);
            throw;
        }
    }

    /// <summary>
    /// Получает оценки по заданию
    /// </summary>
    public async Task<IEnumerable<Grade>> GetGradesByAssignmentAsync(Guid assignmentUid)
    {
        try
        {
            return await _dbContext.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.Person)
                .Include(g => g.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(g => g.AssignmentUid == assignmentUid && !g.IsDeleted)
                .OrderByDescending(g => g.IssuedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grades for assignment {AssignmentUid}", assignmentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает оценки по типу
    /// </summary>
    public async Task<IEnumerable<Grade>> GetGradesByTypeAsync(GradeType gradeType)
    {
        try
        {
            return await _dbContext.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.Person)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                        .ThenInclude(ci => ci.Subject)
                .Include(g => g.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(g => g.Type == gradeType && !g.IsDeleted)
                .OrderByDescending(g => g.IssuedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grades by type {GradeType}", gradeType);
            throw;
        }
    }

    /// <summary>
    /// Получает средний балл студента
    /// </summary>
    public async Task<double> GetStudentAverageGradeAsync(Guid studentUid)
    {
        try
        {
            return await _dbContext.Grades
                .Where(g => g.StudentUid == studentUid && !g.IsDeleted && g.IsPublished)
                .AverageAsync(g => (double)g.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average grade for student {StudentUid}", studentUid);
            return 0;
        }
    }

    /// <summary>
    /// Получает средний балл по экземпляру курса
    /// </summary>
    public async Task<double> GetCourseInstanceAverageGradeAsync(Guid courseInstanceUid)
    {
        try
        {
            return await _dbContext.Grades
                .Where(g => g.Assignment.CourseInstanceUid == courseInstanceUid && !g.IsDeleted && g.IsPublished)
                .AverageAsync(g => (double)g.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average grade for course instance {CourseInstanceUid}", courseInstanceUid);
            return 0;
        }
    }

    /// <summary>
    /// Получает оценки с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Grade> Grades, int TotalCount)> GetGradesPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        Guid? studentUid = null, 
        Guid? courseInstanceUid = null)
    {
        try
        {
            var query = _dbContext.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.Person)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                        .ThenInclude(ci => ci.Subject)
                .Include(g => g.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(g => !g.IsDeleted);

            // Применяем фильтры
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(g => 
                    g.Comment.ToLower().Contains(lowerSearchTerm) ||
                    g.Student != null && g.Student.Person != null && (
                        g.Student.Person.FirstName.ToLower().Contains(lowerSearchTerm) ||
                        g.Student.Person.LastName.ToLower().Contains(lowerSearchTerm)
                    )
                );
            }

            if (studentUid.HasValue)
            {
                query = query.Where(g => g.StudentUid == studentUid.Value);
            }

            if (courseInstanceUid.HasValue)
            {
                query = query.Where(g => g.Assignment.CourseInstanceUid == courseInstanceUid.Value);
            }

            var totalCount = await query.CountAsync();

            var grades = await query
                .OrderByDescending(g => g.IssuedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (grades, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged grades");
            throw;
        }
    }

    /// <summary>
    /// Публикует оценку
    /// </summary>
    public async Task<bool> PublishGradeAsync(Guid gradeUid)
    {
        try
        {
            var grade = await _dbContext.Grades.FindAsync(gradeUid);
            if (grade == null || grade.IsDeleted)
                return false;

            grade.IsPublished = true;
            grade.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Published grade {GradeUid}", gradeUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing grade {GradeUid}", gradeUid);
            return false;
        }
    }

    /// <summary>
    /// Отменяет публикацию оценки
    /// </summary>
    public async Task<bool> UnpublishGradeAsync(Guid gradeUid)
    {
        try
        {
            var grade = await _dbContext.Grades.FindAsync(gradeUid);
            if (grade == null || grade.IsDeleted)
                return false;

            grade.IsPublished = false;
            grade.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Unpublished grade {GradeUid}", gradeUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpublishing grade {GradeUid}", gradeUid);
            return false;
        }
    }

    /// <summary>
    /// Получает статистику оценок (исправленный тип возврата)
    /// </summary>
    public async Task<(decimal AverageGrade, decimal MaxGrade, decimal MinGrade, int TotalGrades, Dictionary<string, int> GradeDistribution)> GetGradeStatisticsAsync(Guid? courseInstanceUid = null, Guid? studentUid = null)
    {
        try
        {
            var query = _dbContext.Grades.Where(g => !g.IsDeleted && g.IsPublished);

            if (studentUid.HasValue)
            {
                query = query.Where(g => g.StudentUid == studentUid.Value);
            }

            if (courseInstanceUid.HasValue)
            {
                query = query.Where(g => g.Assignment.CourseInstanceUid == courseInstanceUid.Value);
            }

            var grades = await query.Select(g => g.Value).ToListAsync();

            if (!grades.Any())
            {
                return (0m, 0m, 0m, 0, new Dictionary<string, int>());
            }

            var averageGrade = grades.Average();
            var maxGrade = grades.Max();
            var minGrade = grades.Min();
            var totalGrades = grades.Count;

            // Создаем распределение оценок по диапазонам
            var gradeDistribution = new Dictionary<string, int>
            {
                ["Отлично (90-100)"] = grades.Count(g => g >= 90),
                ["Хорошо (75-89)"] = grades.Count(g => g >= 75 && g < 90),
                ["Удовлетворительно (60-74)"] = grades.Count(g => g >= 60 && g < 75),
                ["Неудовлетворительно (0-59)"] = grades.Count(g => g < 60)
            };

            return (averageGrade, maxGrade, minGrade, totalGrades, gradeDistribution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grade statistics for course instance {CourseInstanceUid} and student {StudentUid}", courseInstanceUid, studentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает недавние оценки
    /// </summary>
    public async Task<IEnumerable<Grade>> GetRecentGradesAsync(int count = 10)
    {
        try
        {
            return await _dbContext.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.Person)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                        .ThenInclude(ci => ci.Subject)
                .Include(g => g.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(g => !g.IsDeleted && g.IsPublished)
                .OrderByDescending(g => g.IssuedAt)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent grades");
            throw;
        }
    }

    /// <summary>
    /// Получает неопубликованные оценки
    /// </summary>
    public async Task<IEnumerable<Grade>> GetUnpublishedGradesAsync()
    {
        try
        {
            return await _dbContext.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.Person)
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                        .ThenInclude(ci => ci.Subject)
                .Include(g => g.Teacher)
                    .ThenInclude(t => t.Person)
                .Where(g => !g.IsDeleted && !g.IsPublished)
                .OrderByDescending(g => g.IssuedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unpublished grades");
            throw;
        }
    }

    /// <summary>
    /// Массовая публикация оценок
    /// </summary>
    public async Task<bool> BulkPublishGradesAsync(IEnumerable<Guid> gradeUids)
    {
        try
        {
            var grades = await _dbContext.Grades
                .Where(g => gradeUids.Contains(g.Uid) && !g.IsDeleted)
                .ToListAsync();

            foreach (var grade in grades)
            {
                grade.IsPublished = true;
                grade.LastModifiedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Bulk published {Count} grades", grades.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk publishing grades");
            return false;
        }
    }

    /// <summary>
    /// Получает оценки с пагинацией (алиас для GetGradesPagedAsync)
    /// </summary>
    public async Task<(IEnumerable<Grade> Grades, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? searchTerm = null,
        Guid? studentUid = null, Guid? courseInstanceUid = null)
    {
        return await GetGradesPagedAsync(page, pageSize, searchTerm, studentUid, courseInstanceUid);
    }

    /// <summary>
    /// Получает оценку по студенту и заданию
    /// </summary>
    public async Task<Grade?> GetByStudentAndAssignmentAsync(Guid studentUid, Guid assignmentUid)
    {
        try
        {
            return await _dbContext.Grades
                .Include(g => g.Student)
                .ThenInclude(s => s.Person)
                .Include(g => g.Assignment)
                .FirstOrDefaultAsync(g => 
                    g.StudentUid == studentUid && 
                    g.AssignmentUid == assignmentUid && 
                    !g.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grade for student {StudentUid} and assignment {AssignmentUid}", studentUid, assignmentUid);
            throw;
        }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Валидация оценки
    /// </summary>
    private async Task ValidateGradeAsync(Grade grade, bool isCreate)
    {
        var errors = new List<string>();

        // Проверка обязательных полей
        if (grade.StudentUid == Guid.Empty)
            errors.Add("Студент обязателен для оценки");

        if (grade.AssignmentUid == Guid.Empty)
            errors.Add("Задание обязательно для оценки");

        if (grade.TeacherUid == Guid.Empty)
            errors.Add("Преподаватель обязателен для оценки");

        // Проверка значения оценки
        if (grade.Value < 0 || grade.Value > 100)
            errors.Add("Оценка должна быть от 0 до 100 баллов");

        // Проверка существования студента
        var studentExists = await _dbContext.Students
            .AnyAsync(s => s.Uid == grade.StudentUid);

        if (!studentExists)
            errors.Add($"Студент с Uid {grade.StudentUid} не найден");

        // Проверка существования задания
        var assignmentExists = await _dbContext.Assignments
            .AnyAsync(a => a.Uid == grade.AssignmentUid && !a.IsDeleted);

        if (!assignmentExists)
            errors.Add($"Задание с Uid {grade.AssignmentUid} не найдено");

        // Проверка существования преподавателя
        var teacherExists = await _dbContext.Teachers
            .AnyAsync(t => t.Uid == grade.TeacherUid);

        if (!teacherExists)
            errors.Add($"Преподаватель с Uid {grade.TeacherUid} не найден");

        // Проверка дублирования оценки
        if (isCreate)
        {
            var duplicateExists = await _dbContext.Grades
                .Where(g => g.StudentUid == grade.StudentUid && 
                           g.AssignmentUid == grade.AssignmentUid && 
                           !g.IsDeleted)
                .AnyAsync();

            if (duplicateExists)
                errors.Add("Оценка за это задание для данного студента уже существует");
        }

        if (errors.Any())
        {
            throw new ArgumentException($"Validation failed: {string.Join("; ", errors)}");
        }
    }

    #endregion
}

/// <summary>
/// Статистика оценок студента
/// </summary>
public class StudentGradeStatistics
{
    public Guid StudentUid { get; set; }
    public int TotalGrades { get; set; }
    public double AverageGrade { get; set; }
    public double HighestGrade { get; set; }
    public double LowestGrade { get; set; }
    public int PassingGrades { get; set; }
    public int FailingGrades { get; set; }
    public Dictionary<string, double> GradesBySubject { get; set; } = new();
}

/// <summary>
/// Статистика оценок по заданию
/// </summary>
public class AssignmentGradeStatistics
{
    public Guid AssignmentUid { get; set; }
    public int TotalSubmissions { get; set; }
    public double AverageGrade { get; set; }
    public double HighestGrade { get; set; }
    public double LowestGrade { get; set; }
    public int PassingSubmissions { get; set; }
    public int FailingSubmissions { get; set; }
    public Dictionary<string, int> GradeDistribution { get; set; } = new();
}

/// <summary>
/// Результат массового создания оценок
/// </summary>
public class BulkGradeResult
{
    public int SuccessfulGrades { get; set; }
    public int FailedGrades { get; set; }
    public List<Guid> CreatedGradeUids { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Статистика оценок по предметам
/// </summary>
public class GradeStatisticsBySubject
{
    public string SubjectName { get; set; } = string.Empty;
    public int TotalGrades { get; set; }
    public double AverageGrade { get; set; }
    public double HighestGrade { get; set; }
    public double LowestGrade { get; set; }
}
