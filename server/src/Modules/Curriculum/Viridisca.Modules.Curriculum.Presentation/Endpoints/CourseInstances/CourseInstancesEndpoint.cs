using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Commands.AssignTeacher;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Commands.CreateCourseInstance;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Commands.UpdateCourseInstance;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetAllCourseInstances;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetCourseInstance;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetCourseInstancesByGroup;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetCourseInstancesByTeacher;

namespace Viridisca.Modules.Curriculum.Presentation.Endpoints.CourseInstances;

internal sealed class CourseInstancesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/curriculum/course-instances").WithTags("CourseInstances").RequireAuthorization();

        group.MapPost("/", CreateCourseInstance);
        group.MapGet("/", GetAllCourseInstances);
        group.MapGet("/{courseInstanceUid:guid}", GetCourseInstance);
        group.MapPut("/{courseInstanceUid:guid}", UpdateCourseInstance);
        group.MapPut("/{courseInstanceUid:guid}/teacher", AssignTeacher);
        group.MapGet("/by-group/{groupUid:guid}", GetByGroup);
        group.MapGet("/by-teacher/{teacherUid:guid}", GetByTeacher);
    }

    private static async Task<IResult> CreateCourseInstance(CreateCourseInstanceCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var courseInstanceUid = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/curriculum/course-instances/{courseInstanceUid}", courseInstanceUid);
    }

    private static async Task<IResult> GetAllCourseInstances(ISender sender, CancellationToken cancellationToken)
    {
        var courseInstances = await sender.Send(new GetAllCourseInstancesQuery(), cancellationToken);
        return Results.Ok(courseInstances);
    }

    private static async Task<IResult> GetCourseInstance(Guid courseInstanceUid, ISender sender, CancellationToken cancellationToken)
    {
        var courseInstance = await sender.Send(new GetCourseInstanceQuery(courseInstanceUid), cancellationToken);
        return Results.Ok(courseInstance);
    }

    private static async Task<IResult> UpdateCourseInstance(Guid courseInstanceUid, UpdateCourseInstanceRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new UpdateCourseInstanceCommand(courseInstanceUid, request.Name, request.Description, request.MaxEnrollments);
        await sender.Send(command, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> AssignTeacher(Guid courseInstanceUid, AssignTeacherRequest request, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new AssignTeacherToCourseInstanceCommand(courseInstanceUid, request.TeacherUid), cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> GetByGroup(Guid groupUid, ISender sender, CancellationToken cancellationToken)
    {
        var courseInstances = await sender.Send(new GetCourseInstancesByGroupQuery(groupUid), cancellationToken);
        return Results.Ok(courseInstances);
    }

    private static async Task<IResult> GetByTeacher(Guid teacherUid, ISender sender, CancellationToken cancellationToken)
    {
        var courseInstances = await sender.Send(new GetCourseInstancesByTeacherQuery(teacherUid), cancellationToken);
        return Results.Ok(courseInstances);
    }
}

public sealed record UpdateCourseInstanceRequest(string Name, string Description, int MaxEnrollments);

public sealed record AssignTeacherRequest(Guid? TeacherUid);
