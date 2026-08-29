using System;
using MediatR;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Commands.UpdateCourseInstance;

public sealed record UpdateCourseInstanceCommand(Guid CourseInstanceUid, string Name, string Description, int MaxEnrollments) : IRequest<bool>;
