using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Infrastructure.Data;
using ViridiscaUi.Domain.Models;
using ViridiscaUi.Infrastructure.Logger;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для работы с предметами
/// </summary>
public class SubjectService : ISubjectService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SubjectService> _logger;

    public SubjectService(ApplicationDbContext dbContext, ILogger<SubjectService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region ISubjectService Implementation

    /// <summary>
    /// Получает все предметы
    /// </summary>
    public async Task<IEnumerable<Subject>> GetAllAsync()
    {
        return await GetAllSubjectsAsync();
    }

    /// <summary>
    /// Получает предмет по ID
    /// </summary>
    public async Task<Subject?> GetByIdAsync(Guid subjectUid)
    {
        return await GetSubjectAsync(subjectUid);
    }

    /// <summary>
    /// Получает предмет по UID (псевдоним для GetByIdAsync)
    /// </summary>
    public async Task<Subject?> GetByUidAsync(Guid subjectUid)
    {
        return await GetByIdAsync(subjectUid);
    }

    /// <summary>
    /// Создает новый предмет
    /// </summary>
    public async Task<Subject> CreateAsync(Subject subject)
    {
        return await CreateSubjectAsync(subject);
    }

    /// <summary>
    /// Обновляет предмет
    /// </summary>
    public async Task<Subject> UpdateAsync(Subject subject)
    {
        try
        {
            _dbContext.Subjects.Update(subject);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Обновлен предмет {SubjectName} с ID {SubjectUid}", subject.Name, subject.Uid);
            return subject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении предмета {SubjectUid}", subject.Uid);
            throw;
        }
    }

    /// <summary>
    /// Удаляет предмет
    /// </summary>
    public async Task<bool> DeleteAsync(Guid subjectUid)
    {
        return await DeleteSubjectAsync(subjectUid);
    }

    /// <summary>
    /// Получает предметы по департаменту
    /// </summary>
    public async Task<IEnumerable<Subject>> GetByDepartmentAsync(Guid departmentUid)
    {
        return await GetSubjectsByDepartmentAsync(departmentUid);
    }

    /// <summary>
    /// Поиск предметов
    /// </summary>
    public async Task<IEnumerable<Subject>> SearchAsync(string searchTerm)
    {
        return await SearchSubjectsAsync(searchTerm);
    }

    /// <summary>
    /// Валидирует предмет
    /// </summary>
    public async Task<ValidationResult> ValidateAsync(Subject subject)
    {
        var result = new ValidationResult();

        try
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(subject.Name))
                result.AddError("Название предмета обязательно для заполнения");

            if (string.IsNullOrWhiteSpace(subject.Code))
                result.AddError("Код предмета обязателен для заполнения");

            if (subject.Credits <= 0)
                result.AddError("Количество кредитов должно быть больше нуля");

            // Проверка уникальности кода
            var existingByCode = await _dbContext.Subjects
                .FirstOrDefaultAsync(s => s.Code == subject.Code && s.Uid != subject.Uid);
            
            if (existingByCode != null)
                result.AddError($"Предмет с кодом '{subject.Code}' уже существует");

            // Проверка уникальности названия
            var existingByName = await _dbContext.Subjects
                .FirstOrDefaultAsync(s => s.Name == subject.Name && s.Uid != subject.Uid);
            
            if (existingByName != null)
                result.AddError($"Предмет с названием '{subject.Name}' уже существует");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при валидации предмета");
            result.AddError("Ошибка при валидации предмета");
            return result;
        }
    }

    /// <summary>
    /// Получает статистику предмета
    /// </summary>
    public async Task<(int TotalCourseInstances, int ActiveCourseInstances, int TotalStudents, decimal AverageGrade, decimal PassRate, int TotalAssignments)> GetSubjectStatisticsAsync(Guid subjectUid)
    {
        try
        {
            var totalCourseInstances = await _dbContext.CourseInstances
                .CountAsync(ci => ci.SubjectUid == subjectUid);

            var activeCourseInstances = await _dbContext.CourseInstances
                .CountAsync(ci => ci.SubjectUid == subjectUid && ci.Status == CourseStatus.Active);

            var totalStudents = await _dbContext.Enrollments
                .Where(e => e.CourseInstance!.SubjectUid == subjectUid)
                .Select(e => e.StudentUid)
                .Distinct()
                .CountAsync();

            var grades = await _dbContext.Grades
                .Where(g => g.CourseInstance!.SubjectUid == subjectUid && g.Value > 0)
                .Select(g => g.Value)
                .ToListAsync();

            var averageGrade = grades.Any() ? grades.Average() : 0m;
            var passRate = grades.Any() ? (decimal)grades.Count(g => g >= 60) / grades.Count * 100 : 0m;

            var totalAssignments = await _dbContext.Assignments
                .CountAsync(a => a.CourseInstance!.SubjectUid == subjectUid);

            return (totalCourseInstances, activeCourseInstances, totalStudents, averageGrade, passRate, totalAssignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении статистики предмета {SubjectUid}", subjectUid);
            return (0, 0, 0, 0m, 0m, 0);
        }
    }

    /// <summary>
    /// Получает информацию о связанных данных предмета для безопасного удаления
    /// </summary>
    public async Task<SubjectAnalytics> GetSubjectRelatedDataAsync(Guid subjectUid)
    {
        try
        {
            var subject = await _dbContext.Subjects
                .Include(s => s.CourseInstances)
                .ThenInclude(ci => ci.Enrollments)
                .FirstOrDefaultAsync(s => s.Uid == subjectUid);

            if (subject == null)
            {
                return new SubjectAnalytics
                {
                    SubjectUid = subjectUid,
                    LastCalculated = DateTime.UtcNow
                };
            }

            var analytics = new SubjectAnalytics
            {
                SubjectUid = subjectUid,
                TotalCourseInstances = subject.CourseInstances.Count,
                ActiveCourseInstances = subject.CourseInstances.Count(ci => ci.IsActive),
                TotalStudentsEnrolled = subject.CourseInstances.SelectMany(ci => ci.Enrollments).Count(),
                CompletionRate = subject.CourseInstances.SelectMany(ci => ci.Enrollments).Any() 
                    ? (decimal)subject.CourseInstances.SelectMany(ci => ci.Enrollments).Count(e => e.Status == EnrollmentStatus.Completed) / subject.CourseInstances.SelectMany(ci => ci.Enrollments).Count() * 100
                    : 0m,
                AverageGrade = 0m, // Будет вычислено через отдельный запрос к Grade
                LastCalculated = DateTime.UtcNow
            };

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get related data for subject {SubjectUid}", subjectUid);
            return new SubjectAnalytics
            {
                SubjectUid = subjectUid,
                LastCalculated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Получает расширенные данные предмета
    /// </summary>
    public async Task<SubjectAnalytics> GetSubjectExtendedDataAsync(Guid subjectUid)
    {
        // Возвращаем те же данные, что и GetSubjectRelatedDataAsync
        return await GetSubjectRelatedDataAsync(subjectUid);
    }

    /// <summary>
    /// Получает информацию о связанных данных предмета (упрощенная версия)
    /// </summary>
    public async Task<SubjectAnalytics> GetSubjectRelatedDataInfoAsync(Guid subjectUid)
    {
        try
        {
            var subject = await _dbContext.Subjects
                .Include(s => s.CourseInstances)
                .ThenInclude(ci => ci.Enrollments)
                .FirstOrDefaultAsync(s => s.Uid == subjectUid);

            if (subject == null)
            {
                return new SubjectAnalytics
                {
                    SubjectUid = subjectUid,
                    LastCalculated = DateTime.UtcNow
                };
            }

            var analytics = new SubjectAnalytics
            {
                SubjectUid = subjectUid,
                TotalCourseInstances = subject.CourseInstances.Count,
                ActiveCourseInstances = subject.CourseInstances.Count(ci => ci.IsActive),
                TotalStudentsEnrolled = subject.CourseInstances.SelectMany(ci => ci.Enrollments).Count(),
                CompletionRate = subject.CourseInstances.SelectMany(ci => ci.Enrollments).Any() 
                    ? (decimal)subject.CourseInstances.SelectMany(ci => ci.Enrollments).Count(e => e.Status == EnrollmentStatus.Completed) / subject.CourseInstances.SelectMany(ci => ci.Enrollments).Count() * 100
                    : 0m,
                AverageGrade = 0m, // Будет вычислено через отдельный запрос к Grade
                LastCalculated = DateTime.UtcNow
            };

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get related data for subject {SubjectUid}", subjectUid);
            return new SubjectAnalytics
            {
                SubjectUid = subjectUid,
                LastCalculated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Получает задания предмета
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetSubjectAssignmentsAsync(Guid subjectUid)
    {
        try
        {
            return await _dbContext.Assignments
                .Where(a => a.CourseInstance != null && a.CourseInstance.SubjectUid == subjectUid)
                .Include(a => a.CourseInstance)
                .ThenInclude(ci => ci.Subject)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subject assignments: {SubjectUid}", subjectUid);
            return [];
        }
    }

    #endregion

    public async Task<IEnumerable<Subject>> GetAllSubjectsAsync()
    {
        try
        {
            var subjects = await _dbContext.Subjects
                .OrderBy(s => s.Name)
                .ToListAsync();

            return subjects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении всех предметов");
            return [];
        }
    }

    public async Task<Subject?> GetSubjectAsync(Guid uid)
    {
        try
        {
            var subject = await _dbContext.Subjects
                .FirstOrDefaultAsync(s => s.Uid == uid);

            return subject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении предмета {SubjectUid}", uid);
            return null;
        }
    }

    public async Task<Subject?> GetSubjectByUidAsync(Guid uid)
    {
        return await GetSubjectAsync(uid);
    }

    public async Task<IEnumerable<Subject>> GetSubjectsByTeacherAsync(Guid teacherUid)
    {
        try
        {
            // Используем CourseInstances для получения предметов преподавателя
            var subjects = await _dbContext.CourseInstances
                .Where(ci => ci.TeacherUid == teacherUid)
                .Select(ci => ci.Subject)
                .Distinct()
                .ToListAsync();

            return subjects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении предметов преподавателя {TeacherUid}", teacherUid);
            return [];
        }
    }

    public async Task<Subject> CreateSubjectAsync(Subject subject)
    {
        try
        {
            _dbContext.Subjects.Add(subject);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Создан предмет {SubjectName} с ID {SubjectUid}", subject.Name, subject.Uid);
            return subject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании предмета {SubjectName}", subject.Name);
            throw;
        }
    }

    public async Task AddSubjectAsync(Subject subject)
    {
        try
        {
            _dbContext.Subjects.Add(subject);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Добавлен предмет {SubjectName} с ID {SubjectUid}", subject.Name, subject.Uid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении предмета {SubjectName}", subject.Name);
            throw;
        }
    }

    public async Task<bool> UpdateSubjectAsync(Subject subject)
    {
        try
        {
            _dbContext.Subjects.Update(subject);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Обновлен предмет {SubjectName} с ID {SubjectUid}", subject.Name, subject.Uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении предмета {SubjectUid}", subject.Uid);
            return false;
        }
    }

    public async Task<bool> DeleteSubjectAsync(Guid subjectUid)
    {
        try
        {
            var relatedData = await GetSubjectRelatedDataAsync(subjectUid);

            // Проверяем связанные данные по количеству экземпляров курсов
            if (relatedData.TotalCourseInstances > 0)
            {
                throw new InvalidOperationException($"Cannot delete subject because it has {relatedData.TotalCourseInstances} course instances");
            }

            // Проверяем связанные данные по количеству заданий
            if (relatedData.TotalAssignments > 0)
            {
                throw new InvalidOperationException($"Cannot delete subject because it has {relatedData.TotalAssignments} assignments");
            }

            // Если нет связанных данных, можно удалять
            return !relatedData.TotalCourseInstances.Equals(0) && !relatedData.TotalAssignments.Equals(0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if subject can be deleted {SubjectUid}", subjectUid);
            return false;
        }
    }

    public async Task<IEnumerable<Subject>> GetSubjectsByDepartmentAsync(Guid departmentUid)
    {
        try
        {
            var subjects = await _dbContext.Subjects
                .Where(s => s.DepartmentUid == departmentUid)
                .ToListAsync();

            return subjects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении предметов департамента {DepartmentUid}", departmentUid);
            return [];
        }
    }

    public async Task<IEnumerable<Subject>> GetActiveSubjectsAsync()
    {
        try
        {
            var subjects = await _dbContext.Subjects
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return subjects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении активных предметов");
            return [];
        }
    }

    public async Task<IEnumerable<Subject>> SearchSubjectsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllSubjectsAsync();
            }

            var lowerSearchTerm = searchTerm.ToLower();

            return await _dbContext.Subjects
                .Where(s => 
                    s.Name.ToLower().Contains(lowerSearchTerm) ||
                    s.Code.ToLower().Contains(lowerSearchTerm) ||
                    s.Description.ToLower().Contains(lowerSearchTerm))
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске предметов с термином: {SearchTerm}", searchTerm);
            return [];
        }
    }

    public async Task<(IEnumerable<Subject> Subjects, int TotalCount)> GetSubjectsPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null, 
        bool? isActive = null, 
        Guid? departmentUid = null)
    {
        try
        {
            var query = _dbContext.Subjects.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(s => 
                    s.Name.ToLower().Contains(lowerSearchTerm) ||
                    s.Code.ToLower().Contains(lowerSearchTerm) ||
                    s.Description.ToLower().Contains(lowerSearchTerm));
            }

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            if (departmentUid.HasValue)
            {
                query = query.Where(s => s.DepartmentUid == departmentUid.Value);
            }

            var totalCount = await query.CountAsync();

            var subjects = await query
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (subjects, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении предметов с пагинацией");
            return ([], 0);
        }
    }

    public async Task<bool> ExistsByCodeAsync(string code, Guid? excludeUid = null)
    {
        try
        {
            return await _dbContext.Subjects
                .AnyAsync(s => s.Code == code && s.Uid != excludeUid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке существования кода предмета: {Code}", code);
            return false;
        }
    }

    public async Task<bool> SetSubjectActiveStatusAsync(Guid uid, bool isActive)
    {
        try
        {
            var subject = await _dbContext.Subjects.FindAsync(uid);
            if (subject == null)
            {
                _logger.LogWarning("Предмет с ID {SubjectUid} не найден для обновления статуса", uid);
                return false;
            }

            subject.IsActive = isActive;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Статус предмета обновлен: {SubjectName} - Активен: {IsActive}", 
                subject.Name, isActive);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении статуса предмета: {SubjectUid}", uid);
            return false;
        }
    }

    /// <summary>
    /// Создает тестовые данные для предметов
    /// </summary>
    public async Task SeedTestDataAsync()
    {
        try
        {
            if (await _dbContext.Subjects.AnyAsync())
            {
                return; // Данные уже существуют
            }

            var department = await _dbContext.Departments.FirstOrDefaultAsync();

            var subjects = new[]
            {
                new Subject("CS101", "Программирование на C#", "Основы программирования на языке C#", 4, SubjectType.Required, "Программирование", department?.Uid),
                new Subject("DB101", "Базы данных", "Проектирование и работа с базами данных", 3, SubjectType.Required, "Базы данных", department?.Uid),
                new Subject("WEB101", "Веб-разработка", "Создание веб-приложений", 4, SubjectType.Specialized, "Веб-технологии", department?.Uid),
                new Subject("MATH101", "Математический анализ", "Основы математического анализа", 5, SubjectType.Required, "Математика", department?.Uid),
                new Subject("ENG101", "Английский язык", "Английский язык для IT специалистов", 2, SubjectType.Elective, "Языки", department?.Uid)
            };

            await _dbContext.Subjects.AddRangeAsync(subjects);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Test subjects data seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding test subjects data");
            throw;
        }
    }

    private async Task<IEnumerable<Subject>> GetSubjectsWithStatisticsAsync(string? status)
    {
        try
        {
            var query = _dbContext.Subjects.AsQueryable();

            // Фильтрация по статусу через CourseInstances
            if (!string.IsNullOrEmpty(status))
            {
                // Временная заглушка - нужно определить как фильтровать по статусу
                query = query.Where(s => s.IsActive);
            }

            var subjects = await query
                .OrderBy(s => s.Name)
                .ToListAsync();

            return subjects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении предметов со статистикой");
            return [];
        }
    }

    /// <summary>
    /// Получает предметы с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Subject> Subjects, int TotalCount)> GetPagedAsync(
        int page = 1, 
        int pageSize = 20, 
        string? searchTerm = null, 
        Guid? departmentUid = null)
    {
        var query = _dbContext.Subjects.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(s => s.Name.Contains(searchTerm) || s.Code.Contains(searchTerm));
        }

        if (departmentUid.HasValue)
        {
            query = query.Where(s => s.DepartmentUid == departmentUid.Value);
        }

        var totalCount = await query.CountAsync();
        var subjects = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (subjects, totalCount);
    }

    /// <summary>
    /// Получает предмет по коду
    /// </summary>
    public async Task<Subject?> GetByCodeAsync(string code)
    {
        return await _dbContext.Subjects
            .FirstOrDefaultAsync(s => s.Code == code);
    }

    /// <summary>
    /// Получает количество экземпляров курсов для предмета
    /// </summary>
    public async Task<int> GetCourseInstancesCountAsync(Guid subjectUid)
    {
        return await _dbContext.CourseInstances
            .CountAsync(ci => ci.SubjectUid == subjectUid);
    }

    /// <summary>
    /// Получает количество предметов в учебных планах
    /// </summary>
    public async Task<int> GetCurriculumSubjectsCountAsync(Guid subjectUid)
    {
        return await _dbContext.CurriculumSubjects
            .CountAsync(cs => cs.SubjectUid == subjectUid);
    }

    /// <summary>
    /// Получает предмет по названию
    /// </summary>
    public async Task<Subject?> GetByNameAsync(string name)
    {
        try
        {
            return await _dbContext.Subjects
                .FirstOrDefaultAsync(s => s.Name == name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subject by name: {Name}", name);
            return null;
        }
    }

    /// <summary>
    /// Получает общее количество предметов
    /// </summary>
    public async Task<int> GetTotalCountAsync()
    {
        try
        {
            return await _dbContext.Subjects.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total subjects count");
            return 0;
        }
    }

    /// <summary>
    /// Получает курсы предмета
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetSubjectCoursesAsync(Guid subjectUid)
    {
        try
        {
            return await _dbContext.CourseInstances
                .Where(ci => ci.SubjectUid == subjectUid)
                .Include(ci => ci.Subject)
                .Include(ci => ci.Teacher)
                .Include(ci => ci.Group)
                .Include(ci => ci.AcademicPeriod)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subject courses: {SubjectUid}", subjectUid);
            return [];
        }
    }

    /// <summary>
    /// Получает учебные планы предмета
    /// </summary>
    public async Task<IEnumerable<Curriculum>> GetSubjectCurriculaAsync(Guid subjectUid)
    {
        try
        {
            return await _dbContext.CurriculumSubjects
                .Where(cs => cs.SubjectUid == subjectUid)
                .Select(cs => cs.Curriculum)
                .Distinct()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subject curricula: {SubjectUid}", subjectUid);
            return [];
        }
    }
} 