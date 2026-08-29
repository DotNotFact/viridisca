using System;
using MediatR;
using Viridisca.Modules.Academic.Application.Groups.Queries.GetGroup.Dto;

namespace Viridisca.Modules.Academic.Application.Groups.Queries.GetGroup;

public sealed record GetGroupQuery(Guid GroupUid) : IRequest<GroupDto>;
