using Microsoft.EntityFrameworkCore;
using Viridisca.Modules.Identity.Domain.Models;
using Viridisca.Modules.Identity.Domain.Repositories;
using Viridisca.Modules.Identity.Infrastructure.Database;

namespace Viridisca.Modules.Identity.Infrastructure.Roles;

public class RoleRepository : IRoleRepository
{
    private readonly IdentityDbContext _dbContext;

    public RoleRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }
}
