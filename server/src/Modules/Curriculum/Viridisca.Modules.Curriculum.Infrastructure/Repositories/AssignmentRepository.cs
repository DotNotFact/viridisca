using Microsoft.EntityFrameworkCore;
using Viridisca.Modules.Curriculum.Domain.Models;
using Viridisca.Modules.Curriculum.Domain.Repositories;
using Viridisca.Modules.Curriculum.Infrastructure.Database;

namespace Viridisca.Modules.Curriculum.Infrastructure.Repositories;

public class AssignmentRepository(CurriculumDbContext dbContext) : IAssignmentRepository
{
    private readonly CurriculumDbContext _dbContext = dbContext;

    public async Task<Assignment> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default)
        => await _dbContext.Assignments.FirstOrDefaultAsync(a => a.Uid == uid, cancellationToken);

    public async Task<IReadOnlyList<Assignment>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Assignments.OrderByDescending(a => a.CreatedAtUtc).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Assignment>> GetByCourseInstanceUidAsync(Guid courseInstanceUid, CancellationToken cancellationToken = default)
        => await _dbContext.Assignments.Where(a => a.CourseInstanceUid == courseInstanceUid).ToListAsync(cancellationToken);

    public void Insert(Assignment assignment) => _dbContext.Assignments.Add(assignment);

    public void Update(Assignment assignment) => _dbContext.Assignments.Update(assignment);

    public void Delete(Assignment assignment) => _dbContext.Assignments.Remove(assignment);
}
