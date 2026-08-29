using ViridiscaUi.Domain.Entities.Auth;

namespace ViridiscaUi.Domain.Services.Auth;

/// <summary>
/// Сервис для работы со связями ролей и разрешений
/// </summary>
public interface IRolePermissionService
{
    /// <summary>
    /// Получает связь роли и разрешения по идентификатору
    /// </summary>
    Task<RolePermission?> GetRolePermissionAsync(Guid uid);
    
    /// <summary>
    /// Получает все связи ролей и разрешений
    /// </summary>
    Task<IEnumerable<RolePermission>> GetAllRolePermissionsAsync();
    
    /// <summary>
    /// Получает связь по идентификатору роли и разрешения
    /// </summary>
    Task<RolePermission?> GetRolePermissionByRoleAndPermissionAsync(Guid roleUid, Guid permissionUid);
    
    /// <summary>
    /// Получает связи по идентификатору роли
    /// </summary>
    Task<IEnumerable<RolePermission>> GetRolePermissionsByRoleAsync(Guid roleUid);
    
    /// <summary>
    /// Получает связи по идентификатору разрешения
    /// </summary>
    Task<IEnumerable<RolePermission>> GetRolePermissionsByPermissionAsync(Guid permissionUid);
    
    /// <summary>
    /// Добавляет новую связь роли и разрешения
    /// </summary>
    Task AddRolePermissionAsync(RolePermission rolePermission);
    
    /// <summary>
    /// Добавляет разрешение роли
    /// </summary>
    Task<bool> AddPermissionToRoleAsync(Guid roleUid, Guid permissionUid);
    
    /// <summary>
    /// Обновляет существующую связь роли и разрешения
    /// </summary>
    Task<bool> UpdateRolePermissionAsync(RolePermission rolePermission);
    
    /// <summary>
    /// Удаляет связь роли и разрешения
    /// </summary>
    Task<bool> DeleteRolePermissionAsync(Guid uid);
    
    /// <summary>
    /// Удаляет разрешение у роли
    /// </summary>
    Task<bool> RemovePermissionFromRoleAsync(Guid roleUid, Guid permissionUid);
}
