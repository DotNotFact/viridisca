using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Data;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для работы с учебными планами
/// </summary>
/// <remarks>
/// ICurriculumService had no implementation registered anywhere in DI - GroupDialogViewModel,
/// StudentDialogViewModel, and CurriculumViewModel (the routed "Учебные планы" page) all take
/// it as a constructor dependency, so navigating to Curriculum or opening the student/group
/// dialogs threw a DI resolution failure. Added following the same EF-backed pattern as
/// DepartmentService/SubjectService.
/// </remarks>
public class CurriculumService : ICurriculumService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CurriculumService> _logger;

    public CurriculumService(ApplicationDbContext dbContext, ILogger<CurriculumService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<Curriculum>> GetAllAsync()
    {
        try
        {
            return await _dbContext.Curricula
                .Include(c => c.Department)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all curricula");
            throw;
        }
    }

    public async Task<Curriculum?> GetByIdAsync(Guid uid) => await GetByUidAsync(uid);

    public async Task<Curriculum?> GetByCodeAsync(string code)
    {
        try
        {
            return await _dbContext.Curricula
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.Code == code && !c.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting curriculum by code {Code}", code);
            throw;
        }
    }

    public async Task<Curriculum?> GetByUidAsync(Guid uid)
    {
        try
        {
            return await _dbContext.Curricula
                .Include(c => c.Department)
                .Include(c => c.CurriculumSubjects)
                    .ThenInclude(cs => cs.Subject)
                .FirstOrDefaultAsync(c => c.Uid == uid && !c.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting curriculum {CurriculumUid}", uid);
            throw;
        }
    }

    public async Task<Curriculum> CreateAsync(Curriculum curriculum)
    {
        try
        {
            curriculum.Uid = curriculum.Uid == Guid.Empty ? Guid.NewGuid() : curriculum.Uid;
            curriculum.CreatedAt = DateTime.UtcNow;
            curriculum.LastModifiedAt = DateTime.UtcNow;

            _dbContext.Curricula.Add(curriculum);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Curriculum created: {CurriculumName} ({CurriculumUid})", curriculum.Name, curriculum.Uid);
            return curriculum;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating curriculum {CurriculumName}", curriculum.Name);
            throw;
        }
    }

    public async Task<Curriculum> UpdateAsync(Curriculum curriculum)
    {
        try
        {
            var existing = await _dbContext.Curricula.FirstOrDefaultAsync(c => c.Uid == curriculum.Uid);
            if (existing == null)
            {
                throw new InvalidOperationException($"Curriculum {curriculum.Uid} not found");
            }

            existing.Name = curriculum.Name;
            existing.Description = curriculum.Description;
            existing.Code = curriculum.Code;
            existing.TotalCredits = curriculum.TotalCredits;
            existing.DurationSemesters = curriculum.DurationSemesters;
            existing.DurationMonths = curriculum.DurationMonths;
            existing.AcademicYear = curriculum.AcademicYear;
            existing.StartYear = curriculum.StartYear;
            existing.EndYear = curriculum.EndYear;
            existing.DurationInSemesters = curriculum.DurationInSemesters;
            existing.IsActive = curriculum.IsActive;
            existing.ValidFrom = curriculum.ValidFrom;
            existing.ValidTo = curriculum.ValidTo;
            existing.DepartmentUid = curriculum.DepartmentUid;
            existing.LastModifiedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating curriculum {CurriculumUid}", curriculum.Uid);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid uid)
    {
        try
        {
            var curriculum = await _dbContext.Curricula.FirstOrDefaultAsync(c => c.Uid == uid);
            if (curriculum == null) return false;

            curriculum.IsDeleted = true;
            curriculum.LastModifiedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting curriculum {CurriculumUid}", uid);
            return false;
        }
    }

    public async Task<bool> ExistsAsync(Guid uid)
    {
        return await _dbContext.Curricula.AnyAsync(c => c.Uid == uid && !c.IsDeleted);
    }

    public async Task<int> GetCountAsync()
    {
        return await _dbContext.Curricula.CountAsync(c => !c.IsDeleted);
    }

    public async Task<int> GetStudentsCountAsync(Guid curriculumUid)
    {
        return await _dbContext.Students.CountAsync(s => s.CurriculumUid == curriculumUid && !s.IsDeleted);
    }

    public async Task<int> GetSubjectsCountAsync(Guid curriculumUid)
    {
        return await _dbContext.CurriculumSubjects.CountAsync(cs => cs.CurriculumUid == curriculumUid);
    }

    public async Task<Curriculum> CopyAsync(Guid curriculumUid, string? newName = null)
    {
        var source = await _dbContext.Curricula
            .Include(c => c.CurriculumSubjects)
            .FirstOrDefaultAsync(c => c.Uid == curriculumUid);

        if (source == null)
        {
            throw new InvalidOperationException($"Curriculum {curriculumUid} not found");
        }

        var copy = new Curriculum
        {
            Uid = Guid.NewGuid(),
            Name = newName ?? $"{source.Name} (копия)",
            Description = source.Description,
            Code = source.Code == null ? null : $"{source.Code}-COPY",
            TotalCredits = source.TotalCredits,
            DurationSemesters = source.DurationSemesters,
            DurationMonths = source.DurationMonths,
            AcademicYear = source.AcademicYear,
            StartYear = source.StartYear,
            EndYear = source.EndYear,
            DurationInSemesters = source.DurationInSemesters,
            IsActive = source.IsActive,
            ValidFrom = source.ValidFrom,
            ValidTo = source.ValidTo,
            DepartmentUid = source.DepartmentUid,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        _dbContext.Curricula.Add(copy);

        foreach (var cs in source.CurriculumSubjects)
        {
            _dbContext.CurriculumSubjects.Add(new CurriculumSubject
            {
                Uid = Guid.NewGuid(),
                CurriculumUid = copy.Uid,
                SubjectUid = cs.SubjectUid,
                Semester = cs.Semester,
                Credits = cs.Credits,
                IsRequired = cs.IsRequired,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();
        return copy;
    }

    public Task<Curriculum> DuplicateCurriculumAsync(Guid curriculumUid, string newName) => CopyAsync(curriculumUid, newName);

    public async Task<Curriculum> ActivateAsync(Guid curriculumUid)
    {
        var curriculum = await _dbContext.Curricula.FirstOrDefaultAsync(c => c.Uid == curriculumUid)
            ?? throw new InvalidOperationException($"Curriculum {curriculumUid} not found");

        curriculum.IsActive = true;
        curriculum.LastModifiedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return curriculum;
    }

    public async Task<Curriculum> DeactivateAsync(Guid curriculumUid)
    {
        var curriculum = await _dbContext.Curricula.FirstOrDefaultAsync(c => c.Uid == curriculumUid)
            ?? throw new InvalidOperationException($"Curriculum {curriculumUid} not found");

        curriculum.IsActive = false;
        curriculum.LastModifiedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return curriculum;
    }

    public async Task<byte[]> ExportAsync(Guid curriculumUid)
    {
        var curriculum = await GetByUidAsync(curriculumUid)
            ?? throw new InvalidOperationException($"Curriculum {curriculumUid} not found");

        var json = JsonSerializer.Serialize(curriculum, new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        });

        return System.Text.Encoding.UTF8.GetBytes(json);
    }

    public Task<Curriculum?> ImportAsync()
    {
        // No file source is passed in via the interface - importing a curriculum from a
        // file requires a file-picker at the UI layer, which doesn't exist yet for this page.
        _logger.LogWarning("ImportAsync called but no import source is wired up yet");
        return Task.FromResult<Curriculum?>(null);
    }

    public async Task<IEnumerable<CurriculumSubject>> GetCurriculumSubjectsAsync(IEnumerable<Guid> curriculumUids)
    {
        var uids = curriculumUids.ToList();
        return await _dbContext.CurriculumSubjects
            .Include(cs => cs.Subject)
            .Where(cs => uids.Contains(cs.CurriculumUid))
            .ToListAsync();
    }

    public async Task<(int TotalSubjects, int TotalCredits, int CompletedSubjects, decimal CompletionRate)> GetStatisticsAsync(Guid curriculumUid)
    {
        var subjects = await _dbContext.CurriculumSubjects
            .Where(cs => cs.CurriculumUid == curriculumUid)
            .ToListAsync();

        var totalSubjects = subjects.Count;
        var totalCredits = subjects.Sum(s => s.Credits);
        // "Completed" isn't tracked at the curriculum-subject level yet - no per-student
        // progress link exists here, so this reports 0 rather than fabricating a number.
        var completedSubjects = 0;
        var completionRate = totalSubjects > 0 ? (decimal)completedSubjects / totalSubjects * 100m : 0m;

        return (totalSubjects, totalCredits, completedSubjects, completionRate);
    }

    public async Task<(IEnumerable<Curriculum> curricula, int totalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        Guid? departmentUid = null,
        bool? isActive = null,
        int? minCredits = null,
        int? maxCredits = null,
        int? academicYear = null)
    {
        try
        {
            var query = _dbContext.Curricula
                .Include(c => c.Department)
                .Where(c => !c.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lower = searchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(lower) ||
                    (c.Code != null && c.Code.ToLower().Contains(lower)) ||
                    (c.Description != null && c.Description.ToLower().Contains(lower)));
            }

            if (departmentUid.HasValue)
            {
                query = query.Where(c => c.DepartmentUid == departmentUid.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            if (minCredits.HasValue)
            {
                query = query.Where(c => c.TotalCredits >= minCredits.Value);
            }

            if (maxCredits.HasValue)
            {
                query = query.Where(c => c.TotalCredits <= maxCredits.Value);
            }

            if (academicYear.HasValue)
            {
                query = query.Where(c => c.AcademicYear == academicYear.Value);
            }

            var totalCount = await query.CountAsync();
            var curricula = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (curricula, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged curricula");
            throw;
        }
    }
}
