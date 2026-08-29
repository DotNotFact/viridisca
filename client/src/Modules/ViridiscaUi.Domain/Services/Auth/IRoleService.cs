using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.Domain.Services.Auth;

/// <summary>
/// Сервис для работы с ролями
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Получает роль по идентификатору
    /// </summary>
    Task<Role?> GetRoleAsync(Guid uid);
    
    /// <summary>
    /// Получает роль по названию
    /// </summary>
    Task<Role?> GetRoleByNameAsync(string name);
    
    /// <summary>
    /// Получает все роли
    /// </summary>
    Task<IEnumerable<Role>> GetAllRolesAsync();
    
    /// <summary>
    /// Получает роли пользователя
    /// </summary>
    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userUid);
    
    /// <summary>
    /// Добавляет новую роль
    /// </summary>
    Task AddRoleAsync(Role role);
    
    /// <summary>
    /// Обновляет существующую роль
    /// </summary>
    Task<bool> UpdateRoleAsync(Role role);
    
    /// <summary>
    /// Удаляет роль
    /// </summary>
    Task<bool> DeleteRoleAsync(Guid uid);
    
    /// <summary>
    /// Присваивает роль пользователю
    /// </summary>
    Task<bool> AssignRoleToUserAsync(Guid roleUid, Guid userUid);
    
    /// <summary>
    /// Удаляет роль у пользователя
    /// </summary>
    Task<bool> RemoveRoleFromUserAsync(Guid roleUid, Guid userUid);
}
