using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.System.Enums;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Data;
using System.Collections.ObjectModel;
using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Models;
using ViridiscaUi.Domain.Entities.Analytics;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для работы с учебными группами
/// Независимый сервис без наследования от GenericCrudService
/// </summary>
public class GroupService : IGroupService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GroupService> _logger;

    public GroupService(ApplicationDbContext dbContext, ILogger<GroupService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Analytics Methods

    /// <summary>
    /// Получает аналитику группы
    /// </summary>
    public async Task<GroupAnalytics> GetAnalyticsAsync(Guid groupUid)
    {
        try
        {
            var analytics = await _dbContext.GroupAnalytics
                .Include(ga => ga.Group)
                    .ThenInclude(g => g!.Students)
                        .ThenInclude(s => s.Person)
                .Include(ga => ga.Group)
                    .ThenInclude(g => g!.Students)
                        .ThenInclude(s => s.Grades)
                .Include(ga => ga.Group)
                    .ThenInclude(g => g!.Students)
                        .ThenInclude(s => s.Enrollments)
                .Include(ga => ga.Group)
                    .ThenInclude(g => g!.Curator)
                .FirstOrDefaultAsync(ga => ga.GroupUid == groupUid);

            if (analytics == null)
            {
                analytics = new GroupAnalytics
                {
                    GroupUid = groupUid,
                    LastCalculated = DateTime.UtcNow
                };

                _dbContext.GroupAnalytics.Add(analytics);
                await _dbContext.SaveChangesAsync();

                // Перезагружаем с навигационными свойствами
                analytics = await _dbContext.GroupAnalytics
                    .Include(ga => ga.Group)
                        .ThenInclude(g => g!.Students)
                            .ThenInclude(s => s.Person)
                    .Include(ga => ga.Group)
                        .ThenInclude(g => g!.Students)
                            .ThenInclude(s => s.Grades)
                    .Include(ga => ga.Group)
                        .ThenInclude(g => g!.Students)
                            .ThenInclude(s => s.Enrollments)
                    .Include(ga => ga.Group)
                        .ThenInclude(g => g!.Curator)
                    .FirstAsync(ga => ga.Uid == analytics.Uid);
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
            _logger.LogError(ex, "Error getting group analytics {GroupUid}", groupUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику группы (исправленный тип возврата)
    /// </summary>
    public async Task<GroupAnalytics> GetGroupStatisticsAsync(Guid groupUid)
    {
        return await GetAnalyticsAsync(groupUid);
    }

    /// <summary>
    /// Получает информацию о связанных данных группы (заменено на Analytics)
    /// </summary>
    public async Task<GroupAnalytics> GetRelatedDataInfoAsync(Guid groupUid)
    {
        return await GetAnalyticsAsync(groupUid);
    }

    #endregion

    #region Базовые CRUD операции

    /// <summary>
    /// Получает все группы (стандартный метод)
    /// </summary>
    public async Task<IEnumerable<Group>> GetAllAsync()
    {
        var groups = await GetAllGroupsAsync();
        return groups;
    }

    /// <summary>
    /// Получает группу по идентификатору (стандартный метод)
    /// </summary>
    public async Task<Group?> GetByUidAsync(Guid uid)
    {
        return await GetGroupAsync(uid);
    }

    /// <summary>
    /// Создает новую группу (стандартный метод)
    /// </summary>
    public async Task<Group> CreateAsync(Group group)
    {
        return await CreateGroupAsync(group);
    }

    /// <summary>
    /// Обновляет группу (стандартный метод)
    /// </summary>
    public async Task<bool> UpdateAsync(Group group)
    {
        return await UpdateGroupAsync(group);
    }

    /// <summary>
    /// Удаляет группу (стандартный метод)
    /// </summary>
    public async Task<bool> DeleteAsync(Guid uid)
    {
        return await DeleteGroupAsync(uid);
    }

    /// <summary>
    /// Получает группу по идентификатору
    /// </summary>
    public async Task<Group?> GetGroupAsync(Guid uid)
    {
        try
        {
            return await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                    .ThenInclude(c => c.Person)
                .FirstOrDefaultAsync(g => g.Uid == uid && !g.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupUid}", uid);
            throw;
        }
    }

    /// <summary>
    /// Получает все группы
    /// </summary>
    public async Task<IReadOnlyList<Group>> GetAllGroupsAsync()
    {
        try
        {
            var groups = await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                    .ThenInclude(c => c.Person)
                .Where(g => !g.IsDeleted)
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Name)
                .ToListAsync();

            return groups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all groups");
            throw;
        }
    }

    /// <summary>
    /// Получает группы (синоним для GetAllGroupsAsync)
    /// </summary>
    public async Task<IReadOnlyList<Group>> GetGroupsAsync()
    {
        return await GetAllGroupsAsync();
    }

    /// <summary>
    /// Создает новую группу
    /// </summary>
    public async Task<Group> CreateGroupAsync(Group group)
    {
        ArgumentNullException.ThrowIfNull(group);

        try
        {
            // Валидация
            await ValidateGroupAsync(group, true);

            // Генерируем код группы если не указан
            if (string.IsNullOrEmpty(group.Code))
            {
                group.Code = await GenerateGroupCodeAsync(group.Year);
            }

            group.Uid = Guid.NewGuid();
            group.CreatedAt = DateTime.UtcNow;
            group.LastModifiedAt = DateTime.UtcNow;

            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created group {GroupName} with code {GroupCode}", group.Name, group.Code);
            return group;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group {GroupName}", group.Name);
            throw;
        }
    }

    /// <summary>
    /// Добавляет группу (синоним для CreateGroupAsync)
    /// </summary>
    public async Task AddGroupAsync(Group group)
    {
        await CreateGroupAsync(group);
    }

    /// <summary>
    /// Обновляет существующую группу
    /// </summary>
    public async Task<bool> UpdateGroupAsync(Group group)
    {
        ArgumentNullException.ThrowIfNull(group);

        try
        {
            var existingGroup = await _dbContext.Groups.FindAsync(group.Uid);
            if (existingGroup == null || existingGroup.IsDeleted)
                return false;

            // Валидация
            await ValidateGroupAsync(group, false);

            // Обновляем поля
            existingGroup.Name = group.Name;
            existingGroup.Code = group.Code;
            existingGroup.Description = group.Description;
            existingGroup.Year = group.Year;
            existingGroup.MaxStudents = group.MaxStudents;
            existingGroup.CuratorUid = group.CuratorUid;
            existingGroup.DepartmentUid = group.DepartmentUid;
            existingGroup.Status = group.Status;
            existingGroup.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated group {GroupName}", group.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating group {GroupUid}", group.Uid);
            throw;
        }
    }

    /// <summary>
    /// Удаляет группу
    /// </summary>
    public async Task<bool> DeleteGroupAsync(Guid uid)
    {
        try
        {
            var group = await _dbContext.Groups.FindAsync(uid);
            if (group == null || group.IsDeleted)
                return false;

            // Проверяем связанные данные
            var relatedData = await GetGroupRelatedDataInfoAsync(uid);
            if (relatedData.TotalStudents > 0)
            {
                throw new InvalidOperationException($"Cannot delete group because it has {relatedData.TotalStudents} students");
            }

            group.IsDeleted = true;
            group.DeletedAt = DateTime.UtcNow;
            group.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted group {GroupUid}", uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting group {GroupUid}", uid);
            throw;
        }
    }

    #endregion

    #region Специфичные методы для групп

    /// <summary>
    /// Получает группы по курсу
    /// </summary>
    public async Task<IReadOnlyList<Group>> GetGroupsByCourseAsync(int course)
    {
        try
        {
            var groups = await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                    .ThenInclude(c => c.Person)
                .Where(g => g.Year == course && !g.IsDeleted)
                .OrderBy(g => g.Name)
                .ToListAsync();

            return groups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups by course {Course}", course);
            throw;
        }
    }

    /// <summary>
    /// Получает группы по году
    /// </summary>
    public async Task<IReadOnlyList<Group>> GetGroupsByYearAsync(int year)
    {
        try
        {
            var groups = await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                    .ThenInclude(c => c.Person)
                .Where(g => g.Year == year && !g.IsDeleted)
                .OrderBy(g => g.Name)
                .ToListAsync();

            return groups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups by year {Year}", year);
            throw;
        }
    }

    /// <summary>
    /// Получает группы по куратору
    /// </summary>
    public async Task<IReadOnlyList<Group>> GetGroupsByCuratorAsync(Guid curatorUid)
    {
        try
        {
            var groups = await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                    .ThenInclude(c => c.Person)
                .Where(g => g.CuratorUid == curatorUid && !g.IsDeleted)
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Name)
                .ToListAsync();

            return groups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups by curator {CuratorUid}", curatorUid);
            throw;
        }
    }

    /// <summary>
    /// Получает активные группы
    /// </summary>
    public async Task<IReadOnlyList<Group>> GetActiveGroupsAsync()
    {
        try
        {
            return await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                    .ThenInclude(c => c.Person)
                .Where(g => g.Status == GroupStatus.Active && !g.IsDeleted)
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active groups");
            throw;
        }
    }

    /// <summary>
    /// Получает группы по департаменту
    /// </summary>
    public async Task<IReadOnlyList<Group>> GetGroupsByDepartmentAsync(Guid departmentUid)
    {
        try
        {
            return await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                    .ThenInclude(c => c.Person)
                .Where(g => g.DepartmentUid == departmentUid && !g.IsDeleted)
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups by department {DepartmentUid}", departmentUid);
            throw;
        }
    }

    /// <summary>
    /// Поиск групп по названию
    /// </summary>
    public async Task<IReadOnlyList<Group>> SearchGroupsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllGroupsAsync();

            var lowerSearchTerm = searchTerm.ToLower();

            var groups = await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                    .ThenInclude(c => c.Person)
                .Where(g => !g.IsDeleted && (
                    g.Name.ToLower().Contains(lowerSearchTerm) ||
                    g.Code.ToLower().Contains(lowerSearchTerm) ||
                    g.Description.ToLower().Contains(lowerSearchTerm) ||
                    g.Curator != null && g.Curator.Person != null && (
                        g.Curator.Person.FirstName.ToLower().Contains(lowerSearchTerm) ||
                        g.Curator.Person.LastName.ToLower().Contains(lowerSearchTerm)
                    )
                ))
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Name)
                .ToListAsync();

            return groups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching groups with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Получает группы с пагинацией
    /// </summary>
    public async Task<(IReadOnlyList<Group> Groups, int TotalCount)> GetGroupsPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        Guid? departmentUid = null)
    {
        try
        {
            var query = _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                    .ThenInclude(c => c.Person)
                .Where(g => !g.IsDeleted);

            // Применяем фильтры
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(g => 
                    g.Name.ToLower().Contains(lowerSearchTerm) ||
                    g.Code.ToLower().Contains(lowerSearchTerm) ||
                    g.Description.ToLower().Contains(lowerSearchTerm) ||
                    g.Curator != null && g.Curator.Person != null && (
                        g.Curator.Person.FirstName.ToLower().Contains(lowerSearchTerm) ||
                        g.Curator.Person.LastName.ToLower().Contains(lowerSearchTerm)
                    )
                );
            }

            if (departmentUid.HasValue)
            {
                query = query.Where(g => g.DepartmentUid == departmentUid.Value);
            }

            var totalCount = await query.CountAsync();

            var groups = await query
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (groups, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged groups");
            throw;
        }
    }

    /// <summary>
    /// Получает количество студентов в группе
    /// </summary>
    public async Task<int> GetStudentsCountByGroupAsync(Guid groupUid)
    {
        try
        {
            return await _dbContext.Students
                .Where(s => s.GroupUid == groupUid)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting students count for group {GroupUid}", groupUid);
            return 0;
        }
    }

    /// <summary>
    /// Получает количество экземпляров курсов для группы
    /// </summary>
    public async Task<int> GetCourseInstancesCountAsync(Guid groupUid)
    {
        try
        {
            return await _dbContext.CourseInstances
                .Where(ci => ci.GroupUid == groupUid && !ci.IsDeleted)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course instances count for group {GroupUid}", groupUid);
            return 0;
        }
    }

    /// <summary>
    /// Получает экземпляры курсов для группы
    /// </summary>
    public async Task<IReadOnlyList<CourseInstance>> GetGroupCourseInstancesAsync(Guid groupUid)
    {
        try
        {
            return await _dbContext.CourseInstances
                .Include(ci => ci.Subject)
                .Include(ci => ci.Teacher)
                    .ThenInclude(t => t.Person)
                .Include(ci => ci.AcademicPeriod)
                .Where(ci => ci.GroupUid == groupUid && !ci.IsDeleted)
                .OrderBy(ci => ci.StartDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course instances for group {GroupUid}", groupUid);
            throw;
        }
    }

    /// <summary>
    /// Получает информацию о связанных данных группы для безопасного удаления
    /// </summary>
    public async Task<GroupAnalytics> GetGroupRelatedDataInfoAsync(Guid groupUid)
    {
        try
        {
            var relatedData = new GroupAnalytics
            {
                GroupUid = groupUid,
                LastCalculated = DateTime.UtcNow
            };

            // Проверка студентов - используем метод для получения количества
            var studentsCount = await GetStudentsCountByGroupAsync(groupUid);
            
            // Проверка экземпляров курсов - используем метод для получения количества
            var courseInstancesCount = await GetCourseInstancesCountAsync(groupUid);

            // Заполняем дополнительную информацию если есть связанные данные
            if (studentsCount > 0 || courseInstancesCount > 0)
            {
                // Можно добавить дополнительную логику для анализа связанных данных
                // Например, установить флаги или заполнить дополнительные поля
            }

            return relatedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check related data for group {GroupUid}", groupUid);
            return new GroupAnalytics
            {
                GroupUid = groupUid,
                LastCalculated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Получает группы с пагинацией (алиас для GetGroupsPagedAsync)
    /// </summary>
    public async Task<(IEnumerable<Group> Groups, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null, Guid? departmentUid = null)
    {
        return await GetGroupsPagedAsync(page, pageSize, searchTerm, departmentUid);
    }

    /// <summary>
    /// Получает записи на курсы группы
    /// </summary>
    public async Task<IEnumerable<Enrollment>> GetGroupEnrollmentsAsync(Guid groupUid)
    {
        try
        {
            return await _dbContext.Enrollments
                .Include(e => e.Student)
                .ThenInclude(s => s.Person)
                .Include(e => e.CourseInstance)
                .ThenInclude(ci => ci.Subject)
                .Where(e => e.Student.GroupUid == groupUid && !e.IsDeleted)
                .OrderBy(e => e.Student.Person.LastName)
                .ThenBy(e => e.Student.Person.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enrollments for group {GroupUid}", groupUid);
            throw;
        }
    }

    /// <summary>
    /// Получает задания группы
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetGroupAssignmentsAsync(Guid groupUid)
    {
        try
        {
            return await _dbContext.Assignments
                .Include(a => a.CourseInstance)
                .ThenInclude(ci => ci.Subject)
                .Where(a => a.CourseInstance.GroupUid == groupUid && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignments for group {GroupUid}", groupUid);
            throw;
        }
    }

    /// <summary>
    /// Получает оценки группы
    /// </summary>
    public async Task<IEnumerable<Grade>> GetGroupGradesAsync(Guid groupUid)
    {
        try
        {
            return await _dbContext.Grades
                .Include(g => g.Student)
                .ThenInclude(s => s.Person)
                .Include(g => g.Assignment)
                .ThenInclude(a => a.CourseInstance)
                .ThenInclude(ci => ci.Subject)
                .Where(g => g.Student.GroupUid == groupUid && !g.IsDeleted)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grades for group {GroupUid}", groupUid);
            throw;
        }
    }

    /// <summary>
    /// Получает посещаемость группы
    /// </summary>
    public async Task<IEnumerable<Attendance>> GetGroupAttendanceAsync(Guid groupUid)
    {
        try
        {
            return await _dbContext.Attendances
                .Include(a => a.Student)
                .ThenInclude(s => s.Person)
                .Include(a => a.Lesson)
                .ThenInclude(l => l.CourseInstance)
                .ThenInclude(ci => ci.Subject)
                .Where(a => a.Student.GroupUid == groupUid && !a.IsDeleted)
                .OrderByDescending(a => a.CheckedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting attendance for group {GroupUid}", groupUid);
            throw;
        }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Валидация группы
    /// </summary>
    private async Task ValidateGroupAsync(Group group, bool isCreate)
    {
        var errors = new List<string>();

        // Проверка обязательных полей
        if (string.IsNullOrWhiteSpace(group.Name))
            errors.Add("Название группы обязательно для заполнения");

        if (string.IsNullOrWhiteSpace(group.Code))
            errors.Add("Код группы обязателен для заполнения");

        // Проверка уникальности кода группы
        if (!string.IsNullOrWhiteSpace(group.Code))
        {
            var codeExists = await _dbContext.Groups
                .Where(g => g.Uid != group.Uid && g.Code.ToLower() == group.Code.ToLower() && !g.IsDeleted)
                .AnyAsync();

            if (codeExists)
                errors.Add($"Группа с кодом '{group.Code}' уже существует");
        }

        // Проверка максимального количества студентов
        if (group.MaxStudents <= 0)
            errors.Add("Максимальное количество студентов должно быть больше нуля");

        // Проверка года обучения
        if (group.Year <= 0 || group.Year > 6)
            errors.Add("Год обучения должен быть от 1 до 6");

        // Проверка куратора
        if (group.CuratorUid.HasValue)
        {
            var curatorExists = await _dbContext.Teachers
                .Where(t => t.Uid == group.CuratorUid.Value)
                .AnyAsync();

            if (!curatorExists)
                errors.Add($"Куратор с Uid {group.CuratorUid.Value} не найден");
        }

        if (errors.Any())
        {
            throw new ArgumentException($"Validation failed: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Генерирует уникальный код группы
    /// </summary>
    private async Task<string> GenerateGroupCodeAsync(int year)
    {
        var yearCode = year.ToString();
        
        var lastCode = await _dbContext.Groups
            .Where(g => g.Code.StartsWith($"ГР-{yearCode}") && !g.IsDeleted)
            .OrderByDescending(g => g.Code)
            .Select(g => g.Code)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastCode))
        {
            var parts = lastCode.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"ГР-{yearCode}-{nextNumber:D2}";
    }

    #endregion

    #region New Methods

    /// <summary>
    /// Назначает куратора группы
    /// </summary>
    public async Task<bool> AssignCuratorAsync(Guid groupUid, Guid? curatorUid)
    {
        try
        {
            var group = await _dbContext.Groups.FindAsync(groupUid);
            if (group == null || group.IsDeleted)
            {
                _logger.LogWarning("Group {GroupUid} not found for curator assignment", groupUid);
                return false;
            }

            // Проверяем существование куратора если указан
            if (curatorUid.HasValue)
            {
                var curatorExists = await _dbContext.Teachers
                    .AnyAsync(t => t.Uid == curatorUid.Value);

                if (!curatorExists)
                {
                    _logger.LogWarning("Curator {CuratorUid} not found", curatorUid.Value);
                    return false;
                }
            }

            group.CuratorUid = curatorUid;
            group.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning curator {CuratorUid} to group {GroupUid}", curatorUid, groupUid);
            return false;
        }
    }

    /// <summary>
    /// Удаляет куратора группы
    /// </summary>
    public async Task<bool> RemoveCuratorAsync(Guid groupUid)
    {
        return await AssignCuratorAsync(groupUid, null);
    }

    /// <summary>
    /// Переводит группу на следующий курс
    /// </summary>
    public async Task<bool> PromoteToNextCourseAsync(Guid groupUid)
    {
        try
        {
            var group = await _dbContext.Groups.FindAsync(groupUid);
            if (group == null || group.IsDeleted)
            {
                _logger.LogWarning("Group not found for promotion: {GroupUid}", groupUid);
                return false;
            }

            if (group.Year >= 6)
            {
                _logger.LogWarning("Group is already on the final course: {GroupUid}", groupUid);
                return false;
            }

            group.Year++;
            group.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error promoting group to next course: {GroupUid}", groupUid);
            throw;
        }
    }

    /// <summary>
    /// Получает группу по коду
    /// </summary>
    public async Task<Group?> GetByCodeAsync(string code)
    {
        try
        {
            return await _dbContext.Groups
                .Include(g => g.Students)
                .Include(g => g.Curator)
                    .ThenInclude(c => c!.Person)
                .FirstOrDefaultAsync(g => g.Code == code && !g.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group by code {Code}", code);
            throw;
        }
    }

    /// <summary>
    /// Проверяет существование группы с указанным названием
    /// </summary>
    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeUid = null)
    {
        try
        {
            var query = _dbContext.Groups
                .Where(g => g.Name.ToLower() == name.ToLower() && !g.IsDeleted);

            if (excludeUid.HasValue)
            {
                query = query.Where(g => g.Uid != excludeUid.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if group exists by name: {Name}", name);
            throw;
        }
    }

    #endregion
}
