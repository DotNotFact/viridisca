using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Viridisca.Modules.Curriculum.Domain.Models;

namespace Viridisca.Modules.Curriculum.Domain.Repositories;

public interface IAssignmentRepository
{
    Task<Assignment> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Assignment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Assignment>> GetByCourseInstanceUidAsync(Guid courseInstanceUid, CancellationToken cancellationToken = default);
    void Insert(Assignment assignment);
    void Update(Assignment assignment);
    void Delete(Assignment assignment);
}
