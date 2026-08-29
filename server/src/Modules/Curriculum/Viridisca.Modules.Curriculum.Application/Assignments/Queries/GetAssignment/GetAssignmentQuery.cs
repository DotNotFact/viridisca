using System;
using MediatR;
using Viridisca.Modules.Curriculum.Application.Assignments.Queries.Dto;

namespace Viridisca.Modules.Curriculum.Application.Assignments.Queries.GetAssignment;

public sealed record GetAssignmentQuery(Guid AssignmentUid) : IRequest<AssignmentDto>;
