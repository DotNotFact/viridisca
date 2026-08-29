using System;
using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.Dto;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetCourseInstancesByTeacher;

public sealed record GetCourseInstancesByTeacherQuery(Guid TeacherUid) : IRequest<List<CourseInstanceDto>>;
