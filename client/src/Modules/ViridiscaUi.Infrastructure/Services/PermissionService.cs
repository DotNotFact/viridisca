using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Auth;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Infrastructure.Data;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Реализация сервиса для работы с разрешениями
/// </summary>
public class PermissionService(ApplicationDbContext dbContext, IRoleService roleService, Lazy<IAuthService> authService, ILogger<PermissionService> logger) : IPermissionService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IRoleService _roleService = roleService;
    private readonly Lazy<IAuthService> _authService = authService;
    private readonly ILogger<PermissionService> _logger = logger;

    /// <summary>
    /// Получает разрешение по идентификатору
    /// </summary>
    public async Task<Permission?> GetPermissionAsync(Guid uid)
    {
        return await _dbContext.Permissions.FindAsync(uid);
    }
    
    /// <summary>
    /// Получает разрешение по названию
    /// </summary>
    public async Task<Permission?> GetPermissionByNameAsync(string name)
    {
        return await _dbContext.Permissions
            .FirstOrDefaultAsync(p => p.Name == name);
    }
    
    /// <summary>
    /// Получает все разрешения
    /// </summary>
    public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
    {
        return await _dbContext.Permissions
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
    
    /// <summary>
    /// Получает разрешения роли
    /// </summary>
    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleUid)
    {
        return await _dbContext.RolePermissions
            .Where(rp => rp.RoleUid == roleUid)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission!)
            .ToListAsync();
    }
    
    /// <summary>
    /// Получает разрешения пользователя как объекты Permission
    /// </summary>
    public async Task<IEnumerable<Permission>> GetUserPermissionObjectsAsync(Guid userUid)
    {
        // Получаем роли пользователя
        var roles = await _roleService.GetUserRolesAsync(userUid);
        
        var permissions = new List<Permission>();
        
        // Для каждой роли получаем связанные разрешения
        foreach (var role in roles)
        {
            var rolePermissions = await GetRolePermissionsAsync(role.Uid);
            permissions.AddRange(rolePermissions);
        }
        
        // Удаляем дубликаты
        return permissions.GroupBy(p => p.Uid).Select(g => g.First());
    }
    
    /// <summary>
    /// Добавляет новое разрешение
    /// </summary>
    public async Task AddPermissionAsync(Permission permission)
    {
        permission.CreatedAt = DateTime.UtcNow;
        permission.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.Permissions.AddAsync(permission);
        await _dbContext.SaveChangesAsync();
    }
    
    /// <summary>
    /// Обновляет существующее разрешение
    /// </summary>
    public async Task<bool> UpdatePermissionAsync(Permission permission)
    {
        var existingPermission = await _dbContext.Permissions.FindAsync(permission.Uid);
        if (existingPermission == null)
            return false;

        existingPermission.Name = permission.Name;
        existingPermission.Description = permission.Description;
        existingPermission.LastModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    /// <summary>
    /// Удаляет разрешение
    /// </summary>
    public async Task<bool> DeletePermissionAsync(Guid uid)
    {
        var permission = await _dbContext.Permissions.FindAsync(uid);
        if (permission == null)
            return false;

        // Удаляем связанные RolePermission
        var rolePermissions = await _dbContext.RolePermissions
            .Where(rp => rp.PermissionUid == uid)
            .ToListAsync();

        _dbContext.RolePermissions.RemoveRange(rolePermissions);
        _dbContext.Permissions.Remove(permission);

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Проверяет, есть ли у текущего пользователя указанное разрешение
    /// </summary>
    /// <param name="permission">Название разрешения</param>
    /// <returns>True, если разрешение есть</returns>
    public async Task<bool> HasPermissionAsync(string permission)
    {
        try
        {
            var currentUser = await _authService.Value.GetCurrentPersonAsync();
            if (currentUser == null) return false;

            return await HasPermissionAsync(currentUser.Uid, permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке разрешения {Permission}", permission);
            return false;
        }
    }

    /// <summary>
    /// Проверяет, есть ли у пользователя указанное разрешение
    /// </summary>
    /// <param name="personUid">Идентификатор пользователя</param>
    /// <param name="permission">Название разрешения</param>
    /// <returns>True, если разрешение есть</returns>
    public async Task<bool> HasPermissionAsync(Guid personUid, string permission)
    {
        try
        {
            var userPermissions = await GetUserPermissionObjectsAsync(personUid);
            return userPermissions.Any(p => p.Name.Equals(permission, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке разрешения {Permission} для пользователя {PersonUid}", permission, personUid);
            return false;
        }
    }

    // === ДОПОЛНИТЕЛЬНЫЕ МЕТОДЫ ИНТЕРФЕЙСА ===

    /// <summary>
    /// Получает все разрешения (алиас для GetAllPermissionsAsync)
    /// </summary>
    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await GetAllPermissionsAsync();
    }

    /// <summary>
    /// Получает разрешение по идентификатору (алиас для GetPermissionAsync)
    /// </summary>
    public async Task<Permission?> GetByIdAsync(Guid uid)
    {
        return await GetPermissionAsync(uid);
    }

    /// <summary>
    /// Получает разрешения роли (алиас для GetRolePermissionsAsync)
    /// </summary>
    public async Task<IEnumerable<Permission>> GetByRoleAsync(Guid roleUid)
    {
        return await GetRolePermissionsAsync(roleUid);
    }

    /// <summary>
    /// Создает новое разрешение (алиас для AddPermissionAsync)
    /// </summary>
    public async Task<Permission> CreateAsync(Permission permission)
    {
        await AddPermissionAsync(permission);
        return permission;
    }

    /// <summary>
    /// Обновляет разрешение (алиас для UpdatePermissionAsync)
    /// </summary>
    public async Task<Permission> UpdateAsync(Permission permission)
    {
        var success = await UpdatePermissionAsync(permission);
        if (!success)
            throw new InvalidOperationException($"Не удалось обновить разрешение с ID {permission.Uid}");
        return permission;
    }

    /// <summary>
    /// Удаляет разрешение (алиас для DeletePermissionAsync)
    /// </summary>
    public async Task<bool> DeleteAsync(Guid uid)
    {
        return await DeletePermissionAsync(uid);
    }

    /// <summary>
    /// Проверяет существование разрешения
    /// </summary>
    public async Task<bool> ExistsAsync(Guid uid)
    {
        var permission = await GetPermissionAsync(uid);
        return permission != null;
    }

    /// <summary>
    /// Получает количество разрешений
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        return await _dbContext.Permissions.CountAsync();
    }

    /// <summary>
    /// Получает разрешения пользователя как строки (для совместимости с интерфейсом)
    /// </summary>
    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userUid)
    {
        try
        {
            var permissions = await GetUserPermissionObjectsAsync(userUid);
            return permissions.Select(p => p.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении разрешений пользователя {UserUid}", userUid);
            return Enumerable.Empty<string>();
        }
    }
}
