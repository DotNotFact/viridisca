using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Viridisca.Modules.Curriculum.Domain.Models;

namespace Viridisca.Modules.Curriculum.Domain.Repositories;

public interface IAcademicPeriodRepository
{
    Task<AcademicPeriod> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AcademicPeriod>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AcademicPeriod> GetCurrentAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AcademicPeriod>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    void Insert(AcademicPeriod academicPeriod);
    void Update(AcademicPeriod academicPeriod);
    void Delete(AcademicPeriod academicPeriod);
}
