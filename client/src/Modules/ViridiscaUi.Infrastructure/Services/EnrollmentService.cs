using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Data;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Models;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Сервис для управления записями студентов на курсы
/// </summary>
public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(ApplicationDbContext context, ILogger<EnrollmentService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Enrollment>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Получение всех записей на курсы");
            
            return await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.Person)
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci.Teacher)
                        .ThenInclude(t => t.Person)
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении всех записей на курсы");
            throw;
        }
    }

    public async Task<Enrollment?> GetByUidAsync(Guid uid)
    {
        try
        {
            _logger.LogInformation("Получение записи на курс по Uid: {Uid}", uid);
            
            return await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.Person)
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci.Teacher)
                        .ThenInclude(t => t.Person)
                .FirstOrDefaultAsync(e => e.Uid == uid && !e.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении записи на курс по Uid: {Uid}", uid);
            throw;
        }
    }

    public async Task<IEnumerable<Enrollment>> GetByStudentUidAsync(Guid studentUid)
    {
        try
        {
            _logger.LogInformation("Получение записей студента: {StudentUid}", studentUid);
            
            return await _context.Enrollments
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci.Teacher)
                        .ThenInclude(t => t.Person)
                .Where(e => e.StudentUid == studentUid && !e.IsDeleted)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении записей студента: {StudentUid}", studentUid);
            throw;
        }
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseInstanceUidAsync(Guid courseInstanceUid)
    {
        try
        {
            _logger.LogInformation("Получение записей на курс: {CourseInstanceUid}", courseInstanceUid);
            
            return await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.Person)
                .Where(e => e.CourseInstanceUid == courseInstanceUid && !e.IsDeleted)
                .OrderBy(e => e.Student.Person.LastName)
                .ThenBy(e => e.Student.Person.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении записей на курс: {CourseInstanceUid}", courseInstanceUid);
            throw;
        }
    }

    public async Task<Enrollment> CreateAsync(Enrollment enrollment)
    {
        ArgumentNullException.ThrowIfNull(enrollment);

        try
        {
            _logger.LogInformation("Создание записи на курс для студента: {StudentUid}", enrollment.StudentUid);

            // Валидация
            await ValidateEnrollmentAsync(enrollment, isCreate: true);

            enrollment.Uid = Guid.NewGuid();
            enrollment.CreatedAt = DateTime.UtcNow;
            enrollment.Status = EnrollmentStatus.Enrolled;

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Запись на курс создана: {Uid}", enrollment.Uid);
            return enrollment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании записи на курс");
            throw;
        }
    }

    public async Task<Enrollment> UpdateAsync(Enrollment enrollment)
    {
        ArgumentNullException.ThrowIfNull(enrollment);

        try
        {
            _logger.LogInformation("Обновление записи на курс: {Uid}", enrollment.Uid);

            var existing = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.Uid == enrollment.Uid && !e.IsDeleted);

            if (existing == null)
                throw new InvalidOperationException($"Запись на курс с Uid {enrollment.Uid} не найдена");

            // Валидация
            await ValidateEnrollmentAsync(enrollment, isCreate: false);

            // Обновление полей
            existing.Status = enrollment.Status;
            existing.FinalGrade = enrollment.FinalGrade;
            existing.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Запись на курс обновлена: {Uid}", enrollment.Uid);
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении записи на курс: {Uid}", enrollment.Uid);
            throw;
        }
    }

    public async Task DeleteAsync(Guid uid)
    {
        try
        {
            _logger.LogInformation("Удаление записи на курс: {Uid}", uid);

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.Uid == uid && !e.IsDeleted);

            if (enrollment == null)
                throw new InvalidOperationException($"Запись на курс с Uid {uid} не найдена");

            // Мягкое удаление
            enrollment.IsDeleted = true;
            enrollment.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Запись на курс удалена: {Uid}", uid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении записи на курс: {Uid}", uid);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid uid)
    {
        try
        {
            return await _context.Enrollments
                .AnyAsync(e => e.Uid == uid && !e.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке существования записи: {Uid}", uid);
            throw;
        }
    }

    public async Task<Enrollment> EnrollStudentAsync(Guid studentUid, Guid courseInstanceUid)
    {
        try
        {
            _logger.LogInformation("Запись студента {StudentUid} на курс {CourseInstanceUid}", studentUid, courseInstanceUid);

            // Проверяем, не записан ли уже студент
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentUid == studentUid && 
                                         e.CourseInstanceUid == courseInstanceUid && 
                                         !e.IsDeleted);

            if (existingEnrollment != null)
                throw new InvalidOperationException("Студент уже записан на этот курс");

            var enrollment = new Enrollment
            {
                StudentUid = studentUid,
                CourseInstanceUid = courseInstanceUid,
                Status = EnrollmentStatus.Enrolled
            };

            return await CreateAsync(enrollment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при записи студента на курс");
            throw;
        }
    }

    public async Task UnenrollStudentAsync(Guid studentUid, Guid courseInstanceUid)
    {
        try
        {
            _logger.LogInformation("Отчисление студента {StudentUid} с курса {CourseInstanceUid}", studentUid, courseInstanceUid);

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentUid == studentUid && 
                                         e.CourseInstanceUid == courseInstanceUid && 
                                         !e.IsDeleted);

            if (enrollment == null)
                throw new InvalidOperationException("Запись на курс не найдена");

            enrollment.Status = EnrollmentStatus.Dropped;
            enrollment.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Студент отчислен с курса: {EnrollmentUid}", enrollment.Uid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отчислении студента с курса");
            throw;
        }
    }

    public async Task<IEnumerable<Enrollment>> GetByStatusAsync(EnrollmentStatus status)
    {
        try
        {
            _logger.LogInformation("Получение записей по статусу: {Status}", status);
            
            return await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.Person)
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Where(e => e.Status == status && !e.IsDeleted)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении записей по статусу: {Status}", status);
            throw;
        }
    }

    public async Task UpdateStatusAsync(Guid enrollmentUid, EnrollmentStatus status)
    {
        try
        {
            _logger.LogInformation("Обновление статуса записи {EnrollmentUid} на {Status}", enrollmentUid, status);

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.Uid == enrollmentUid && !e.IsDeleted);

            if (enrollment == null)
                throw new InvalidOperationException($"Запись на курс с Uid {enrollmentUid} не найдена");

            enrollment.Status = status;
            enrollment.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Статус записи обновлен: {EnrollmentUid}", enrollmentUid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении статуса записи: {EnrollmentUid}", enrollmentUid);
            throw;
        }
    }

    public async Task<IEnumerable<Enrollment>> GetActiveEnrollmentsByStudentAsync(Guid studentUid)
    {
        try
        {
            return await _context.Enrollments
                .Include(e => e.CourseInstance)
                    .ThenInclude(ci => ci.Subject)
                .Where(e => e.StudentUid == studentUid && 
                           e.Status == EnrollmentStatus.Enrolled && 
                           !e.IsDeleted)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении активных записей студента: {StudentUid}", studentUid);
            throw;
        }
    }

    public async Task<int> GetEnrollmentCountByCourseAsync(Guid courseInstanceUid)
    {
        try
        {
            return await _context.Enrollments
                .CountAsync(e => e.CourseInstanceUid == courseInstanceUid && 
                                e.Status == EnrollmentStatus.Enrolled && 
                                !e.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении количества записей на курс: {CourseInstanceUid}", courseInstanceUid);
            throw;
        }
    }

    public async Task<bool> IsStudentEnrolledAsync(Guid studentUid, Guid courseInstanceUid)
    {
        try
        {
            return await _context.Enrollments
                .AnyAsync(e => e.StudentUid == studentUid && 
                              e.CourseInstanceUid == courseInstanceUid && 
                              e.Status == EnrollmentStatus.Enrolled && 
                              !e.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке записи студента на курс");
            throw;
        }
    }

    private async Task ValidateEnrollmentAsync(Enrollment enrollment, bool isCreate)
    {
        var errors = new List<string>();

        // Проверка студента
        if (enrollment.StudentUid == Guid.Empty)
            errors.Add("Студент должен быть указан");
        else
        {
            var studentExists = await _context.Students
                .AnyAsync(s => s.Uid == enrollment.StudentUid && !s.IsDeleted);
            if (!studentExists)
                errors.Add("Указанный студент не существует");
        }

        // Проверка курса
        if (enrollment.CourseInstanceUid == Guid.Empty)
            errors.Add("Курс должен быть указан");
        else
        {
            var courseExists = await _context.CourseInstances
                .AnyAsync(ci => ci.Uid == enrollment.CourseInstanceUid && !ci.IsDeleted);
            if (!courseExists)
                errors.Add("Указанный курс не существует");
        }

        // Проверка дублирования при создании
        if (isCreate)
        {
            var duplicateExists = await _context.Enrollments
                .AnyAsync(e => e.StudentUid == enrollment.StudentUid && 
                              e.CourseInstanceUid == enrollment.CourseInstanceUid && 
                              !e.IsDeleted);
            if (duplicateExists)
                errors.Add("Студент уже записан на этот курс");
        }

        if (errors.Any())
            throw new ArgumentException($"Ошибки валидации: {string.Join(", ", errors)}");
    }
} 