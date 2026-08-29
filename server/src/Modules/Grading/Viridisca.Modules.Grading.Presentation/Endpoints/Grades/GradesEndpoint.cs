using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Grading.Application.Grades.Commands.CreateGrade;
using Viridisca.Modules.Grading.Application.Grades.Commands.PublishGrade;
using Viridisca.Modules.Grading.Application.Grades.Commands.UnpublishGrade;
using Viridisca.Modules.Grading.Application.Grades.Commands.UpdateGrade;
using Viridisca.Modules.Grading.Application.Grades.Queries.GetGrade;
using Viridisca.Modules.Grading.Application.Grades.Queries.GetGradesByStudent;
using Viridisca.Modules.Grading.Application.Grades.Queries.GetGradesBySubject;

namespace Viridisca.Modules.Grading.Presentation.Endpoints.Grades;

internal sealed class GradesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/grading/grades").WithTags("Grades").RequireAuthorization();

        group.MapPost("/", CreateGrade);
        group.MapGet("/{gradeUid:guid}", GetGrade);
        group.MapPut("/{gradeUid:guid}", UpdateGrade);
        group.MapPost("/{gradeUid:guid}/publish", PublishGrade);
        group.MapPost("/{gradeUid:guid}/unpublish", UnpublishGrade);
        group.MapGet("/by-student/{studentUid:guid}", GetGradesByStudent);
        group.MapGet("/by-subject/{subjectUid:guid}", GetGradesBySubject);
    }

    private static async Task<IResult> CreateGrade(CreateGradeCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var gradeUid = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/grading/grades/{gradeUid}", gradeUid);
    }

    private static async Task<IResult> GetGrade(Guid gradeUid, ISender sender, CancellationToken cancellationToken)
    {
        var grade = await sender.Send(new GetGradeQuery(gradeUid), cancellationToken);
        return Results.Ok(grade);
    }

    private static async Task<IResult> UpdateGrade(Guid gradeUid, UpdateGradeRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new UpdateGradeCommand(gradeUid, request.Value, request.Description, request.Reason);
        await sender.Send(command, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> PublishGrade(Guid gradeUid, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new PublishGradeCommand(gradeUid), cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> UnpublishGrade(Guid gradeUid, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new UnpublishGradeCommand(gradeUid), cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> GetGradesByStudent(Guid studentUid, ISender sender, CancellationToken cancellationToken)
    {
        var grades = await sender.Send(new GetGradesByStudentQuery(studentUid), cancellationToken);
        return Results.Ok(grades);
    }

    private static async Task<IResult> GetGradesBySubject(Guid subjectUid, ISender sender, CancellationToken cancellationToken)
    {
        var grades = await sender.Send(new GetGradesBySubjectQuery(subjectUid), cancellationToken);
        return Results.Ok(grades);
    }
}

public sealed record UpdateGradeRequest(decimal Value, string? Description = null, string? Reason = null);
