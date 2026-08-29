using System;
using MediatR;

namespace Viridisca.Modules.Curriculum.Application.Assignments.Commands.PublishAssignment;

public sealed record PublishAssignmentCommand(Guid AssignmentUid) : IRequest<bool>;
