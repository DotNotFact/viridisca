using System;
using MediatR;

namespace Viridisca.Modules.Academic.Application.Groups.Commands.UpdateGroup;

public sealed record UpdateGroupCommand(Guid GroupUid, string Name, string Description, int MaxStudents) : IRequest<bool>;
