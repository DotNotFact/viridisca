using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Viridisca.Common.Presentation;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Academic.Application.Subjects.Commands.CreateSubject;
using Viridisca.Modules.Academic.Application.Subjects.Commands.UpdateSubject;
using Viridisca.Modules.Academic.Application.Subjects.Queries.GetAllSubjects;
using Viridisca.Modules.Academic.Application.Subjects.Queries.GetSubject;

namespace Viridisca.Modules.Academic.Presentation.Subjects;

internal sealed class SubjectsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/academic/subjects").WithTags(Tags.Subjects).RequireAuthorization();

        group.MapPost("/", CreateSubject);
        group.MapGet("/", GetAllSubjects);
        group.MapGet("/{subjectUid:guid}", GetSubject);
        group.MapPut("/{subjectUid:guid}", UpdateSubject);
    }

    private static async Task<IResult> CreateSubject(CreateSubjectCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var subjectUid = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/academic/subjects/{subjectUid}", subjectUid);
    }

    private static async Task<IResult> GetAllSubjects(ISender sender, CancellationToken cancellationToken)
    {
        var subjects = await sender.Send(new GetAllSubjectsQuery(), cancellationToken);
        return Results.Ok(subjects);
    }

    private static async Task<IResult> GetSubject(Guid subjectUid, ISender sender, CancellationToken cancellationToken)
    {
        var subject = await sender.Send(new GetSubjectQuery(subjectUid), cancellationToken);
        return Results.Ok(subject);
    }

    private static async Task<IResult> UpdateSubject(Guid subjectUid, UpdateSubjectRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new UpdateSubjectCommand(subjectUid, request.Name, request.Description, request.Credits);
        await sender.Send(command, cancellationToken);
        return Results.Ok();
    }
}

public sealed record UpdateSubjectRequest(string Name, string Description, int Credits);
