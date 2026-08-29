using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Data;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Сервис для управления академическими периодами
/// </summary>
public class AcademicPeriodService : IAcademicPeriodService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AcademicPeriodService> _logger;

    public AcademicPeriodService(ApplicationDbContext context, ILogger<AcademicPeriodService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<AcademicPeriod>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Получение всех академических периодов");
            
            return await _context.AcademicPeriods
                .Where(ap => !ap.IsDeleted)
                .OrderBy(ap => ap.StartDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении всех академических периодов");
            throw;
        }
    }

    public async Task<AcademicPeriod?> GetByIdAsync(Guid uid)
    {
        return await GetByUidAsync(uid);
    }

    public async Task<AcademicPeriod?> GetByUidAsync(Guid uid)
    {
        try
        {
            _logger.LogInformation("Получение академического периода по Uid: {Uid}", uid);
            
            return await _context.AcademicPeriods
                .FirstOrDefaultAsync(ap => ap.Uid == uid && !ap.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении академического периода по Uid: {Uid}", uid);
            throw;
        }
    }

    public async Task<AcademicPeriod?> GetCurrentAsync()
    {
        try
        {
            _logger.LogInformation("Получение текущего академического периода");
            
            var now = DateTime.Now;
            return await _context.AcademicPeriods
                .Where(ap => !ap.IsDeleted && ap.StartDate <= now && ap.EndDate >= now)
                .OrderBy(ap => ap.StartDate)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении текущего академического периода");
            throw;
        }
    }

    public async Task<IEnumerable<AcademicPeriod>> GetActiveAsync()
    {
        try
        {
            _logger.LogInformation("Получение активных академических периодов");
            
            var now = DateTime.Now;
            return await _context.AcademicPeriods
                .Where(ap => !ap.IsDeleted && ap.EndDate >= now)
                .OrderBy(ap => ap.StartDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении активных академических периодов");
            throw;
        }
    }

    public async Task<AcademicPeriod> CreateAsync(AcademicPeriod academicPeriod)
    {
        ArgumentNullException.ThrowIfNull(academicPeriod);

        try
        {
            _logger.LogInformation("Создание академического периода: {Name}", academicPeriod.Name);

            // Валидация
            await ValidateAcademicPeriodAsync(academicPeriod, isCreate: true);

            academicPeriod.Uid = Guid.NewGuid();
            academicPeriod.CreatedAt = DateTime.UtcNow;

            _context.AcademicPeriods.Add(academicPeriod);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Академический период создан: {Uid}", academicPeriod.Uid);
            return academicPeriod;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании академического периода");
            throw;
        }
    }

    public async Task<AcademicPeriod> UpdateAsync(AcademicPeriod academicPeriod)
    {
        ArgumentNullException.ThrowIfNull(academicPeriod);

        try
        {
            _logger.LogInformation("Обновление академического периода: {Uid}", academicPeriod.Uid);

            var existing = await _context.AcademicPeriods
                .FirstOrDefaultAsync(ap => ap.Uid == academicPeriod.Uid && !ap.IsDeleted);

            if (existing == null)
                throw new InvalidOperationException($"Академический период с Uid {academicPeriod.Uid} не найден");

            // Валидация
            await ValidateAcademicPeriodAsync(academicPeriod, isCreate: false);

            // Обновление полей
            existing.Name = academicPeriod.Name;
            existing.Type = academicPeriod.Type;
            existing.StartDate = academicPeriod.StartDate;
            existing.EndDate = academicPeriod.EndDate;
            existing.Description = academicPeriod.Description;
            existing.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Академический период обновлен: {Uid}", academicPeriod.Uid);
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении академического периода: {Uid}", academicPeriod.Uid);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid uid)
    {
        try
        {
            _logger.LogInformation("Удаление академического периода: {Uid}", uid);

            var academicPeriod = await _context.AcademicPeriods
                .FirstOrDefaultAsync(ap => ap.Uid == uid && !ap.IsDeleted);

            if (academicPeriod == null)
                return false;

            // Проверяем, есть ли связанные данные
            var hasRelatedData = await _context.CourseInstances
                .AnyAsync(ci => ci.AcademicPeriodUid == uid && !ci.IsDeleted);

            if (hasRelatedData)
                throw new InvalidOperationException("Нельзя удалить академический период, который используется в курсах");

            // Мягкое удаление
            academicPeriod.IsDeleted = true;
            academicPeriod.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Академический период удален: {Uid}", uid);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении академического периода: {Uid}", uid);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid uid)
    {
        try
        {
            return await _context.AcademicPeriods
                .AnyAsync(ap => ap.Uid == uid && !ap.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке существования академического периода: {Uid}", uid);
            throw;
        }
    }

    public async Task<int> GetCountAsync()
    {
        try
        {
            return await _context.AcademicPeriods
                .CountAsync(ap => !ap.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении количества академических периодов");
            throw;
        }
    }

    private async Task ValidateAcademicPeriodAsync(AcademicPeriod academicPeriod, bool isCreate)
    {
        var errors = new List<string>();

        // Проверка обязательных полей
        if (string.IsNullOrWhiteSpace(academicPeriod.Name))
            errors.Add("Название академического периода обязательно");

        if (academicPeriod.StartDate >= academicPeriod.EndDate)
            errors.Add("Дата начала должна быть раньше даты окончания");

        // Проверка пересечения с другими периодами
        var overlappingPeriods = await _context.AcademicPeriods
            .Where(ap => !ap.IsDeleted && 
                        (isCreate || ap.Uid != academicPeriod.Uid) &&
                        (ap.StartDate <= academicPeriod.StartDate && ap.EndDate >= academicPeriod.StartDate ||
                         ap.StartDate <= academicPeriod.EndDate && ap.EndDate >= academicPeriod.EndDate ||
                         ap.StartDate >= academicPeriod.StartDate && ap.EndDate <= academicPeriod.EndDate))
            .ToListAsync();

        if (overlappingPeriods.Any())
            errors.Add("Академический период пересекается с существующими периодами");

        if (errors.Any())
            throw new ArgumentException($"Ошибки валидации: {string.Join(", ", errors)}");
    }
} 