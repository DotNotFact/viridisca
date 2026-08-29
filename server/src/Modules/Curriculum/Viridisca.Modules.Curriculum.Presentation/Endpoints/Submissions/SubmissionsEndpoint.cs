using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Curriculum.Application.Submissions.Commands.CreateSubmission;
using Viridisca.Modules.Curriculum.Application.Submissions.Commands.GradeSubmission;
using Viridisca.Modules.Curriculum.Application.Submissions.Queries.GetSubmission;
using Viridisca.Modules.Curriculum.Application.Submissions.Queries.GetSubmissionsByAssignment;
using Viridisca.Modules.Curriculum.Application.Submissions.Queries.GetSubmissionsByStudent;

namespace Viridisca.Modules.Curriculum.Presentation.Endpoints.Submissions;

internal sealed class SubmissionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/curriculum/submissions").WithTags("Submissions").RequireAuthorization();

        group.MapPost("/", CreateSubmission);
        group.MapGet("/{submissionUid:guid}", GetSubmission);
        group.MapPost("/{submissionUid:guid}/grade", GradeSubmission);
        group.MapGet("/by-student/{studentUid:guid}", GetByStudent);
        group.MapGet("/by-assignment/{assignmentUid:guid}", GetByAssignment);
    }

    private static async Task<IResult> CreateSubmission(CreateSubmissionCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var submissionUid = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/curriculum/submissions/{submissionUid}", submissionUid);
    }

    private static async Task<IResult> GetSubmission(Guid submissionUid, ISender sender, CancellationToken cancellationToken)
    {
        var submission = await sender.Send(new GetSubmissionQuery(submissionUid), cancellationToken);
        return Results.Ok(submission);
    }

    private static async Task<IResult> GradeSubmission(Guid submissionUid, GradeSubmissionRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new GradeSubmissionCommand(submissionUid, request.Score, request.Feedback, request.GradedByUid);
        await sender.Send(command, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> GetByStudent(Guid studentUid, ISender sender, CancellationToken cancellationToken)
    {
        var submissions = await sender.Send(new GetSubmissionsByStudentQuery(studentUid), cancellationToken);
        return Results.Ok(submissions);
    }

    private static async Task<IResult> GetByAssignment(Guid assignmentUid, ISender sender, CancellationToken cancellationToken)
    {
        var submissions = await sender.Send(new GetSubmissionsByAssignmentQuery(assignmentUid), cancellationToken);
        return Results.Ok(submissions);
    }
}

public sealed record GradeSubmissionRequest(decimal Score, string Feedback, Guid GradedByUid);
