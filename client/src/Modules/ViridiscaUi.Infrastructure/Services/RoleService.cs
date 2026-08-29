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
/// Реализация сервиса для работы с ролями
/// </summary>
public class RoleService(ApplicationDbContext dbContext, ILogger<RoleService> logger) : IRoleService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<RoleService> _logger = logger;

    public async Task<Role?> GetRoleAsync(Guid uid)
    {
        return await _dbContext.Roles.FindAsync(uid);
    }

    public async Task<Role?> GetRoleByNameAsync(string name)
    {
        return await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _dbContext.Roles
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid personUid)
    {
        return await _dbContext.PersonRoles
            .Where(pr => pr.PersonUid == personUid)
            .Include(pr => pr.Role)
            .Select(pr => pr.Role)
            .ToListAsync();
    }

    public async Task AddRoleAsync(Role role)
    {
        role.CreatedAt = DateTime.UtcNow;
        role.LastModifiedAt = DateTime.UtcNow;
        
        await _dbContext.Roles.AddAsync(role);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> UpdateRoleAsync(Role role)
    {
        var existingRole = await _dbContext.Roles.FindAsync(role.Uid);
        if (existingRole == null)
            return false;

        existingRole.Name = role.Name;
        existingRole.Description = role.Description;
        existingRole.RoleType = role.RoleType;
        existingRole.LastModifiedAt = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteRoleAsync(Guid uid)
    {
        // Проверяем, есть ли пользователи с этой ролью
        var hasAssignedUsers = await _dbContext.PersonRoles
            .Where(pr => pr.RoleUid == uid && pr.IsActive)
            .AnyAsync();
            
        if (hasAssignedUsers)
            return false; // Нельзя удалить роль, используемую пользователями

        var role = await _dbContext.Roles.FindAsync(uid);
        if (role == null)
            return false;

        _dbContext.Roles.Remove(role);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignRoleToUserAsync(Guid personUid, Guid roleUid)
    {
        try
        {
            var existingAssignment = await _dbContext.PersonRoles
                .FirstOrDefaultAsync(pr => pr.PersonUid == personUid && pr.RoleUid == roleUid);

            if (existingAssignment != null)
            {
                return false; // Роль уже назначена
            }

            var personRole = new PersonRole
            {
                Uid = Guid.NewGuid(),
                PersonUid = personUid,
                RoleUid = roleUid,
                AssignedBy = null,
                CreatedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow
            };

            _dbContext.PersonRoles.Add(personRole);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleUid} to person {PersonUid}", roleUid, personUid);
            return false;
        }
    }

    public async Task<bool> RemoveRoleFromUserAsync(Guid personUid, Guid roleUid)
    {
        try
        {
            var personRole = await _dbContext.PersonRoles
                .FirstOrDefaultAsync(pr => pr.PersonUid == personUid && pr.RoleUid == roleUid);

            if (personRole == null)
            {
                return false;
            }

            _dbContext.PersonRoles.Remove(personRole);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleUid} from person {PersonUid}", roleUid, personUid);
            return false;
        }
    }

    public async Task<IEnumerable<Person>> GetUsersInRoleAsync(Guid roleUid)
    {
        return await _dbContext.Persons
            .Where(p => p.PersonRoles.Any(pr => pr.RoleUid == roleUid))
            .ToListAsync();
    }
}
