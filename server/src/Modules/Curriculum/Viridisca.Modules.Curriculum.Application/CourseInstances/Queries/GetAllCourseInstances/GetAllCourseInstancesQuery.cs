using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.Dto;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetAllCourseInstances;

public sealed record GetAllCourseInstancesQuery : IRequest<List<CourseInstanceDto>>;
