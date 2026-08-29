using Microsoft.EntityFrameworkCore;
using Viridisca.Modules.Academic.Domain.Groups;
using Viridisca.Modules.Academic.Infrastructure.Database;

namespace Viridisca.Modules.Academic.Infrastructure.Repositories;

public class GroupRepository(AcademicDbContext dbContext) : IGroupRepository
{
    private readonly AcademicDbContext _dbContext = dbContext;

    public async Task<Group?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Groups.FirstOrDefaultAsync(g => g.Uid == id, cancellationToken);

    public async Task<Group?> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default)
        => await _dbContext.Groups.FirstOrDefaultAsync(g => g.Uid == uid, cancellationToken);

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _dbContext.Groups.AnyAsync(g => g.Code == code, cancellationToken);

    public async Task<List<Group>> GetByDepartmentAsync(Guid departmentUid, CancellationToken cancellationToken = default)
        => await _dbContext.Groups.Where(g => g.DepartmentUid == departmentUid).ToListAsync(cancellationToken);

    public async Task<List<Group>> GetByCuratorAsync(Guid curatorUid, CancellationToken cancellationToken = default)
        => await _dbContext.Groups.Where(g => g.CuratorUid == curatorUid).ToListAsync(cancellationToken);

    public async Task<List<Group>> GetByStatusAsync(GroupStatus status, CancellationToken cancellationToken = default)
        => await _dbContext.Groups.Where(g => g.Status == status).ToListAsync(cancellationToken);

    public async Task<List<Group>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Groups.ToListAsync(cancellationToken);

    public void Insert(Group group) => _dbContext.Groups.Add(group);

    public void Update(Group group) => _dbContext.Groups.Update(group);

    public void Delete(Group group) => _dbContext.Groups.Remove(group);
}
