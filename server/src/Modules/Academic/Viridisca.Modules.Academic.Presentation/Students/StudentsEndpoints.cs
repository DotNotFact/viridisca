using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Viridisca.Common.Presentation;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Academic.Application.Students.Commands.AddStudentParent;
using Viridisca.Modules.Academic.Application.Students.Commands.AssignStudentToGroup;
using Viridisca.Modules.Academic.Application.Students.Commands.CreateStudent;
using Viridisca.Modules.Academic.Application.Students.Commands.UpdateStudent;
using Viridisca.Modules.Academic.Application.Students.Queries.GetStudent;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Presentation.Students;

internal sealed class StudentsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/academic/students").WithTags(Tags.Students).RequireAuthorization();

        group.MapPost("/", CreateStudent);
        group.MapGet("/{studentUid:guid}", GetStudent);
        group.MapPut("/{studentUid:guid}", UpdateStudent);
        group.MapPost("/{studentUid:guid}/group", AssignToGroup);
        group.MapPost("/{studentUid:guid}/parents", AddParent);
    }

    private static async Task<IResult> CreateStudent(CreateStudentCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var studentUid = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/academic/students/{studentUid}", studentUid);
    }

    private static async Task<IResult> GetStudent(Guid studentUid, ISender sender, CancellationToken cancellationToken)
    {
        var student = await sender.Send(new GetStudentQuery(studentUid), cancellationToken);
        return Results.Ok(student);
    }

    private static async Task<IResult> UpdateStudent(Guid studentUid, UpdateStudentRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new UpdateStudentCommand(studentUid, request.EmergencyContactName, request.EmergencyContactPhone, request.MedicalInformation);
        await sender.Send(command, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> AssignToGroup(Guid studentUid, AssignToGroupRequest request, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new AssignStudentToGroupCommand(studentUid, request.GroupUid), cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> AddParent(Guid studentUid, AddParentRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new AddStudentParentCommand(studentUid, request.ParentUserUid, request.Relation, request.IsPrimaryContact, request.HasLegalGuardianship);
        var parentUid = await sender.Send(command, cancellationToken);
        return Results.Ok(parentUid);
    }
}

public sealed record UpdateStudentRequest(string EmergencyContactName, string EmergencyContactPhone, string MedicalInformation);

public sealed record AssignToGroupRequest(Guid GroupUid);

public sealed record AddParentRequest(Guid ParentUserUid, ParentRelationType Relation, bool IsPrimaryContact = false, bool HasLegalGuardianship = false);
