using System;
using MediatR;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Commands.CreateCourseInstance;

public sealed record CreateCourseInstanceCommand(
    Guid SubjectUid,
    Guid GroupUid,
    Guid AcademicPeriodUid,
    string Name,
    string Code,
    DateTime StartDate,
    Guid? TeacherUid = null,
    string Description = "",
    int MaxEnrollments = 30) : IRequest<Guid>;
