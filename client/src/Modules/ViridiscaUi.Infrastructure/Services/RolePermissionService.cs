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
/// Реализация сервиса для работы со связями ролей и разрешений
/// </summary>
public class RolePermissionService(ApplicationDbContext dbContext) : IRolePermissionService
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<RolePermission?> GetRolePermissionAsync(Guid uid)
    {
        return await _dbContext.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(rp => rp.Uid == uid);
    }

    public async Task<IEnumerable<RolePermission>> GetAllRolePermissionsAsync()
    {
        return await _dbContext.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task<RolePermission?> GetRolePermissionByRoleAndPermissionAsync(Guid roleUid, Guid permissionUid)
    {
        return await _dbContext.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(rp => rp.RoleUid == roleUid && rp.PermissionUid == permissionUid);
    }

    public async Task<IEnumerable<RolePermission>> GetRolePermissionsByRoleAsync(Guid roleUid)
    {
        return await _dbContext.RolePermissions
            .Where(rp => rp.RoleUid == roleUid)
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task<IEnumerable<RolePermission>> GetRolePermissionsByPermissionAsync(Guid permissionUid)
    {
        return await _dbContext.RolePermissions
            .Where(rp => rp.PermissionUid == permissionUid)
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task AddRolePermissionAsync(RolePermission rolePermission)
    {
        rolePermission.CreatedAt = DateTime.UtcNow;
        rolePermission.LastModifiedAt = DateTime.UtcNow;
        
        await _dbContext.RolePermissions.AddAsync(rolePermission);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> AddPermissionToRoleAsync(Guid roleUid, Guid permissionUid)
    {
        // Проверяем существование роли и разрешения
        var role = await _dbContext.Roles.FindAsync(roleUid);
        var permission = await _dbContext.Permissions.FindAsync(permissionUid);
        
        if (role == null || permission == null)
            return false;

        // Проверяем, не назначено ли уже разрешение
        var existingAssignment = await _dbContext.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleUid == roleUid && rp.PermissionUid == permissionUid);
            
        if (existingAssignment != null)
            return true; // Разрешение уже назначено

        // Создаем новую связь
        var rolePermission = new RolePermission
        {
            Uid = Guid.NewGuid(),
            RoleUid = roleUid,
            PermissionUid = permissionUid,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        await _dbContext.RolePermissions.AddAsync(rolePermission);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateRolePermissionAsync(RolePermission rolePermission)
    {
        var existingRolePermission = await _dbContext.RolePermissions.FindAsync(rolePermission.Uid);
        if (existingRolePermission == null)
            return false;

        existingRolePermission.RoleUid = rolePermission.RoleUid;
        existingRolePermission.PermissionUid = rolePermission.PermissionUid;
        existingRolePermission.LastModifiedAt = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteRolePermissionAsync(Guid uid)
    {
        var rolePermission = await _dbContext.RolePermissions.FindAsync(uid);
        if (rolePermission == null)
            return false;

        _dbContext.RolePermissions.Remove(rolePermission);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemovePermissionFromRoleAsync(Guid roleUid, Guid permissionUid)
    {
        var rolePermission = await _dbContext.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleUid == roleUid && rp.PermissionUid == permissionUid);
            
        if (rolePermission == null)
            return false;

        _dbContext.RolePermissions.Remove(rolePermission);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
