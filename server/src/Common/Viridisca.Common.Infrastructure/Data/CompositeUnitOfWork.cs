using Microsoft.EntityFrameworkCore;
using Viridisca.Common.Application.Data;

namespace Viridisca.Common.Infrastructure.Data;

/// <summary>
/// Each module registers its own DbContext as the base <see cref="DbContext"/> service
/// (in addition to its concrete type) so this composite can find it here. Without this,
/// every module registering `IUnitOfWork` directly against its own DbContext collides —
/// DI resolves only the last registration, so calling SaveChanges from one module's
/// handler could silently save nothing at all if a later-registered module "won" the slot.
/// </summary>
public sealed class CompositeUnitOfWork(IEnumerable<DbContext> dbContexts) : IUnitOfWork
{
    private readonly IReadOnlyCollection<DbContext> _dbContexts = dbContexts.ToList();

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int totalChanges = 0;

        foreach (DbContext dbContext in _dbContexts)
        {
            if (dbContext.ChangeTracker.HasChanges())
            {
                totalChanges += await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return totalChanges;
    }
}
