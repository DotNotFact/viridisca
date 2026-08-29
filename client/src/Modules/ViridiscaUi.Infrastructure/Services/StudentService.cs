using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Models.Common;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.System.Enums;
using static ViridiscaUi.Domain.Services.Education.IStudentService;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Data;
using ViridiscaUi.Domain.Entities.Analytics;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для работы со студентами
/// Независимый сервис без наследования от GenericCrudService
/// </summary>
public class StudentService : IStudentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<StudentService> _logger;

    public StudentService(ApplicationDbContext dbContext, ILogger<StudentService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Analytics Methods

    /// <summary>
    /// Получает аналитику студента
    /// </summary>
    public async Task<StudentAnalytics> GetAnalyticsAsync(Guid studentUid, Guid? academicPeriodUid = null)
    {
        try
        {
            // Используем текущий академический период, если не указан
            if (!academicPeriodUid.HasValue)
            {
                var currentPeriod = await _dbContext.AcademicPeriods
                    .Where(ap => ap.StartDate <= DateTime.UtcNow && ap.EndDate >= DateTime.UtcNow)
                    .FirstOrDefaultAsync();
                
                academicPeriodUid = currentPeriod?.Uid ?? Guid.Empty;
            }

            // Ищем существующую аналитику
            var analytics = await _dbContext.StudentAnalytics
                .Include(sa => sa.Student)
                    .ThenInclude(s => s!.Person)
                .Include(sa => sa.Student)
                    .ThenInclude(s => s!.Grades)
                .Include(sa => sa.Student)
                    .ThenInclude(s => s!.Enrollments)
                        .ThenInclude(e => e.CourseInstance)
                .Include(sa => sa.AcademicPeriod)
                .FirstOrDefaultAsync(sa => sa.StudentUid == studentUid && sa.AcademicPeriodUid == academicPeriodUid);

            // Если не найдена, создаем новую
            if (analytics == null)
            {
                analytics = new StudentAnalytics
                {
                    StudentUid = studentUid,
                    AcademicPeriodUid = academicPeriodUid.Value,
                    LastCalculated = DateTime.UtcNow
                };

                _dbContext.StudentAnalytics.Add(analytics);
                await _dbContext.SaveChangesAsync();

                // Перезагружаем с навигационными свойствами
                analytics = await _dbContext.StudentAnalytics
                    .Include(sa => sa.Student)
                        .ThenInclude(s => s!.Person)
                    .Include(sa => sa.Student)
                        .ThenInclude(s => s!.Grades)
                    .Include(sa => sa.Student)
                        .ThenInclude(s => s!.Enrollments)
                            .ThenInclude(e => e.CourseInstance)
                    .Include(sa => sa.AcademicPeriod)
                    .FirstAsync(sa => sa.Uid == analytics.Uid);
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
            _logger.LogError(ex, "Error getting student analytics {StudentUid}", studentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику студента (для совместимости с интерфейсом)
    /// </summary>
    public async Task<StudentAnalytics> GetStudentStatisticsAsync(Guid studentUid, Guid? academicPeriodUid = null)
    {
        return await GetAnalyticsAsync(studentUid, academicPeriodUid);
    }

    #endregion

    #region Базовые CRUD операции

    /// <summary>
    /// Получает студента по идентификатору
    /// </summary>
    public async Task<Student?> GetByUidAsync(Guid uid)
    {
        try
        {
            return await _dbContext.Students
                .Include(s => s.Person)
                .Include(s => s.Group)
                .Include(s => s.Curriculum)
                .FirstOrDefaultAsync(s => s.Uid == uid && !s.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student {StudentUid}", uid);
            throw;
        }
    }

    /// <summary>
    /// Получает всех студентов
    /// </summary>
    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        try
        {
            return await _dbContext.Students
                .Include(s => s.Person)
                .Include(s => s.Group)
                .Include(s => s.Curriculum)
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Person.LastName)
                .ThenBy(s => s.Person.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all students");
            throw;
        }
    }

    /// <summary>
    /// Создает нового студента
    /// </summary>
    public async Task<Student> CreateAsync(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);

        try
        {
            // Валидация
            await ValidateStudentAsync(student, true);

            // Генерируем код студента если не указан
            if (string.IsNullOrEmpty(student.StudentCode))
            {
                student.StudentCode = await GenerateStudentCodeAsync();
            }

            student.Uid = Guid.NewGuid();
            student.CreatedAt = DateTime.UtcNow;
            student.LastModifiedAt = DateTime.UtcNow;

            _dbContext.Students.Add(student);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created student {StudentCode} with UID {StudentUid}", 
                student.StudentCode, student.Uid);
            return student;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating student {StudentCode}", student.StudentCode);
            throw;
        }
    }

    /// <summary>
    /// Обновляет существующего студента
    /// </summary>
    public async Task<bool> UpdateAsync(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);

        try
        {
            var existingStudent = await _dbContext.Students.FindAsync(student.Uid);
            if (existingStudent == null || existingStudent.IsDeleted)
                return false;

            // Валидация
            await ValidateStudentAsync(student, false);

            // Обновляем поля
            existingStudent.StudentCode = student.StudentCode;
            existingStudent.PersonUid = student.PersonUid;
            existingStudent.GroupUid = student.GroupUid;
            existingStudent.CurriculumUid = student.CurriculumUid;
            existingStudent.Status = student.Status;
            existingStudent.EnrollmentDate = student.EnrollmentDate;
            existingStudent.GraduationDate = student.GraduationDate;
            existingStudent.GPA = student.GPA;
            existingStudent.IsActive = student.IsActive;
            existingStudent.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated student {StudentUid}", student.Uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating student {StudentUid}", student.Uid);
            throw;
        }
    }

    /// <summary>
    /// Удаляет студента
    /// </summary>
    public async Task<bool> DeleteAsync(Guid uid)
    {
        try
        {
            var student = await _dbContext.Students.FindAsync(uid);
            if (student == null || student.IsDeleted)
                return false;

            // Мягкое удаление
            student.IsDeleted = true;
            student.DeletedAt = DateTime.UtcNow;
            student.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Deleted student {StudentUid}", uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting student {StudentUid}", uid);
            throw;
        }
    }

    #endregion

    #region Дополнительные методы для совместимости с ViewModels

    /// <summary>
    /// Получает студентов с пагинацией (упрощенная версия)
    /// </summary>
    public async Task<(IEnumerable<Student> Students, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null)
    {
        try
        {
            var query = _dbContext.Students
                .Include(s => s.Person)
                .Include(s => s.Group)
                .Include(s => s.Curriculum)
                .Where(s => !s.IsDeleted);

            // Применяем поиск если указан
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s => 
                    s.Person.FirstName.Contains(searchTerm) ||
                    s.Person.LastName.Contains(searchTerm) ||
                    s.StudentCode.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var students = await query
                .OrderBy(s => s.Person.LastName)
                .ThenBy(s => s.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (students, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged students");
            throw;
        }
    }

    /// <summary>
    /// Получает общее количество студентов
    /// </summary>
    public async Task<int> CountAsync()
    {
        try
        {
            return await _dbContext.Students
                .Where(s => !s.IsDeleted)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting students");
            throw;
        }
    }

    /// <summary>
    /// Обновляет студента (алиас для UpdateAsync)
    /// </summary>
    public async Task<bool> UpdateStudentAsync(Student student)
    {
        return await UpdateAsync(student);
    }

    /// <summary>
    /// Удаляет студента (алиас для DeleteAsync)
    /// </summary>
    public async Task<bool> DeleteStudentAsync(Guid uid)
    {
        return await DeleteAsync(uid);
    }

    #endregion

    #region Специфичные методы для Student

    /// <summary>
    /// Получает активных студентов
    /// </summary>
    public async Task<IEnumerable<Student>> GetActiveStudentsAsync()
    {
        try
        {
            return await _dbContext.Students
                .Include(s => s.Person)
                .Include(s => s.Group)
                .Include(s => s.Curriculum)
                .Where(s => !s.IsDeleted && s.IsActive && s.Status == StudentStatus.Active)
                .OrderBy(s => s.Person.LastName)
                .ThenBy(s => s.Person.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active students");
            throw;
        }
    }

    /// <summary>
    /// Получает студентов с пагинацией
    /// </summary>
    public async Task<(IEnumerable<Student> Students, int TotalCount)> GetStudentsPagedAsync(
        int page, int pageSize, string? searchTerm = null, Guid? groupUid = null, StudentStatus? status = null)
    {
        try
        {
            var query = _dbContext.Students
                .Include(s => s.Person)
                .Include(s => s.Group)
                .Include(s => s.Curriculum)
                .Where(s => !s.IsDeleted);

            // Применяем фильтры
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(s => 
                    s.Person.FirstName.ToLower().Contains(lowerSearchTerm) ||
                    s.Person.LastName.ToLower().Contains(lowerSearchTerm) ||
                    s.StudentCode.ToLower().Contains(lowerSearchTerm) ||
                    s.Person.Email.ToLower().Contains(lowerSearchTerm)
                );
            }

            if (groupUid.HasValue)
            {
                query = query.Where(s => s.GroupUid == groupUid.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            var totalCount = await query.CountAsync();

            var students = await query
                .OrderBy(s => s.Person.LastName)
                .ThenBy(s => s.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (students, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged students");
            throw;
        }
    }

    /// <summary>
    /// Проверяет существование по коду студента
    /// </summary>
    public async Task<bool> ExistsByStudentCodeAsync(string studentCode, Guid? excludeUid = null)
    {
        try
        {
            var query = _dbContext.Students
                .Where(s => s.StudentCode.ToLower() == studentCode.ToLower() && !s.IsDeleted);

            if (excludeUid.HasValue)
            {
                query = query.Where(s => s.Uid != excludeUid.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if student exists by code {StudentCode}", studentCode);
            throw;
        }
    }

    /// <summary>
    /// Переводит студента в другую группу
    /// </summary>
    public async Task<bool> TransferStudentAsync(Guid studentUid, Guid newGroupUid)
    {
        try
        {
            var student = await _dbContext.Students.FindAsync(studentUid);
            if (student == null || student.IsDeleted)
                return false;

            // Проверяем существование новой группы
            var newGroup = await _dbContext.Groups.FindAsync(newGroupUid);
            if (newGroup == null || newGroup.IsDeleted)
                return false;

            var oldGroupUid = student.GroupUid;
            student.GroupUid = newGroupUid;
            student.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Transferred student {StudentUid} from group {OldGroupUid} to group {NewGroupUid}", 
                studentUid, oldGroupUid, newGroupUid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring student {StudentUid} to group {NewGroupUid}", studentUid, newGroupUid);
            return false;
        }
    }

    /// <summary>
    /// Получает GPA студента
    /// </summary>
    public async Task<double> GetStudentGPAAsync(Guid studentUid)
    {
        try
        {
            var student = await _dbContext.Students
                .FirstOrDefaultAsync(s => s.Uid == studentUid && !s.IsDeleted);

            if (student == null)
                return 0.0;

            return (double)student.GPA; // Explicit cast from decimal to double
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting GPA for student {StudentUid}", studentUid);
            return 0.0;
        }
    }

    /// <summary>
    /// Получает записи студента на курсы
    /// </summary>
    public async Task<IEnumerable<Enrollment>> GetStudentEnrollmentsAsync(Guid studentUid)
    {
        try
        {
            return await _dbContext.Enrollments
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci.Teacher)
                        .ThenInclude(t => t.Person)
                .Where(e => e.StudentUid == studentUid)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enrollments for student {StudentUid}", studentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает студентов по группе
    /// </summary>
    public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(Guid groupUid)
    {
        try
        {
            return await _dbContext.Students
                .Include(s => s.Person)
                .Include(s => s.Curriculum)
                .Where(s => s.GroupUid == groupUid && !s.IsDeleted)
                .OrderBy(s => s.Person.LastName)
                .ThenBy(s => s.Person.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting students by group {GroupUid}", groupUid);
            throw;
        }
    }

    /// <summary>
    /// Получает студентов по статусу
    /// </summary>
    public async Task<IEnumerable<Student>> GetStudentsByStatusAsync(StudentStatus status)
    {
        try
        {
            return await _dbContext.Students
                .Include(s => s.Person)
                .Include(s => s.Group)
                .Include(s => s.Curriculum)
                .Where(s => s.Status == status && !s.IsDeleted)
                .OrderBy(s => s.Person.LastName)
                .ThenBy(s => s.Person.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting students by status {Status}", status);
            throw;
        }
    }

    /// <summary>
    /// Поиск студентов
    /// </summary>
    public async Task<IEnumerable<Student>> SearchStudentsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var lowerSearchTerm = searchTerm.ToLower();

            return await _dbContext.Students
                .Include(s => s.Person)
                .Include(s => s.Group)
                .Include(s => s.Curriculum)
                .Where(s => !s.IsDeleted && (
                    s.Person.FirstName.ToLower().Contains(lowerSearchTerm) ||
                    s.Person.LastName.ToLower().Contains(lowerSearchTerm) ||
                    s.StudentCode.ToLower().Contains(lowerSearchTerm) ||
                    s.Person.Email.ToLower().Contains(lowerSearchTerm) ||
                    s.Person.PhoneNumber != null && s.Person.PhoneNumber.Contains(searchTerm)
                ))
                .OrderBy(s => s.Person.LastName)
                .ThenBy(s => s.Person.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching students with term {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Получает студента по коду
    /// </summary>
    public async Task<Student?> GetByStudentCodeAsync(string studentCode)
    {
        try
        {
            return await _dbContext.Students
                .Include(s => s.Person)
                .Include(s => s.Group)
                .Include(s => s.Curriculum)
                .FirstOrDefaultAsync(s => s.StudentCode.ToLower() == studentCode.ToLower() && !s.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student by code {StudentCode}", studentCode);
            throw;
        }
    }

    /// <summary>
    /// Изменяет статус студента
    /// </summary>
    public async Task<bool> ChangeStudentStatusAsync(Guid studentUid, StudentStatus newStatus)
    {
        try
        {
            var student = await _dbContext.Students.FindAsync(studentUid);
            if (student == null || student.IsDeleted)
                return false;

            var oldStatus = student.Status;
            student.Status = newStatus;
            student.LastModifiedAt = DateTime.UtcNow;

            // Если студент выпускается, устанавливаем дату выпуска
            if (newStatus == StudentStatus.Graduated && !student.GraduationDate.HasValue)
            {
                student.GraduationDate = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Changed status for student {StudentUid} from {OldStatus} to {NewStatus}", 
                studentUid, oldStatus, newStatus);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing status for student {StudentUid}", studentUid);
            return false;
        }
    }

    /// <summary>
    /// Получает статистику студентов
    /// </summary>
    public async Task<StudentAnalytics> GetStudentStatisticsAsync()
    {
        try
        {
            // Получаем первого студента для демонстрации
            var firstStudent = await _dbContext.Students.FirstOrDefaultAsync(s => !s.IsDeleted);
            if (firstStudent != null)
            {
                var statisticsService = new StatisticsService(_dbContext);
                return await statisticsService.GetStudentAnalyticsAsync(firstStudent.Uid);
            }
            
            return new StudentAnalytics
            {
                StudentUid = Guid.Empty,
                LastCalculated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student statistics");
            return new StudentAnalytics
            {
                StudentUid = Guid.Empty,
                LastCalculated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Получает оценки студента
    /// </summary>
    public async Task<IEnumerable<Grade>> GetStudentGradesAsync(Guid studentUid)
    {
        try
        {
            return await _dbContext.Grades
                .Include(g => g.Assignment)
                    .ThenInclude(a => a.CourseInstance)
                        .ThenInclude(ci => ci.Subject)
                .Where(g => g.StudentUid == studentUid)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting grades for student {StudentUid}", studentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает статистику конкретного студента
    /// </summary>
    public async Task<StudentAnalytics> GetStudentStatisticsAsync(Guid studentUid)
    {
        try
        {
            var statisticsService = new StatisticsService(_dbContext);
            return await statisticsService.GetStudentAnalyticsAsync(studentUid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics for student {StudentUid}", studentUid);
            return new StudentAnalytics
            {
                StudentUid = studentUid,
                LastCalculated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Получает общую статистику студентов
    /// </summary>
    public async Task<GeneralStudentStatistics> GetAllStudentStatisticsAsync()
    {
        try
        {
            var totalStudents = await _dbContext.Students.CountAsync(s => !s.IsDeleted);
            var activeStudents = await _dbContext.Students.CountAsync(s => !s.IsDeleted && s.Status == StudentStatus.Active);
            var graduatedStudents = await _dbContext.Students.CountAsync(s => !s.IsDeleted && s.Status == StudentStatus.Graduated);
            var suspendedStudents = await _dbContext.Students.CountAsync(s => !s.IsDeleted && s.Status == StudentStatus.Suspended);

            var averageGPA = await _dbContext.Students
                .Where(s => !s.IsDeleted && s.GPA > 0)
                .AverageAsync(s => (double)s.GPA);

            var studentsByGroup = await _dbContext.Students
                .Include(s => s.Group)
                .Where(s => !s.IsDeleted && s.Group != null)
                .GroupBy(s => s.Group!.Name)
                .Select(g => new { Group = g.Key, Count = g.Count() })
                .ToListAsync();

            return new GeneralStudentStatistics
            {
                TotalStudents = totalStudents,
                ActiveStudents = activeStudents,
                GraduatedStudents = graduatedStudents,
                SuspendedStudents = suspendedStudents,
                AverageGPA = averageGPA,
                StudentsByGroup = studentsByGroup.ToDictionary(x => x.Group, x => x.Count),
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting general student statistics");
            throw;
        }
    }

    /// <summary>
    /// Получает всех студентов (алиас)
    /// </summary>
    public async Task<IEnumerable<Student>> GetStudentsAsync()
    {
        return await GetAllAsync();
    }

    /// <summary>
    /// Получает всех студентов (еще один алиас)
    /// </summary>
    public async Task<IEnumerable<Student>> GetAllStudentsAsync()
    {
        return await GetAllAsync();
    }

    /// <summary>
    /// Получает студента по UID (алиас для GetByUidAsync)
    /// </summary>
    public async Task<Student?> GetStudentAsync(Guid uid)
    {
        return await GetByUidAsync(uid);
    }

    /// <summary>
    /// Создает студента (алиас для CreateAsync)
    /// </summary>
    public async Task<Student> CreateStudentAsync(Student student)
    {
        return await CreateAsync(student);
    }

    /// <summary>
    /// Получает количество студентов в группе
    /// </summary>
    public async Task<int> GetStudentsCountByGroupAsync(Guid groupUid)
    {
        try
        {
            return await _dbContext.Students
                .CountAsync(s => s.GroupUid == groupUid && !s.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting students count for group {GroupUid}", groupUid);
            throw;
        }
    }

    /// <summary>
    /// Получает курсы студента
    /// </summary>
    public async Task<IEnumerable<CourseInstance>> GetStudentCoursesAsync(Guid studentUid)
    {
        try
        {
            return await _dbContext.Enrollments
                .Include(e => e.CourseInstance)
                .ThenInclude(ci => ci.Subject)
                .Include(e => e.CourseInstance)
                .ThenInclude(ci => ci.Teacher)
                .ThenInclude(t => t.Person)
                .Where(e => e.StudentUid == studentUid && !e.IsDeleted)
                .Select(e => e.CourseInstance)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting courses for student {StudentUid}", studentUid);
            throw;
        }
    }

    /// <summary>
    /// Получает посещаемость студента
    /// </summary>
    public async Task<IEnumerable<Attendance>> GetStudentAttendanceAsync(Guid studentUid)
    {
        try
        {
            return await _dbContext.Attendances
                .Include(a => a.Lesson)
                .ThenInclude(l => l.CourseInstance)
                .ThenInclude(ci => ci.Subject)
                .Where(a => a.StudentUid == studentUid && !a.IsDeleted)
                .OrderByDescending(a => a.CheckedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting attendance for student {StudentUid}", studentUid);
            throw;
        }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Валидация студента
    /// </summary>
    private async Task ValidateStudentAsync(Student student, bool isCreate)
    {
        var errors = new List<string>();

        // Проверка обязательных полей
        if (student.PersonUid == Guid.Empty)
            errors.Add("PersonUid обязателен");

        if (string.IsNullOrWhiteSpace(student.StudentCode))
            errors.Add("Код студента обязателен");

        // Проверка уникальности кода студента
        if (!string.IsNullOrWhiteSpace(student.StudentCode))
        {
            var codeExists = await ExistsByStudentCodeAsync(student.StudentCode, isCreate ? null : student.Uid);
            if (codeExists)
                errors.Add($"Студент с кодом '{student.StudentCode}' уже существует");
        }

        // Проверка существования Person
        if (student.PersonUid != Guid.Empty)
        {
            var personExists = await _dbContext.Persons.AnyAsync(p => p.Uid == student.PersonUid && !p.IsDeleted);
            if (!personExists)
                errors.Add($"Person с UID {student.PersonUid} не найден");
        }

        // Проверка существования группы
        if (student.GroupUid.HasValue)
        {
            var groupExists = await _dbContext.Groups.AnyAsync(g => g.Uid == student.GroupUid.Value && !g.IsDeleted);
            if (!groupExists)
                errors.Add($"Группа с UID {student.GroupUid.Value} не найдена");
        }

        // Проверка существования учебного плана
        if (student.CurriculumUid.HasValue)
        {
            var curriculumExists = await _dbContext.Curricula.AnyAsync(c => c.Uid == student.CurriculumUid.Value && !c.IsDeleted);
            if (!curriculumExists)
                errors.Add($"Учебный план с UID {student.CurriculumUid.Value} не найден");
        }

        if (errors.Any())
        {
            throw new ArgumentException($"Validation failed: {string.Join("; ", errors)}");
        }
    }

    /// <summary>
    /// Генерирует код студента
    /// </summary>
    private async Task<string> GenerateStudentCodeAsync()
    {
        var year = DateTime.Now.Year.ToString();
        var prefix = "STU";
        
        // Находим последний номер для текущего года
        var lastCode = await _dbContext.Students
            .Where(s => s.StudentCode.StartsWith($"{prefix}{year}"))
            .OrderByDescending(s => s.StudentCode)
            .Select(s => s.StudentCode)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (!string.IsNullOrEmpty(lastCode))
        {
            var numberPart = lastCode.Substring($"{prefix}{year}".Length);
            if (int.TryParse(numberPart, out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{year}{nextNumber:D4}";
    }

    #endregion
}

/// <summary>
/// Общая статистика студентов
/// </summary>
public class GeneralStudentStatistics
{
    public int TotalStudents { get; set; }
    public int ActiveStudents { get; set; }
    public int GraduatedStudents { get; set; }
    public int SuspendedStudents { get; set; }
    public double AverageGPA { get; set; }
    public Dictionary<string, int> StudentsByGroup { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}
