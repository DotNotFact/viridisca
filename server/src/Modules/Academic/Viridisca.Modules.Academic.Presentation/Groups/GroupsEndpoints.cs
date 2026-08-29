using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Viridisca.Common.Presentation;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Academic.Application.Groups.Commands.CreateGroup;
using Viridisca.Modules.Academic.Application.Groups.Commands.SetGroupCurator;
using Viridisca.Modules.Academic.Application.Groups.Commands.UpdateGroup;
using Viridisca.Modules.Academic.Application.Groups.Queries.GetAllGroups;
using Viridisca.Modules.Academic.Application.Groups.Queries.GetGroup;

namespace Viridisca.Modules.Academic.Presentation.Groups;

internal sealed class GroupsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/academic/groups").WithTags(Tags.Groups).RequireAuthorization();

        group.MapPost("/", CreateGroup);
        group.MapGet("/", GetAllGroups);
        group.MapGet("/{groupUid:guid}", GetGroup);
        group.MapPut("/{groupUid:guid}", UpdateGroup);
        group.MapPut("/{groupUid:guid}/curator", SetCurator);
    }

    private static async Task<IResult> CreateGroup(CreateGroupCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var groupUid = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/academic/groups/{groupUid}", groupUid);
    }

    private static async Task<IResult> GetAllGroups(ISender sender, CancellationToken cancellationToken)
    {
        var groups = await sender.Send(new GetAllGroupsQuery(), cancellationToken);
        return Results.Ok(groups);
    }

    private static async Task<IResult> GetGroup(Guid groupUid, ISender sender, CancellationToken cancellationToken)
    {
        var group = await sender.Send(new GetGroupQuery(groupUid), cancellationToken);
        return Results.Ok(group);
    }

    private static async Task<IResult> UpdateGroup(Guid groupUid, UpdateGroupRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new UpdateGroupCommand(groupUid, request.Name, request.Description, request.MaxStudents);
        await sender.Send(command, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> SetCurator(Guid groupUid, SetCuratorRequest request, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new SetGroupCuratorCommand(groupUid, request.CuratorUid), cancellationToken);
        return Results.Ok();
    }
}

public sealed record UpdateGroupRequest(string Name, string Description, int MaxStudents);

public sealed record SetCuratorRequest(Guid? CuratorUid);
