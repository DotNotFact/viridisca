using Microsoft.EntityFrameworkCore;
using Viridisca.Modules.Curriculum.Domain.Models;
using Viridisca.Modules.Curriculum.Domain.Repositories;
using Viridisca.Modules.Curriculum.Infrastructure.Database;

namespace Viridisca.Modules.Curriculum.Infrastructure.Repositories;

public class AcademicPeriodRepository(CurriculumDbContext dbContext) : IAcademicPeriodRepository
{
    private readonly CurriculumDbContext _dbContext = dbContext;

    public async Task<AcademicPeriod> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default)
        => await _dbContext.AcademicPeriods.FirstOrDefaultAsync(p => p.Uid == uid, cancellationToken);

    public async Task<IReadOnlyList<AcademicPeriod>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.AcademicPeriods.OrderByDescending(p => p.StartDate).ToListAsync(cancellationToken);

    public async Task<AcademicPeriod> GetCurrentAsync(CancellationToken cancellationToken = default)
        => await _dbContext.AcademicPeriods.FirstOrDefaultAsync(p => p.IsCurrent, cancellationToken);

    public async Task<IReadOnlyList<AcademicPeriod>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _dbContext.AcademicPeriods.Where(p => p.Status == AcademicPeriodStatus.Active).ToListAsync(cancellationToken);

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _dbContext.AcademicPeriods.AnyAsync(p => p.Code == code, cancellationToken);

    public void Insert(AcademicPeriod academicPeriod) => _dbContext.AcademicPeriods.Add(academicPeriod);

    public void Update(AcademicPeriod academicPeriod) => _dbContext.AcademicPeriods.Update(academicPeriod);

    public void Delete(AcademicPeriod academicPeriod) => _dbContext.AcademicPeriods.Remove(academicPeriod);
}
