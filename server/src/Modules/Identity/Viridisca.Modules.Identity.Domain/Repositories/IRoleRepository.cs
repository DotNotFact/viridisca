using Viridisca.Modules.Identity.Domain.Models;

namespace Viridisca.Modules.Identity.Domain.Repositories;

public interface IRoleRepository
{
    /// <summary>
    /// Get role by name
    /// </summary>
    /// <param name="name">Role name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Role or null if not found</returns>
    Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
