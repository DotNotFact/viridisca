using System;
using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.Dto;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetCourseInstancesByGroup;

public sealed record GetCourseInstancesByGroupQuery(Guid GroupUid) : IRequest<List<CourseInstanceDto>>;
