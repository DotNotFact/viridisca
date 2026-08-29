using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Viridisca.Modules.Curriculum.Domain.Models;

namespace Viridisca.Modules.Curriculum.Domain.Repositories;

public interface ISubmissionRepository
{
    Task<Submission> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Submission>> GetByStudentUidAsync(Guid studentUid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Submission>> GetByAssignmentUidAsync(Guid assignmentUid, CancellationToken cancellationToken = default);
    Task<Submission> GetByStudentAndAssignmentAsync(Guid studentUid, Guid assignmentUid, CancellationToken cancellationToken = default);
    void Insert(Submission submission);
    void Update(Submission submission);
    void Delete(Submission submission);
}
