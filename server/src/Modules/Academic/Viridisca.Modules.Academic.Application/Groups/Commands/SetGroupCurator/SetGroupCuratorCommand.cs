using System;
using MediatR;

namespace Viridisca.Modules.Academic.Application.Groups.Commands.SetGroupCurator;

public sealed record SetGroupCuratorCommand(Guid GroupUid, Guid? CuratorUid) : IRequest<bool>;
