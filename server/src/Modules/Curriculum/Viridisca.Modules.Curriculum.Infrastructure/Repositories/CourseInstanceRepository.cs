using Microsoft.EntityFrameworkCore;
using Viridisca.Modules.Curriculum.Domain.Models;
using Viridisca.Modules.Curriculum.Domain.Repositories;
using Viridisca.Modules.Curriculum.Infrastructure.Database;

namespace Viridisca.Modules.Curriculum.Infrastructure.Repositories;

public class CourseInstanceRepository(CurriculumDbContext dbContext) : ICourseInstanceRepository
{
    private readonly CurriculumDbContext _dbContext = dbContext;

    public async Task<CourseInstance> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default)
        => await _dbContext.CourseInstances.FirstOrDefaultAsync(c => c.Uid == uid, cancellationToken);

    public async Task<IReadOnlyList<CourseInstance>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.CourseInstances.OrderByDescending(c => c.StartDate).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CourseInstance>> GetByGroupUidAsync(Guid groupUid, CancellationToken cancellationToken = default)
        => await _dbContext.CourseInstances.Where(c => c.GroupUid == groupUid).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CourseInstance>> GetByTeacherUidAsync(Guid teacherUid, CancellationToken cancellationToken = default)
        => await _dbContext.CourseInstances.Where(c => c.TeacherUid == teacherUid).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CourseInstance>> GetBySubjectUidAsync(Guid subjectUid, CancellationToken cancellationToken = default)
        => await _dbContext.CourseInstances.Where(c => c.SubjectUid == subjectUid).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CourseInstance>> GetByAcademicPeriodUidAsync(Guid academicPeriodUid, CancellationToken cancellationToken = default)
        => await _dbContext.CourseInstances.Where(c => c.AcademicPeriodUid == academicPeriodUid).ToListAsync(cancellationToken);

    public void Insert(CourseInstance courseInstance) => _dbContext.CourseInstances.Add(courseInstance);

    public void Update(CourseInstance courseInstance) => _dbContext.CourseInstances.Update(courseInstance);

    public void Delete(CourseInstance courseInstance) => _dbContext.CourseInstances.Remove(courseInstance);
}
