using System;
using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Curriculum.Application.Assignments.Queries.Dto;

namespace Viridisca.Modules.Curriculum.Application.Assignments.Queries.GetAssignmentsByCourseInstance;

public sealed record GetAssignmentsByCourseInstanceQuery(Guid CourseInstanceUid) : IRequest<List<AssignmentDto>>;
