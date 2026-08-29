using System;
using MediatR;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.Dto;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetCourseInstance;

public sealed record GetCourseInstanceQuery(Guid CourseInstanceUid) : IRequest<CourseInstanceDto>;
