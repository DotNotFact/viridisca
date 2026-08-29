using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Data;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.Domain.Entities.Education.Enums;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для работы с базовыми курсами (шаблонами)
/// Независимый сервис без наследования от GenericCrudService
/// </summary>
public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CourseService> _logger;

    public CourseService(ApplicationDbContext dbContext, ILogger<CourseService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Базовые CRUD операции

    /// <summary>
    /// Получает курс по идентификатору
    /// </summary>
    public async Task<Course?> GetByUidAsync(Guid uid)
    {
        try
        {
            return await _dbContext.Courses
                .Include(c => c.Department)
                .Include(c => c.CourseInstances)
                .FirstOrDefaultAsync(c => c.Uid == uid && !c.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course {CourseUid}", uid);
            throw;
        }
    }

    /// <summary>
    /// Получает все курсы
    /// </summary>
    public async Task<IEnumerable<Course>> GetAllAsync()
    {
        try
        {
            return await _dbContext.Courses
                .Include(c => c.Department)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all courses");
            throw;
        }
    }

    /// <summary>
    /// Создает новый курс
    /// </summary>
    public async Task<Course> CreateAsync(Course course)
    {
        ArgumentNullException.ThrowIfNull(course);

        try
        {
            // Валидация
            await ValidateCourseAsync(course, true);

            course.Uid = Guid.NewGuid();
            course.CreatedAt = DateTime.UtcNow;
            course.LastModifiedAt = DateTime.UtcNow;

            _dbContext.Courses.Add(course);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created course {CourseName} with code {CourseCode}", course.Name, course.Code);
            return course;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course {CourseName}", course.Name);
            throw;
        }
    }

    /// <summary>
    /// Обновляет существующий курс
    /// </summary>
    public async Task<bool> UpdateAsync(Course course)
    {
        ArgumentNullException.ThrowIfNull(course);

        try
        {
            var existingCourse = await _dbContext.Courses.FindAsync(course.Uid);
            if (existingCourse == null || existingCourse.IsDeleted)
                return false;

            // Валидация
            await ValidateCourseAsync(course, false);

            // Обновляем поля
            existingCourse.Name = course.Name;
            existingCourse.Code = course.Code;
            existingCourse.Description = course.Description;
            existingCourse.Credits = course.Credits;
            existingCourse.DurationHours = course.DurationHours;
            existingCourse.DepartmentUid = course.DepartmentUid;
            existingCourse.IsActive = course.IsActive;
            existingCourse.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated course {CourseName}", course.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course {CourseUid}", course.Uid);
            throw;
        }
    }

    /// <summary>
    /// Удаляет курс
    /// </summary>
    public async Task<bool> DeleteAsync(Guid uid)
    {
        try
        {
            var course = await _dbContext.Courses.FindAsync(uid);
            if (course == null || course.IsDeleted)
                return false;

            // Проверяем связанные данные
            var hasInstances = await _dbContext.CourseInstances
                .AnyAsync(ci => ci.CourseUid == uid && !ci.IsDeleted);

            if (hasInstances)
                throw new InvalidOperationException("Cannot delete course as it has associated course instances");

            // Мягкое удаление
            course.IsDeleted = true;
            course.DeletedAt = DateTime.UtcNow;
            course.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted course {CourseUid}", uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course {CourseUid}", uid);
            throw;
        }
    }

    #endregion

    #region Специфичные методы для курсов

    /// <summary>
    /// Получает курсы по департаменту
    /// </summary>
    public async Task<IEnumerable<Course>> GetCoursesByDepartmentAsync(Guid departmentUid)
    {
        try
        {
            return await _dbContext.Courses
                .Include(c => c.Department)
                .Where(c => !c.IsDeleted && c.DepartmentUid == departmentUid)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting courses for department {DepartmentUid}", departmentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает курсы по предмету
    /// </summary>
    public async Task<IEnumerable<Course>> GetCoursesBySubjectAsync(Guid subjectUid)
    {
        try
        {
            return await _dbContext.Courses
                .Include(c => c.Department)
                .Where(c => !c.IsDeleted && c.SubjectUid == subjectUid)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting courses for subject {SubjectUid}", subjectUid);
            throw;
        }
    }

    /// <summary>
    /// Поиск курсов
    /// </summary>
    public async Task<IEnumerable<Course>> SearchCoursesAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var lowerSearchTerm = searchTerm.ToLower();

            return await _dbContext.Courses
                .Include(c => c.Department)
                .Where(c => !c.IsDeleted && (
                    c.Name.ToLower().Contains(lowerSearchTerm) ||
                    c.Code.ToLower().Contains(lowerSearchTerm) ||
                    c.Description.ToLower().Contains(lowerSearchTerm) ||
                    c.Department != null && c.Department.Name.ToLower().Contains(lowerSearchTerm)
                ))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching courses with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Получает курсы с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Course> Courses, int TotalCount)> GetCoursesPagedAsync(
        int page, int pageSize, string? searchTerm = null, Guid? departmentUid = null)
    {
        try
        {
            var query = _dbContext.Courses
                .Include(c => c.Department)
                .Where(c => !c.IsDeleted);

            // Применяем фильтры
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(c => 
                    c.Name.ToLower().Contains(lowerSearchTerm) ||
                    c.Code.ToLower().Contains(lowerSearchTerm) ||
                    c.Description.ToLower().Contains(lowerSearchTerm)
                );
            }

            if (departmentUid.HasValue)
            {
                query = query.Where(c => c.DepartmentUid == departmentUid.Value);
            }

            var totalCount = await query.CountAsync();

            var courses = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (courses, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged courses");
            throw;
        }
    }

    /// <summary>
    /// Получает статистику курса (исправленный тип возврата)
    /// </summary>
    public async Task<(int TotalInstances, int ActiveInstances, int TotalEnrollments, int CompletedEnrollments, double CompletionRate, double AverageGrade)> GetCourseStatisticsAsync(Guid courseUid)
    {
        try
        {
            var course = await _dbContext.Subjects
                .Include(s => s.CourseInstances)
                    .ThenInclude(ci => ci.Enrollments)
                .FirstOrDefaultAsync(s => s.Uid == courseUid && !s.IsDeleted);

            if (course == null)
            {
                return (0, 0, 0, 0, 0, 0);
            }

            var totalInstances = course.CourseInstances?.Count ?? 0;
            var activeInstances = course.CourseInstances?.Count(ci => ci.Status == CourseStatus.Active) ?? 0;

            var totalEnrollments = course.CourseInstances?
                .SelectMany(ci => ci.Enrollments)
                .Count() ?? 0;

            var completedEnrollments = course.CourseInstances?
                .SelectMany(ci => ci.Enrollments)
                .Count(e => e.Status == EnrollmentStatus.Completed) ?? 0;

            var completionRate = totalEnrollments > 0 ? (double)completedEnrollments / totalEnrollments : 0;

            // Средняя оценка по всем экземплярам курса
            var averageGrade = 0.0;
            if (totalInstances > 0)
            {
                var grades = await _dbContext.Grades
                    .Where(g => course.CourseInstances!.Select(ci => ci.Uid).Contains(g.Assignment!.CourseInstanceUid))
                    .Select(g => g.Value)
                    .ToListAsync();

                if (grades.Any())
                {
                    averageGrade = (double)grades.Average();
                }
            }

            return (totalInstances, activeInstances, totalEnrollments, completedEnrollments, completionRate, averageGrade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course statistics {CourseUid}", courseUid);
            throw;
        }
    }

    /// <summary>
    /// Получает связанные данные курса
    /// </summary>
    public async Task<CourseAnalytics> GetCourseRelatedDataAsync(Guid courseUid)
    {
        try
        {
            var course = await _dbContext.Courses
                .Include(c => c.CourseInstances)
                    .ThenInclude(ci => ci.Enrollments)
                .Include(c => c.CourseInstances)
                    .ThenInclude(ci => ci.Assignments)
                .FirstOrDefaultAsync(c => c.Uid == courseUid);

            if (course == null)
                throw new ArgumentException($"Course with UID {courseUid} not found");

            var analytics = new CourseAnalytics
            {
                CourseInstanceUid = course.CourseInstances.FirstOrDefault()?.Uid ?? Guid.Empty,
                EngagementLevel = 0, // Будет вычислено позже
                AverageGrade = 0m, // Будет вычислено через отдельный запрос к Grade
                LastCalculated = DateTime.UtcNow
            };

            return analytics;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error getting course related data for {courseUid}: {ex.Message}", nameof(CourseService));
            throw;
        }
    }

    /// <summary>
    /// Получает все курсы (алиас)
    /// </summary>
    public async Task<IEnumerable<Course>> GetCoursesAsync()
    {
        return await GetAllAsync();
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Валидация курса
    /// </summary>
    private async Task ValidateCourseAsync(Course course, bool isCreate)
    {
        var errors = new List<string>();

        // Проверка обязательных полей
        if (string.IsNullOrWhiteSpace(course.Name))
            errors.Add("Course name is required");

        if (string.IsNullOrWhiteSpace(course.Code))
            errors.Add("Course code is required");

        if (course.Credits <= 0)
            errors.Add("Credits must be greater than zero");

        if (course.DurationHours <= 0)
            errors.Add("Duration hours must be greater than zero");

        // Проверка уникальности кода
        if (!string.IsNullOrWhiteSpace(course.Code))
        {
            var codeExists = await _dbContext.Courses
                .Where(c => c.Uid != course.Uid && 
                           c.Code.ToLower() == course.Code.ToLower() && 
                           !c.IsDeleted)
                .AnyAsync();

            if (codeExists)
                errors.Add($"Course with code '{course.Code}' already exists");
        }

        // Проверка существования департамента
        if (course.DepartmentUid.HasValue)
        {
            var departmentExists = await _dbContext.Departments
                .AnyAsync(d => d.Uid == course.DepartmentUid.Value && !d.IsDeleted);

            if (!departmentExists)
                errors.Add($"Department with Uid {course.DepartmentUid.Value} not found");
        }

        if (errors.Any())
        {
            throw new ArgumentException($"Validation failed: {string.Join("; ", errors)}");
        }
    }

    #endregion
}