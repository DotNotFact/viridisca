using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Curriculum.Application.Assignments.Commands.CreateAssignment;
using Viridisca.Modules.Curriculum.Application.Assignments.Commands.PublishAssignment;
using Viridisca.Modules.Curriculum.Application.Assignments.Commands.UpdateAssignment;
using Viridisca.Modules.Curriculum.Application.Assignments.Queries.GetAssignment;
using Viridisca.Modules.Curriculum.Application.Assignments.Queries.GetAssignmentsByCourseInstance;

namespace Viridisca.Modules.Curriculum.Presentation.Endpoints.Assignments;

internal sealed class AssignmentsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/curriculum/assignments").WithTags("Assignments").RequireAuthorization();

        group.MapPost("/", CreateAssignment);
        group.MapGet("/{assignmentUid:guid}", GetAssignment);
        group.MapPut("/{assignmentUid:guid}", UpdateAssignment);
        group.MapPost("/{assignmentUid:guid}/publish", PublishAssignment);
        group.MapGet("/by-course-instance/{courseInstanceUid:guid}", GetByCourseInstance);
    }

    private static async Task<IResult> CreateAssignment(CreateAssignmentCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var assignmentUid = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/curriculum/assignments/{assignmentUid}", assignmentUid);
    }

    private static async Task<IResult> GetAssignment(Guid assignmentUid, ISender sender, CancellationToken cancellationToken)
    {
        var assignment = await sender.Send(new GetAssignmentQuery(assignmentUid), cancellationToken);
        return Results.Ok(assignment);
    }

    private static async Task<IResult> UpdateAssignment(Guid assignmentUid, UpdateAssignmentRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new UpdateAssignmentCommand(assignmentUid, request.Title, request.Description, request.Instructions, request.DueDate, request.MaxScore);
        await sender.Send(command, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> PublishAssignment(Guid assignmentUid, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new PublishAssignmentCommand(assignmentUid), cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> GetByCourseInstance(Guid courseInstanceUid, ISender sender, CancellationToken cancellationToken)
    {
        var assignments = await sender.Send(new GetAssignmentsByCourseInstanceQuery(courseInstanceUid), cancellationToken);
        return Results.Ok(assignments);
    }
}

public sealed record UpdateAssignmentRequest(string Title, string Description, string Instructions, DateTime? DueDate, decimal MaxScore);
