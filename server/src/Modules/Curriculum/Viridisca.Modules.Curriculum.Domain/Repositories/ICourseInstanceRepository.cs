using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Viridisca.Modules.Curriculum.Domain.Models;

namespace Viridisca.Modules.Curriculum.Domain.Repositories;

public interface ICourseInstanceRepository
{
    Task<CourseInstance> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseInstance>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseInstance>> GetByGroupUidAsync(Guid groupUid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseInstance>> GetByTeacherUidAsync(Guid teacherUid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseInstance>> GetBySubjectUidAsync(Guid subjectUid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseInstance>> GetByAcademicPeriodUidAsync(Guid academicPeriodUid, CancellationToken cancellationToken = default);
    void Insert(CourseInstance courseInstance);
    void Update(CourseInstance courseInstance);
    void Delete(CourseInstance courseInstance);
}
