using System;
using MediatR;

namespace Viridisca.Modules.Academic.Application.Groups.Commands.CreateGroup;

public sealed record CreateGroupCommand(
    string Code,
    string Name,
    string Description,
    int Year,
    DateTime StartDate,
    int MaxStudents,
    Guid DepartmentUid,
    Guid? CuratorUid = null) : IRequest<Guid>;
