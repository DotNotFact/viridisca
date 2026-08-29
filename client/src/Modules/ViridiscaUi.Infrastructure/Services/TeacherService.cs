using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.System.Enums;
using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Data;
using ViridiscaUi.Domain.Models;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для работы с преподавателями
/// Независимый сервис без наследования от GenericCrudService
/// </summary>
public class TeacherService : ITeacherService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TeacherService> _logger;

    public TeacherService(ApplicationDbContext dbContext, ILogger<TeacherService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Analytics Methods

    /// <summary>
    /// Получает аналитику преподавателя
    /// </summary>
    public async Task<TeacherAnalytics> GetAnalyticsAsync(Guid teacherUid)
    {
        try
        {
            var analytics = await _dbContext.TeacherAnalytics
                .Include(ta => ta.Teacher)
                    .ThenInclude(t => t!.Person)
                .Include(ta => ta.Teacher)
                    .ThenInclude(t => t!.CourseInstances)
                        .ThenInclude(ci => ci.Enrollments)
                .Include(ta => ta.Teacher)
                    .ThenInclude(t => t!.CourseInstances)
                        .ThenInclude(ci => ci.Assignments)
                .Include(ta => ta.Teacher)
                    .ThenInclude(t => t!.CuratorGroups)
                .FirstOrDefaultAsync(ta => ta.TeacherUid == teacherUid);

            if (analytics == null)
            {
                analytics = new TeacherAnalytics
                {
                    TeacherUid = teacherUid,
                    LastCalculated = DateTime.UtcNow
                };

                _dbContext.TeacherAnalytics.Add(analytics);
                await _dbContext.SaveChangesAsync();

                // Перезагружаем с навигационными свойствами
                analytics = await _dbContext.TeacherAnalytics
                    .Include(ta => ta.Teacher)
                        .ThenInclude(t => t!.Person)
                    .Include(ta => ta.Teacher)
                        .ThenInclude(t => t!.CourseInstances)
                            .ThenInclude(ci => ci.Enrollments)
                    .Include(ta => ta.Teacher)
                        .ThenInclude(t => t!.CourseInstances)
                            .ThenInclude(ci => ci.Assignments)
                    .Include(ta => ta.Teacher)
                        .ThenInclude(t => t!.CuratorGroups)
                    .FirstAsync(ta => ta.Uid == analytics.Uid);
            }

            // Обновляем cached поля если нужно (каждый час)
            if (analytics.LastCalculated < DateTime.UtcNow.AddHours(-1))
            {
                analytics.RefreshCalculatedFields();
                await _dbContext.SaveChangesAsync();
            }

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher analytics {TeacherUid}", teacherUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику преподавателя (исправленный тип возврата)
    /// </summary>
    public async Task<TeacherAnalytics> GetTeacherStatisticsAsync(Guid teacherUid)
    {
        return await GetAnalyticsAsync(teacherUid);
    }

    /// <summary>
    /// Получает информацию о связанных данных преподавателя (заменено на Analytics)
    /// </summary>
    public async Task<TeacherAnalytics> GetRelatedDataInfoAsync(Guid teacherUid)
    {
        return await GetAnalyticsAsync(teacherUid);
    }

    #endregion

    #region Базовые CRUD операции

    /// <summary>
    /// Получает преподавателя по идентификатору
    /// </summary>
    public async Task<Teacher?> GetByUidAsync(Guid uid)
    {
        try
        {
            return await _dbContext.Teachers
                .Include(t => t.Person)
                .Include(t => t.Department)
                .Include(t => t.CourseInstances)
                .FirstOrDefaultAsync(t => t.Uid == uid && !t.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher {TeacherUid}", uid);
            throw;
        }
    }

    /// <summary>
    /// Получает всех преподавателей
    /// </summary>
    public async Task<IEnumerable<Teacher>> GetAllAsync()
    {
        try
        {
            return await _dbContext.Teachers
                .Include(t => t.Person)
                .Include(t => t.Department)
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.Person.LastName)
                .ThenBy(t => t.Person.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all teachers");
            throw;
        }
    }

    /// <summary>
    /// Создает нового преподавателя
    /// </summary>
    public async Task<Teacher> CreateAsync(Teacher teacher)
    {
        ArgumentNullException.ThrowIfNull(teacher);

        try
        {
            // Валидация
            await ValidateTeacherAsync(teacher, true);

            teacher.Uid = Guid.NewGuid();
            teacher.CreatedAt = DateTime.UtcNow;
            teacher.LastModifiedAt = DateTime.UtcNow;

            // Генерация кода сотрудника если не указан
            if (string.IsNullOrWhiteSpace(teacher.EmployeeCode))
            {
                teacher.EmployeeCode = await GenerateEmployeeCodeAsync();
            }

            _dbContext.Teachers.Add(teacher);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created teacher with employee code {EmployeeCode}", teacher.EmployeeCode);
            return teacher;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating teacher");
            throw;
        }
    }

    /// <summary>
    /// Обновляет существующего преподавателя
    /// </summary>
    public async Task<bool> UpdateAsync(Teacher teacher)
    {
        ArgumentNullException.ThrowIfNull(teacher);

        try
        {
            var existingTeacher = await _dbContext.Teachers.FindAsync(teacher.Uid);
            if (existingTeacher == null || existingTeacher.IsDeleted)
                return false;

            // Валидация
            await ValidateTeacherAsync(teacher, false);

            // Обновляем поля
            existingTeacher.PersonUid = teacher.PersonUid;
            existingTeacher.EmployeeCode = teacher.EmployeeCode;
            existingTeacher.DepartmentUid = teacher.DepartmentUid;
            existingTeacher.Salary = teacher.Salary;
            existingTeacher.Qualification = teacher.Qualification;
            existingTeacher.IsActive = teacher.IsActive;
            existingTeacher.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated teacher {TeacherUid}", teacher.Uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating teacher {TeacherUid}", teacher.Uid);
            throw;
        }
    }

    /// <summary>
    /// Удаляет преподавателя
    /// </summary>
    public async Task<bool> DeleteAsync(Guid uid)
    {
        try
        {
            var teacher = await _dbContext.Teachers.FindAsync(uid);
            if (teacher == null || teacher.IsDeleted)
                return false;

            // Проверяем связанные данные
            var hasCourseInstances = await _dbContext.CourseInstances
                .AnyAsync(ci => ci.TeacherUid == uid && !ci.IsDeleted);

            if (hasCourseInstances)
                throw new InvalidOperationException("Cannot delete teacher as they have assigned course instances");

            // Мягкое удаление
            teacher.IsDeleted = true;
            teacher.DeletedAt = DateTime.UtcNow;
            teacher.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted teacher {TeacherUid}", uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting teacher {TeacherUid}", uid);
            throw;
        }
    }

    #endregion

    #region Дополнительные методы для совместимости с ViewModels

    /// <summary>
    /// Получает преподавателя по email Person
    /// </summary>
    public async Task<Teacher?> GetByPersonEmailAsync(string email)
    {
        try
        {
            return await _dbContext.Teachers
                .Include(t => t.Person)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.Person.Email == email && !t.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher by email {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Обновляет преподавателя (алиас для UpdateAsync)
    /// </summary>
    public async Task<bool> UpdateTeacherAsync(Teacher teacher)
    {
        return await UpdateAsync(teacher);
    }

    /// <summary>
    /// Создает преподавателя (алиас для CreateAsync)
    /// </summary>
    public async Task<Teacher> CreateTeacherAsync(Teacher teacher)
    {
        return await CreateAsync(teacher);
    }

    #endregion

    #region Специфичные методы для преподавателей

    /// <summary>
    /// Получает преподавателей по департаменту
    /// </summary>
    public async Task<IEnumerable<Teacher>> GetTeachersByDepartmentAsync(Guid departmentUid)
    {
        try
        {
            return await _dbContext.Teachers
                .Include(t => t.Person)
                .Include(t => t.Department)
                .Where(t => t.DepartmentUid == departmentUid && !t.IsDeleted)
                .OrderBy(t => t.Person.LastName)
                .ThenBy(t => t.Person.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teachers for department {DepartmentUid}", departmentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает активных преподавателей
    /// </summary>
    public async Task<IEnumerable<Teacher>> GetActiveTeachersAsync()
    {
        try
        {
            return await _dbContext.Teachers
                .Include(t => t.Person)
                .Include(t => t.Department)
                .Where(t => t.IsActive && !t.IsDeleted)
                .OrderBy(t => t.Person.LastName)
                .ThenBy(t => t.Person.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active teachers");
            throw;
        }
    }

    /// <summary>
    /// Поиск преподавателей
    /// </summary>
    public async Task<IEnumerable<Teacher>> SearchTeachersAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var lowerSearchTerm = searchTerm.ToLower();

            return await _dbContext.Teachers
                .Include(t => t.Person)
                .Include(t => t.Department)
                .Where(t => !t.IsDeleted && (
                    t.Person.FirstName.ToLower().Contains(lowerSearchTerm) ||
                    t.Person.LastName.ToLower().Contains(lowerSearchTerm) ||
                    t.Person.MiddleName.ToLower().Contains(lowerSearchTerm) ||
                    t.EmployeeCode.ToLower().Contains(lowerSearchTerm) ||
                    t.Qualification.ToLower().Contains(lowerSearchTerm) ||
                    t.Department != null && t.Department.Name.ToLower().Contains(lowerSearchTerm)
                ))
                .OrderBy(t => t.Person.LastName)
                .ThenBy(t => t.Person.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching teachers with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Получает преподавателей с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Teacher> Teachers, int TotalCount)> GetTeachersPagedAsync(
        int page, int pageSize, string? searchTerm = null, Guid? departmentUid = null)
    {
        try
        {
            var query = _dbContext.Teachers
                .Include(t => t.Person)
                .Include(t => t.Department)
                .Where(t => !t.IsDeleted);

            // Применяем фильтры
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(t => 
                    t.Person.FirstName.ToLower().Contains(lowerSearchTerm) ||
                    t.Person.LastName.ToLower().Contains(lowerSearchTerm) ||
                    t.Person.MiddleName.ToLower().Contains(lowerSearchTerm) ||
                    t.EmployeeCode.ToLower().Contains(lowerSearchTerm) ||
                    t.Qualification.ToLower().Contains(lowerSearchTerm)
                );
            }

            if (departmentUid.HasValue)
            {
                query = query.Where(t => t.DepartmentUid == departmentUid.Value);
            }

            var totalCount = await query.CountAsync();

            var teachers = await query
                .OrderBy(t => t.Person.LastName)
                .ThenBy(t => t.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (teachers, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged teachers");
            throw;
        }
    }

    /// <summary>
    /// Получает преподавателя по коду сотрудника
    /// </summary>
    public async Task<Teacher?> GetByEmployeeCodeAsync(string employeeCode)
    {
        if (string.IsNullOrWhiteSpace(employeeCode))
            return null;

        try
        {
            return await _dbContext.Teachers
                .Include(t => t.Person)
                .Include(t => t.Department)
                .FirstOrDefaultAsync(t => t.EmployeeCode == employeeCode && !t.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting teacher by employee code {EmployeeCode}", employeeCode);
            throw;
        }
    }

    /// <summary>
    /// Проверяет существование преподавателя по коду сотрудника
    /// </summary>
    public async Task<bool> ExistsByEmployeeCodeAsync(string employeeCode, Guid? excludeUid = null)
    {
        if (string.IsNullOrWhiteSpace(employeeCode))
            return false;

        try
        {
            var query = _dbContext.Teachers.Where(t => t.EmployeeCode == employeeCode && !t.IsDeleted);

            if (excludeUid.HasValue)
                query = query.Where(t => t.Uid != excludeUid.Value);

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking teacher existence by employee code {EmployeeCode}", employeeCode);
            throw;
        }
    }

    /// <summary>
    /// Получает экземпляры курсов преподавателя
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetTeacherCourseInstancesAsync(Guid teacherUid)
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Group)
                .Include(ci => ci.AcademicPeriod)
                .Where(ci => ci.TeacherUid.HasValue && ci.TeacherUid.Value == teacherUid && !ci.IsDeleted)
                .OrderBy(ci => ci.Subject.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course instances for teacher {TeacherUid}", teacherUid);
            throw;
        }
    }

    /// <summary>
    /// Получает группы преподавателя
    /// </summary>
    public async Task<IEnumerable<Group>> GetTeacherGroupsAsync(Guid teacherUid)
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Group)
                .Where(ci => ci.TeacherUid.HasValue && ci.TeacherUid.Value == teacherUid && !ci.IsDeleted)
                .Select(ci => ci.Group)
                .Distinct()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups for teacher {TeacherUid}", teacherUid);
            throw;
        }
    }

    /// <summary>
    /// Получает студентов преподавателя
    /// </summary>
    public async Task<IEnumerable<Student>> GetTeacherStudentsAsync(Guid teacherUid)
    {
        try
        {
            return await _dbContext.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.Person)
                .Where(e => e.CourseInstance.TeacherUid.HasValue && 
                           e.CourseInstance.TeacherUid.Value == teacherUid && 
                           e.Status == EnrollmentStatus.Enrolled)
                .Select(e => e.Student)
                .Distinct()
                .OrderBy(s => s.Person.LastName)
                .ThenBy(s => s.Person.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting students for teacher {TeacherUid}", teacherUid);
            throw;
        }
    }

    /// <summary>
    /// Получает рабочую нагрузку преподавателя
    /// </summary>
    public async Task<double> GetTeacherWorkloadAsync(Guid teacherUid)
    {
        try
        {
            var activeCourseInstances = await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Where(ci => ci.TeacherUid.HasValue && ci.TeacherUid.Value == teacherUid && ci.IsActive && !ci.IsDeleted)
                .ToListAsync();

            // Рассчитываем нагрузку как сумму кредитов всех активных курсов
            return activeCourseInstances.Sum(ci => ci.Subject?.Credits ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workload for teacher {TeacherUid}", teacherUid);
            return 0;
        }
    }

    /// <summary>
    /// Получает всех преподавателей (алиас)
    /// </summary>
    public async Task<IEnumerable<Teacher>> GetAllTeachersAsync()
    {
        return await GetAllAsync();
    }

    /// <summary>
    /// Получает всех преподавателей (алиас)
    /// </summary>
    public async Task<IEnumerable<Teacher>> GetTeachersAsync()
    {
        return await GetAllAsync();
    }

    /// <summary>
    /// Добавляет нового преподавателя (алиас)
    /// </summary>
    public async Task AddTeacherAsync(Teacher teacher)
    {
        await CreateAsync(teacher);
    }

    /// <summary>
    /// Назначает преподавателя на курс
    /// </summary>
    public async Task<bool> AssignToCourseAsync(Guid teacherUid, Guid courseUid)
    {
        try
        {
            // Находим экземпляры курса, которые можно назначить преподавателю
            var courseInstances = await _dbContext.CourseInstances
                .Where(ci => ci.SubjectUid == courseUid && ci.TeacherUid == null && !ci.IsDeleted)
                .ToListAsync();

            if (!courseInstances.Any())
                return false;

            // Назначаем преподавателя на первый доступный экземпляр
            var courseInstance = courseInstances.First();
            courseInstance.TeacherUid = teacherUid;
            courseInstance.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Assigned teacher {TeacherUid} to course instance {CourseInstanceUid}", teacherUid, courseInstance.Uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning teacher {TeacherUid} to course {CourseUid}", teacherUid, courseUid);
            return false;
        }
    }

    /// <summary>
    /// Получает курируемые группы
    /// </summary>
    public async Task<IEnumerable<Group>> GetCuratedGroupsAsync(Guid teacherUid)
    {
        try
        {
            // Группы, которые курирует преподаватель (где он назначен куратором)
            return await _dbContext.Groups
                .Where(g => g.CuratorUid == teacherUid && !g.IsDeleted)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting curated groups for teacher {TeacherUid}", teacherUid);
            throw;
        }
    }

    /// <summary>
    /// Экспортирует данные преподавателей
    /// </summary>
    public async Task<string> ExportTeachersAsync(IEnumerable<Teacher> teachers, string format = "xlsx")
    {
        try
        {
            // Простая реализация - возвращаем CSV строку
            var lines = new List<string>
            {
                "Employee Code,First Name,Last Name,Middle Name,Department,Qualification,Salary,Is Active"
            };

            foreach (var teacher in teachers)
            {
                var line = $"{teacher.EmployeeCode},{teacher.Person?.FirstName},{teacher.Person?.LastName},{teacher.Person?.MiddleName},{teacher.Department?.Name},{teacher.Qualification},{teacher.Salary},{teacher.IsActive}";
                lines.Add(line);
            }

            return string.Join(Environment.NewLine, lines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting teachers");
            throw;
        }
    }

    /// <summary>
    /// Назначает преподавателя куратором группы
    /// </summary>
    public async Task<bool> AssignToGroupAsync(Guid teacherUid, Guid groupUid)
    {
        try
        {
            var group = await _dbContext.Groups.FindAsync(groupUid);
            if (group == null || group.IsDeleted)
                return false;

            group.CuratorUid = teacherUid;
            group.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Assigned teacher {TeacherUid} as curator to group {GroupUid}", teacherUid, groupUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning teacher {TeacherUid} to group {GroupUid}", teacherUid, groupUid);
            return false;
        }
    }

    /// <summary>
    /// Отменяет назначение преподавателя куратором группы
    /// </summary>
    public async Task<bool> UnassignFromGroupAsync(Guid teacherUid, Guid groupUid)
    {
        try
        {
            var group = await _dbContext.Groups
                .FirstOrDefaultAsync(g => g.Uid == groupUid && g.CuratorUid == teacherUid && !g.IsDeleted);

            if (group == null)
                return false;

            group.CuratorUid = null;
            group.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Unassigned teacher {TeacherUid} from group {GroupUid}", teacherUid, groupUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning teacher {TeacherUid} from group {GroupUid}", teacherUid, groupUid);
            return false;
        }
    }

    /// <summary>
    /// Отменяет назначение преподавателя на курс
    /// </summary>
    public async Task<bool> UnassignFromCourseAsync(Guid teacherUid, Guid courseUid)
    {
        try
        {
            var courseInstances = await _dbContext.CourseInstances
                .Where(ci => ci.SubjectUid == courseUid && ci.TeacherUid == teacherUid && !ci.IsDeleted)
                .ToListAsync();

            if (!courseInstances.Any())
                return false;

            foreach (var courseInstance in courseInstances)
            {
                courseInstance.TeacherUid = null;
                courseInstance.LastModifiedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Unassigned teacher {TeacherUid} from {CourseInstancesCount} course instances", teacherUid, courseInstances.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning teacher {TeacherUid} from course {CourseUid}", teacherUid, courseUid);
            return false;
        }
    }

    /// <summary>
    /// Получает доступных кураторов
    /// </summary>
    public async Task<IEnumerable<Teacher>> GetAvailableCuratorsAsync()
    {
        try
        {
            return await _dbContext.Teachers
                .Include(t => t.Person)
                .Include(t => t.Department)
                .Where(t => t.IsActive && !t.IsDeleted)
                .OrderBy(t => t.Person.LastName)
                .ThenBy(t => t.Person.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available curators");
            throw;
        }
    }

    /// <summary>
    /// Получает доступных кураторов для группы
    /// </summary>
    public async Task<IEnumerable<Teacher>> GetAvailableCuratorsForGroupAsync(Guid groupUid)
    {
        try
        {
            // Возвращаем всех активных преподавателей
            return await GetAvailableCuratorsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available curators for group {GroupUid}", groupUid);
            throw;
        }
    }

    /// <summary>
    /// Проверяет существование преподавателя по email
    /// </summary>
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await ExistsByEmailAsync(email, default);
    }

    /// <summary>
    /// Проверяет существование преподавателя по email (с исключением)
    /// </summary>
    public async Task<bool> ExistsByEmailAsync(string email, Guid excludeUid = default)
    {
        try
        {
            var person = await _dbContext.Persons
                .FirstOrDefaultAsync(p => p.Email == email);

            if (person == null)
                return false;

            var teacher = await _dbContext.Teachers
                .FirstOrDefaultAsync(t => t.PersonUid == person.Uid && t.Uid != excludeUid);

            return teacher != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if teacher exists by email {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Получает преподавателя по идентификатору (алиас)
    /// </summary>
    public async Task<Teacher?> GetTeacherAsync(Guid uid)
    {
        return await GetByUidAsync(uid);
    }

    /// <summary>
    /// Получает преподавателей с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Teacher> teachers, int totalCount)> GetPagedAsync(int page, int pageSize, string? searchQuery = null)
    {
        try
        {
            var query = _dbContext.Teachers
                .Include(t => t.Person)
                .Include(t => t.Department)
                .Where(t => !t.IsDeleted);

            // Применяем поиск если указан
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchLower = searchQuery.ToLower();
                query = query.Where(t => 
                    t.Person.FirstName.ToLower().Contains(searchLower) ||
                    t.Person.LastName.ToLower().Contains(searchLower) ||
                    t.EmployeeCode.ToLower().Contains(searchLower) ||
                    t.Department != null && t.Department.Name.ToLower().Contains(searchLower));
            }

            var totalCount = await query.CountAsync();
            
            var teachers = await query
                .OrderBy(t => t.Person.LastName)
                .ThenBy(t => t.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (teachers, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged teachers");
            throw;
        }
    }

    /// <summary>
    /// Удаляет преподавателя (алиас для DeleteAsync)
    /// </summary>
    public async Task<bool> DeleteTeacherAsync(Guid uid)
    {
        return await DeleteAsync(uid);
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Валидация преподавателя
    /// </summary>
    private async Task ValidateTeacherAsync(Teacher teacher, bool isCreate)
    {
        var errors = new List<string>();

        // Проверка обязательных полей
        if (teacher.PersonUid == Guid.Empty)
            errors.Add("Person is required for teacher");

        if (string.IsNullOrWhiteSpace(teacher.EmployeeCode))
            errors.Add("Employee code is required");

        // Проверка существования Person
        if (teacher.PersonUid != Guid.Empty)
        {
            var personExists = await _dbContext.Persons
                .AnyAsync(p => p.Uid == teacher.PersonUid && !p.IsDeleted);

            if (!personExists)
                errors.Add($"Person with Uid {teacher.PersonUid} not found");
        }

        // Проверка существования Department
        if (teacher.DepartmentUid.HasValue && teacher.DepartmentUid != Guid.Empty)
        {
            var departmentExists = await _dbContext.Departments
                .AnyAsync(d => d.Uid == teacher.DepartmentUid.Value && !d.IsDeleted);

            if (!departmentExists)
                errors.Add($"Department with Uid {teacher.DepartmentUid.Value} not found");
        }

        // Проверка уникальности кода сотрудника
        if (!string.IsNullOrWhiteSpace(teacher.EmployeeCode))
        {
            var codeExists = await _dbContext.Teachers
                .Where(t => t.Uid != teacher.Uid && 
                           t.EmployeeCode.ToLower() == teacher.EmployeeCode.ToLower() && 
                           !t.IsDeleted)
                .AnyAsync();

            if (codeExists)
                errors.Add($"Teacher with employee code '{teacher.EmployeeCode}' already exists");
        }

        // Проверка уникальности Person (один Person не может быть преподавателем дважды)
        if (teacher.PersonUid != Guid.Empty)
        {
            var personAlreadyTeacher = await _dbContext.Teachers
                .Where(t => t.Uid != teacher.Uid && 
                           t.PersonUid == teacher.PersonUid && 
                           !t.IsDeleted)
                .AnyAsync();

            if (personAlreadyTeacher)
                errors.Add("This person is already registered as a teacher");
        }

        if (errors.Any())
        {
            throw new ArgumentException($"Validation failed: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Генерирует код сотрудника
    /// </summary>
    private async Task<string> GenerateEmployeeCodeAsync()
    {
        var currentYear = DateTime.Now.Year;
        var prefix = $"EMP{currentYear}";

        // Находим последний номер для текущего года
        var lastTeacher = await _dbContext.Teachers
            .Where(t => t.EmployeeCode.StartsWith(prefix) && !t.IsDeleted)
            .OrderByDescending(t => t.EmployeeCode)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastTeacher != null && lastTeacher.EmployeeCode.Length > prefix.Length)
        {
            var numberPart = lastTeacher.EmployeeCode.Substring(prefix.Length);
            if (int.TryParse(numberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}"; // EMP2024XXXX
    }

    #endregion
}