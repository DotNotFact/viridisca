using Microsoft.EntityFrameworkCore;
using Viridisca.Modules.Curriculum.Domain.Models;
using Viridisca.Modules.Curriculum.Domain.Repositories;
using Viridisca.Modules.Curriculum.Infrastructure.Database;

namespace Viridisca.Modules.Curriculum.Infrastructure.Repositories;

public class SubmissionRepository(CurriculumDbContext dbContext) : ISubmissionRepository
{
    private readonly CurriculumDbContext _dbContext = dbContext;

    public async Task<Submission> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default)
        => await _dbContext.Submissions.FirstOrDefaultAsync(s => s.Uid == uid, cancellationToken);

    public async Task<IReadOnlyList<Submission>> GetByStudentUidAsync(Guid studentUid, CancellationToken cancellationToken = default)
        => await _dbContext.Submissions.Where(s => s.StudentUid == studentUid).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Submission>> GetByAssignmentUidAsync(Guid assignmentUid, CancellationToken cancellationToken = default)
        => await _dbContext.Submissions.Where(s => s.AssignmentUid == assignmentUid).ToListAsync(cancellationToken);

    public async Task<Submission> GetByStudentAndAssignmentAsync(Guid studentUid, Guid assignmentUid, CancellationToken cancellationToken = default)
        => await _dbContext.Submissions.FirstOrDefaultAsync(s => s.StudentUid == studentUid && s.AssignmentUid == assignmentUid, cancellationToken);

    public void Insert(Submission submission) => _dbContext.Submissions.Add(submission);

    public void Update(Submission submission) => _dbContext.Submissions.Update(submission);

    public void Delete(Submission submission) => _dbContext.Submissions.Remove(submission);
}
