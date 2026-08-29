using Microsoft.EntityFrameworkCore;
using Viridisca.Modules.Academic.Domain.Models;
using Viridisca.Modules.Academic.Infrastructure.Database;

namespace Viridisca.Modules.Academic.Infrastructure.Repositories;

public class SubjectRepository(AcademicDbContext dbContext) : ISubjectRepository
{
    private readonly AcademicDbContext _dbContext = dbContext;

    public async Task<Subject> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default)
        => await _dbContext.Subjects
            .Include(s => s.Teachers)
            .FirstOrDefaultAsync(s => s.Uid == uid, cancellationToken);

    public async Task<Subject> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _dbContext.Subjects
            .Include(s => s.Teachers)
            .FirstOrDefaultAsync(s => s.Code == code, cancellationToken);

    public async Task<IEnumerable<Subject>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Subjects.ToListAsync(cancellationToken);

    public async Task<IEnumerable<Subject>> GetByDepartmentUidAsync(Guid departmentUid, CancellationToken cancellationToken = default)
        => await _dbContext.Subjects
            .Where(s => s.DepartmentUid == departmentUid)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Subject>> GetActiveSubjectsAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Subjects
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _dbContext.Subjects.AnyAsync(s => s.Code == code, cancellationToken);

    public void Insert(Subject subject) => _dbContext.Subjects.Add(subject);

    public void Update(Subject subject) => _dbContext.Subjects.Update(subject);

    public void Delete(Subject subject) => _dbContext.Subjects.Remove(subject);
}
