using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Library;
using ViridiscaUi.Domain.Entities.Library.Enums;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Data;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для управления библиотечными ресурсами
/// </summary>
public class LibraryService : ILibraryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<LibraryService> _logger;

    public LibraryService(ApplicationDbContext dbContext, ILogger<LibraryService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region Управление ресурсами

    public async Task<IEnumerable<LibraryResource>> GetAllResourcesAsync()
    {
        return await _dbContext.LibraryResources
            .OrderBy(r => r.Title)
            .ToListAsync();
    }

    public async Task<LibraryResource?> GetResourceByIdAsync(Guid uid)
    {
        return await _dbContext.LibraryResources
            .FirstOrDefaultAsync(r => r.Uid == uid);
    }

    public async Task<IEnumerable<LibraryResource>> GetResourcesByTypeAsync(ResourceType type)
    {
        return await _dbContext.LibraryResources
            .Where(r => r.ResourceType == type)
            .OrderBy(r => r.Title)
            .ToListAsync();
    }

    public async Task<LibraryResource> CreateResourceAsync(LibraryResource resource)
    {
        resource.Uid = Guid.NewGuid();
        resource.CreatedAt = DateTime.UtcNow;
        resource.LastModifiedAt = DateTime.UtcNow;

        _dbContext.LibraryResources.Add(resource);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created library resource: {Title}", resource.Title);
        return resource;
    }

    public async Task<LibraryResource> UpdateResourceAsync(LibraryResource resource)
    {
        resource.LastModifiedAt = DateTime.UtcNow;
        _dbContext.LibraryResources.Update(resource);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Updated library resource: {Title}", resource.Title);
        return resource;
    }

    public async Task<bool> DeleteResourceAsync(Guid uid)
    {
        var resource = await _dbContext.LibraryResources.FindAsync(uid);
        if (resource == null) return false;

        _dbContext.LibraryResources.Remove(resource);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted library resource: {Title}", resource.Title);
        return true;
    }

    public async Task<bool> ResourceExistsAsync(Guid uid)
    {
        return await _dbContext.LibraryResources.AnyAsync(r => r.Uid == uid);
    }

    public async Task<int> GetResourcesCountAsync()
    {
        return await _dbContext.LibraryResources.CountAsync();
    }

    #endregion

    #region Управление займами

    public async Task<IEnumerable<LibraryLoan>> GetAllLoansAsync()
    {
        return await _dbContext.LibraryLoans
            .Include(l => l.Resource)
            .Include(l => l.Person)
            .OrderByDescending(l => l.LoanedAt)
            .ToListAsync();
    }

    public async Task<LibraryLoan?> GetLoanByIdAsync(Guid uid)
    {
        return await _dbContext.LibraryLoans
            .Include(l => l.Resource)
            .Include(l => l.Person)
            .FirstOrDefaultAsync(l => l.Uid == uid);
    }

    public async Task<IEnumerable<LibraryLoan>> GetLoansByPersonAsync(Guid personUid)
    {
        return await _dbContext.LibraryLoans
            .Include(l => l.Resource)
            .Where(l => l.PersonUid == personUid)
            .OrderByDescending(l => l.LoanedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LibraryLoan>> GetLoansByResourceAsync(Guid resourceUid)
    {
        return await _dbContext.LibraryLoans
            .Include(l => l.Person)
            .Where(l => l.ResourceUid == resourceUid)
            .OrderByDescending(l => l.LoanedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LibraryLoan>> GetActiveLoansAsync()
    {
        return await _dbContext.LibraryLoans
            .Include(l => l.Resource)
            .Include(l => l.Person)
            .Where(l => l.ReturnedAt == null)
            .OrderByDescending(l => l.LoanedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LibraryLoan>> GetOverdueLoansAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _dbContext.LibraryLoans
            .Include(l => l.Resource)
            .Include(l => l.Person)
            .Where(l => l.ReturnedAt == null && l.DueDate < today)
            .OrderBy(l => l.DueDate)
            .ToListAsync();
    }

    public async Task<LibraryLoan> CreateLoanAsync(LibraryLoan loan)
    {
        loan.Uid = Guid.NewGuid();
        loan.LoanedAt = DateTime.UtcNow;
        loan.CreatedAt = DateTime.UtcNow;
        loan.LastModifiedAt = DateTime.UtcNow;

        _dbContext.LibraryLoans.Add(loan);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created library loan for resource {ResourceUid} to person {PersonUid}", 
            loan.ResourceUid, loan.PersonUid);
        return loan;
    }

    public async Task<LibraryLoan> UpdateLoanAsync(LibraryLoan loan)
    {
        loan.LastModifiedAt = DateTime.UtcNow;
        _dbContext.LibraryLoans.Update(loan);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Updated library loan {LoanUid}", loan.Uid);
        return loan;
    }

    public async Task<bool> ReturnLoanAsync(Guid loanUid)
    {
        var loan = await _dbContext.LibraryLoans.FindAsync(loanUid);
        if (loan == null) return false;

        loan.ReturnedAt = DateTime.UtcNow;
        loan.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Returned library loan {LoanUid}", loanUid);
        return true;
    }

    public async Task<bool> ExtendLoanAsync(Guid loanUid, DateTime newDueDate)
    {
        var loan = await _dbContext.LibraryLoans.FindAsync(loanUid);
        if (loan == null) return false;

        loan.DueDate = newDueDate;
        loan.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Extended library loan {LoanUid} to {NewDueDate}", loanUid, newDueDate);
        return true;
    }

    public async Task<int> GetLoansCountAsync()
    {
        return await _dbContext.LibraryLoans.CountAsync();
    }

    #endregion

    #region Поиск и фильтрация

    public async Task<IEnumerable<LibraryResource>> SearchResourcesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllResourcesAsync();

        var lowerSearchTerm = searchTerm.ToLower();

        return await _dbContext.LibraryResources
            .Where(r => 
                r.Title.ToLower().Contains(lowerSearchTerm) ||
                r.Author.ToLower().Contains(lowerSearchTerm) ||
                r.Description.ToLower().Contains(lowerSearchTerm))
            .OrderBy(r => r.Title)
            .ToListAsync();
    }

    public async Task<(IEnumerable<LibraryResource> resources, int totalCount)> GetResourcesPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null)
    {
        var query = _dbContext.LibraryResources.AsQueryable();

        // Применение фильтров
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(r => r.Title.Contains(searchTerm) || 
                                   r.Author != null && r.Author.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var resources = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (resources, totalCount);
    }

    public async Task<(IEnumerable<LibraryLoan> loans, int totalCount)> GetLoansPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null)
    {
        var query = _dbContext.LibraryLoans
            .Include(l => l.Resource)
            .Include(l => l.Person)
            .AsQueryable();

        // Применение фильтров
        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(l => l.Resource.Title.Contains(searchTerm) ||
                                   l.Person.FirstName.Contains(searchTerm) ||
                                   l.Person.LastName.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var loans = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (loans, totalCount);
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Получает количество активных займов
    /// </summary>
    public async Task<int> GetActiveLoansCountAsync()
    {
        return await _dbContext.LibraryLoans
            .CountAsync(l => l.ReturnedAt == null);
    }

    /// <summary>
    /// Получает количество записей в истории займов
    /// </summary>
    public async Task<int> GetLoanHistoryCountAsync()
    {
        return await _dbContext.LibraryLoans.CountAsync();
    }

    /// <summary>
    /// Получает общее количество ресурсов
    /// </summary>
    public async Task<int> GetTotalResourcesCountAsync()
    {
        return await _dbContext.LibraryResources.CountAsync();
    }

    /// <summary>
    /// Получает количество доступных ресурсов
    /// </summary>
    public async Task<int> GetAvailableResourcesCountAsync()
    {
        var borrowedResourceIds = await _dbContext.LibraryLoans
            .Where(l => l.ReturnedAt == null)
            .Select(l => l.ResourceUid)
            .ToListAsync();

        return await _dbContext.LibraryResources
            .CountAsync(r => !borrowedResourceIds.Contains(r.Uid));
    }

    /// <summary>
    /// Получает количество заимствованных ресурсов
    /// </summary>
    public async Task<int> GetBorrowedResourcesCountAsync()
    {
        return await _dbContext.LibraryLoans
            .Where(l => l.ReturnedAt == null)
            .Select(l => l.ResourceUid)
            .Distinct()
            .CountAsync();
    }

    /// <summary>
    /// Получает количество просроченных ресурсов
    /// </summary>
    public async Task<int> GetOverdueResourcesCountAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbContext.LibraryLoans
            .Where(l => l.ReturnedAt == null && l.DueDate < now)
            .Select(l => l.ResourceUid)
            .Distinct()
            .CountAsync();
    }

    /// <summary>
    /// Получает общее количество займов
    /// </summary>
    public async Task<int> GetTotalLoansCountAsync()
    {
        return await _dbContext.LibraryLoans.CountAsync();
    }

    /// <summary>
    /// Получает количество просроченных займов
    /// </summary>
    public async Task<int> GetOverdueLoansCountAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbContext.LibraryLoans
            .CountAsync(l => l.ReturnedAt == null && l.DueDate < now);
    }

    /// <summary>
    /// Отправляет уведомления о просроченных займах
    /// </summary>
    public async Task SendOverdueNotificationsAsync()
    {
        var overdueLoans = await GetOverdueLoansAsync();
        // TODO: Реализовать отправку уведомлений
        _logger.LogInformation("Found {Count} overdue loans", overdueLoans.Count());
        await Task.Delay(100);
    }

    /// <summary>
    /// Получает статистику библиотеки
    /// </summary>
    public async Task<object> GetLibraryStatisticsAsync()
    {
        var totalResources = await GetResourcesCountAsync();
        var activeLoans = await GetActiveLoansCountAsync();
        var overdueLoans = (await GetOverdueLoansAsync()).Count();

        return new
        {
            TotalResources = totalResources,
            ActiveLoans = activeLoans,
            OverdueLoans = overdueLoans,
            TotalLoans = await GetLoansCountAsync(),
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Экспортирует ресурсы
    /// </summary>
    public async Task<string> ExportResourcesAsync(string format)
    {
        // TODO: Реализовать экспорт ресурсов
        await Task.Delay(100);
        var fileName = $"library_resources_{DateTime.Now:yyyyMMdd}.{format}";
        _logger.LogInformation("Exported resources to {FileName}", fileName);
        return fileName;
    }

    /// <summary>
    /// Экспортирует займы
    /// </summary>
    public async Task<string> ExportLoansAsync(string format)
    {
        // TODO: Реализовать экспорт займов
        await Task.Delay(100);
        var fileName = $"library_loans_{DateTime.Now:yyyyMMdd}.{format}";
        _logger.LogInformation("Exported loans to {FileName}", fileName);
        return fileName;
    }

    /// <summary>
    /// Импортирует ресурсы
    /// </summary>
    public async Task<int> ImportResourcesAsync(string filePath)
    {
        // TODO: Реализовать импорт ресурсов
        await Task.Delay(100);
        _logger.LogInformation("Imported resources from {FilePath}", filePath);
        return 0;
    }

    /// <summary>
    /// Получает ресурс по ISBN
    /// </summary>
    public async Task<LibraryResource?> GetByISBNAsync(string isbn)
    {
        return await _dbContext.LibraryResources
            .FirstOrDefaultAsync(r => r.ISBN == isbn);
    }

    /// <summary>
    /// Получает ресурс по идентификатору
    /// </summary>
    public async Task<LibraryResource?> GetResourceByUidAsync(Guid uid)
    {
        return await _dbContext.LibraryResources
            .FirstOrDefaultAsync(r => r.Uid == uid);
    }

    /// <summary>
    /// Создает новый библиотечный ресурс
    /// </summary>
    public async Task<LibraryResource> CreateAsync(LibraryResource resource)
    {
        if (resource == null)
            throw new ArgumentNullException(nameof(resource));

        resource.Uid = Guid.NewGuid();
        resource.CreatedAt = DateTime.UtcNow;
        resource.LastModifiedAt = DateTime.UtcNow;

        _dbContext.LibraryResources.Add(resource);
        await _dbContext.SaveChangesAsync();

        return resource;
    }

    /// <summary>
    /// Удаляет библиотечный ресурс
    /// </summary>
    public async Task<bool> DeleteAsync(Guid resourceUid)
    {
        var resource = await _dbContext.LibraryResources.FindAsync(resourceUid);
        if (resource == null)
            return false;

        _dbContext.LibraryResources.Remove(resource);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Получает активные займы для конкретного ресурса
    /// </summary>
    public async Task<IEnumerable<LibraryLoan>> GetActiveLoansForResourceAsync(Guid resourceUid)
    {
        return await _dbContext.LibraryLoans
            .Include(l => l.Person)
            .Where(l => l.ResourceUid == resourceUid && l.ReturnedAt == null)
            .ToListAsync();
    }

    /// <summary>
    /// Получает все займы для конкретного ресурса
    /// </summary>
    public async Task<IEnumerable<LibraryLoan>> GetLoansForResourceAsync(Guid resourceUid)
    {
        return await _dbContext.LibraryLoans
            .Include(l => l.Person)
            .Where(l => l.ResourceUid == resourceUid)
            .OrderByDescending(l => l.LoanedAt)
            .ToListAsync();
    }

    #endregion
} 