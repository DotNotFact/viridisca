using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Viridisca.Common.Presentation;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Academic.Application.Teachers.Commands.AssignSubjectToTeacher;
using Viridisca.Modules.Academic.Application.Teachers.Commands.CreateTeacher;
using Viridisca.Modules.Academic.Application.Teachers.Queries.GetTeacher;

namespace Viridisca.Modules.Academic.Presentation.Teachers;

internal sealed class TeachersEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/academic/teachers").WithTags(Tags.Teachers).RequireAuthorization();

        group.MapPost("/", CreateTeacher);
        group.MapGet("/{teacherUid:guid}", GetTeacher);
        group.MapPost("/{teacherUid:guid}/subjects", AssignSubject);
    }

    private static async Task<IResult> CreateTeacher(CreateTeacherCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var teacherUid = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/academic/teachers/{teacherUid}", teacherUid);
    }

    private static async Task<IResult> GetTeacher(Guid teacherUid, ISender sender, CancellationToken cancellationToken)
    {
        var teacher = await sender.Send(new GetTeacherQuery(teacherUid), cancellationToken);
        return Results.Ok(teacher);
    }

    private static async Task<IResult> AssignSubject(Guid teacherUid, AssignSubjectRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new AssignSubjectToTeacherCommand(teacherUid, request.SubjectUid, request.IsMainTeacher);
        var teacherSubjectUid = await sender.Send(command, cancellationToken);
        return Results.Ok(teacherSubjectUid);
    }
}

public sealed record AssignSubjectRequest(Guid SubjectUid, bool IsMainTeacher = false);
