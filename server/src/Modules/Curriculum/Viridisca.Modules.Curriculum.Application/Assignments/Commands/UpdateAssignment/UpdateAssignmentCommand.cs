using System;
using MediatR;

namespace Viridisca.Modules.Curriculum.Application.Assignments.Commands.UpdateAssignment;

public sealed record UpdateAssignmentCommand(Guid AssignmentUid, string Title, string Description, string Instructions, DateTime? DueDate, decimal MaxScore) : IRequest<bool>;
