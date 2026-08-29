using ViridiscaUi.Domain.Entities.System;

namespace ViridiscaUi.Domain.Services.System;

/// <summary>
/// Сервис для работы с департаментами
/// </summary>
public interface IDepartmentService
{
    /// <summary>
    /// Получает департамент по идентификатору
    /// </summary>
    Task<Department?> GetDepartmentAsync(Guid uid);

    /// <summary>
    /// Получает все департаменты
    /// </summary>
    Task<IEnumerable<Department>> GetAllDepartmentsAsync();

    /// <summary>
    /// Получает активные департаменты
    /// </summary>
    Task<IEnumerable<Department>> GetActiveDepartmentsAsync();

    /// <summary>
    /// Создает новый департамент
    /// </summary>
    Task<Department> CreateDepartmentAsync(Department department);

    /// <summary>
    /// Добавляет новый департамент
    /// </summary>
    Task AddDepartmentAsync(Department department);

    /// <summary>
    /// Обновляет существующий департамент
    /// </summary>
    Task<bool> UpdateDepartmentAsync(Department department);

    /// <summary>
    /// Удаляет департамент
    /// </summary>
    Task<bool> DeleteDepartmentAsync(Guid uid);

    /// <summary>
    /// Поиск департаментов по названию или коду
    /// </summary>
    Task<IEnumerable<Department>> SearchDepartmentsAsync(string searchTerm);

    /// <summary>
    /// Получает департаменты с пагинацией
    /// </summary>
    Task<(IEnumerable<Department> Departments, int TotalCount)> GetDepartmentsPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null);

    /// <summary>
    /// Проверяет существование департамента с указанным кодом
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeUid = null);

    /// <summary>
    /// Получает статистику по департаменту
    /// </summary>
    Task<DepartmentStatistics> GetDepartmentStatisticsAsync(Guid departmentUid);

    /// <summary>
    /// Активирует/деактивирует департамент
    /// </summary>
    Task<bool> SetDepartmentActiveStatusAsync(Guid uid, bool isActive);

    /// <summary>
    /// Получает департамент по идентификатору
    /// </summary>
    Task<Department?> GetByUidAsync(Guid uid);

    /// <summary>
    /// Получает все департаменты
    /// </summary>
    Task<IEnumerable<Department>> GetAllAsync();

    /// <summary>
    /// Удаляет департамент
    /// </summary>
    Task<bool> DeleteAsync(Guid uid);

    /// <summary>
    /// Обновляет департамент
    /// </summary>
    Task<bool> UpdateAsync(Department department);

    /// <summary>
    /// Создает новый департамент
    /// </summary>
    Task<Department> CreateAsync(Department department);

    /// <summary>
    /// Получает департамент по коду
    /// </summary>
    Task<Department?> GetByCodeAsync(string code);

    /// <summary>
    /// Получает все департаменты с пагинацией
    /// </summary>
    Task<(IEnumerable<Department> Departments, int TotalCount)> GetPagedAsync(
        int page = 1, 
        int pageSize = 20, 
        string? searchTerm = null);

    /// <summary>
    /// Проверяет существование департамента
    /// </summary>
    Task<bool> ExistsAsync(Guid uid);

    /// <summary>
    /// Получает статистику департаментов
    /// </summary>
    Task<object> GetStatisticsAsync();

    /// <summary>
    /// Получает общее количество департаментов
    /// </summary>
    Task<int> GetTotalCountAsync();
}
