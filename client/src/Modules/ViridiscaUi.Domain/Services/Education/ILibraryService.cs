using ViridiscaUi.Domain.Entities.Library;
using ViridiscaUi.Domain.Entities.Library.Enums;

namespace ViridiscaUi.Domain.Services.Education;

/// <summary>
/// Сервис для управления библиотечными ресурсами
/// </summary>
public interface ILibraryService
{
    // Управление ресурсами
    Task<IEnumerable<LibraryResource>> GetAllResourcesAsync();
    Task<LibraryResource?> GetResourceByIdAsync(Guid uid);
    Task<IEnumerable<LibraryResource>> GetResourcesByTypeAsync(ResourceType type);
    Task<LibraryResource> CreateResourceAsync(LibraryResource resource);
    Task<LibraryResource> UpdateResourceAsync(LibraryResource resource);
    Task<bool> DeleteResourceAsync(Guid uid);
    Task<bool> ResourceExistsAsync(Guid uid);
    Task<int> GetResourcesCountAsync();
    
    // Управление займами
    Task<IEnumerable<LibraryLoan>> GetAllLoansAsync();
    Task<LibraryLoan?> GetLoanByIdAsync(Guid uid);
    Task<IEnumerable<LibraryLoan>> GetLoansByPersonAsync(Guid personUid);
    Task<IEnumerable<LibraryLoan>> GetLoansByResourceAsync(Guid resourceUid);
    Task<IEnumerable<LibraryLoan>> GetActiveLoansAsync();
    Task<IEnumerable<LibraryLoan>> GetOverdueLoansAsync();
    Task<LibraryLoan> CreateLoanAsync(LibraryLoan loan);
    Task<LibraryLoan> UpdateLoanAsync(LibraryLoan loan);
    Task<bool> ReturnLoanAsync(Guid loanUid);
    Task<bool> ExtendLoanAsync(Guid loanUid, DateTime newDueDate);
    Task<int> GetLoansCountAsync();
    
    // Поиск и фильтрация
    Task<IEnumerable<LibraryResource>> SearchResourcesAsync(string searchTerm);
    Task<(IEnumerable<LibraryResource> resources, int totalCount)> GetResourcesPagedAsync(
        int page, int pageSize, string? searchTerm = null);
    Task<(IEnumerable<LibraryLoan> loans, int totalCount)> GetLoansPagedAsync(
        int page, int pageSize, string? searchTerm = null);

    /// <summary>
    /// Получает количество активных займов
    /// </summary>
    Task<int> GetActiveLoansCountAsync();

    /// <summary>
    /// Получает количество записей в истории займов
    /// </summary>
    Task<int> GetLoanHistoryCountAsync();

    /// <summary>
    /// Получает общее количество ресурсов
    /// </summary>
    Task<int> GetTotalResourcesCountAsync();

    /// <summary>
    /// Получает количество доступных ресурсов
    /// </summary>
    Task<int> GetAvailableResourcesCountAsync();

    /// <summary>
    /// Получает количество заимствованных ресурсов
    /// </summary>
    Task<int> GetBorrowedResourcesCountAsync();

    /// <summary>
    /// Получает количество просроченных ресурсов
    /// </summary>
    Task<int> GetOverdueResourcesCountAsync();

    /// <summary>
    /// Получает общее количество займов
    /// </summary>
    Task<int> GetTotalLoansCountAsync();

    /// <summary>
    /// Получает количество просроченных займов
    /// </summary>
    Task<int> GetOverdueLoansCountAsync();

    /// <summary>
    /// Отправляет уведомления о просроченных займах
    /// </summary>
    Task SendOverdueNotificationsAsync();

    /// <summary>
    /// Получает статистику библиотеки
    /// </summary>
    Task<object> GetLibraryStatisticsAsync();

    /// <summary>
    /// Экспортирует ресурсы
    /// </summary>
    Task<string> ExportResourcesAsync(string format);

    /// <summary>
    /// Экспортирует займы
    /// </summary>
    Task<string> ExportLoansAsync(string format);

    /// <summary>
    /// Импортирует ресурсы
    /// </summary>
    Task<int> ImportResourcesAsync(string filePath);

    /// <summary>
    /// Получает ресурс по ISBN
    /// </summary>
    Task<LibraryResource?> GetByISBNAsync(string isbn);

    /// <summary>
    /// Получает ресурс по идентификатору
    /// </summary>
    Task<LibraryResource?> GetResourceByUidAsync(Guid uid);

    /// <summary>
    /// Создает новый библиотечный ресурс
    /// </summary>
    Task<LibraryResource> CreateAsync(LibraryResource resource);

    /// <summary>
    /// Удаляет библиотечный ресурс
    /// </summary>
    Task<bool> DeleteAsync(Guid resourceUid);

    /// <summary>
    /// Получает активные займы для конкретного ресурса
    /// </summary>
    Task<IEnumerable<LibraryLoan>> GetActiveLoansForResourceAsync(Guid resourceUid);

    /// <summary>
    /// Получает все займы для конкретного ресурса
    /// </summary>
    Task<IEnumerable<LibraryLoan>> GetLoansForResourceAsync(Guid resourceUid);
} 