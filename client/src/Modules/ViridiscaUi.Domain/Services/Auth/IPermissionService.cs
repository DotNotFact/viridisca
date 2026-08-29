using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.Domain.Services.Auth;

/// <summary>
/// Сервис для управления правами доступа
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Получает все разрешения
    /// </summary>
    Task<IEnumerable<Permission>> GetAllAsync();
    
    /// <summary>
    /// Получает разрешение по идентификатору
    /// </summary>
    Task<Permission?> GetByIdAsync(Guid uid);
    
    /// <summary>
    /// Получает разрешения роли
    /// </summary>
    Task<IEnumerable<Permission>> GetByRoleAsync(Guid roleUid);
    
    /// <summary>
    /// Добавляет новое разрешение
    /// </summary>
    Task<Permission> CreateAsync(Permission permission);
    
    /// <summary>
    /// Обновляет существующее разрешение
    /// </summary>
    Task<Permission> UpdateAsync(Permission permission);
    
    /// <summary>
    /// Удаляет разрешение
    /// </summary>
    Task<bool> DeleteAsync(Guid uid);
    
    /// <summary>
    /// Проверяет существование разрешения
    /// </summary>
    Task<bool> ExistsAsync(Guid uid);
    
    /// <summary>
    /// Получает количество разрешений
    /// </summary>
    Task<int> GetCountAsync();
    
    /// <summary>
    /// Проверяет, есть ли у текущего пользователя указанное разрешение
    /// </summary>
    /// <param name="permission">Название разрешения</param>
    /// <returns>True, если разрешение есть</returns>
    Task<bool> HasPermissionAsync(string permission);
    
    /// <summary>
    /// Проверяет, есть ли у пользователя указанное разрешение
    /// </summary>
    /// <param name="personUid">Идентификатор пользователя</param>
    /// <param name="permission">Название разрешения</param>
    /// <returns>True, если разрешение есть</returns>
    Task<bool> HasPermissionAsync(Guid personUid, string permission);
    
    /// <summary>
    /// Получает разрешения пользователя
    /// </summary>
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid personUid);
}
