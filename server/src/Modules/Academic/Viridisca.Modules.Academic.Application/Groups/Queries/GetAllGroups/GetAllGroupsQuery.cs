using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Academic.Application.Groups.Queries.GetGroup.Dto;

namespace Viridisca.Modules.Academic.Application.Groups.Queries.GetAllGroups;

public sealed record GetAllGroupsQuery : IRequest<List<GroupDto>>;
