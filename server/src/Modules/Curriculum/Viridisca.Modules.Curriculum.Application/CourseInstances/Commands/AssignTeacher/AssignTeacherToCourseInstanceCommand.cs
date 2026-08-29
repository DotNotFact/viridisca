using System;
using MediatR;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Commands.AssignTeacher;

public sealed record AssignTeacherToCourseInstanceCommand(Guid CourseInstanceUid, Guid? TeacherUid) : IRequest<bool>;
