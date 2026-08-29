using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Curriculum.Application.AcademicPeriods.Commands.CreateAcademicPeriod;
using Viridisca.Modules.Curriculum.Application.AcademicPeriods.Commands.SetCurrentAcademicPeriod;
using Viridisca.Modules.Curriculum.Application.AcademicPeriods.Commands.UpdateAcademicPeriod;
using Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.GetAcademicPeriod;
using Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.GetAllAcademicPeriods;
using Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.GetCurrentAcademicPeriod;

namespace Viridisca.Modules.Curriculum.Presentation.Endpoints.AcademicPeriods;

internal sealed class AcademicPeriodsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/curriculum/academic-periods").WithTags("AcademicPeriods").RequireAuthorization();

        group.MapPost("/", CreatePeriod);
        group.MapGet("/", GetAllPeriods);
        group.MapGet("/current", GetCurrentPeriod);
        group.MapGet("/{periodUid:guid}", GetPeriod);
        group.MapPut("/{periodUid:guid}", UpdatePeriod);
        group.MapPut("/{periodUid:guid}/current", SetCurrent);
    }

    private static async Task<IResult> CreatePeriod(CreateAcademicPeriodCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var periodUid = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/curriculum/academic-periods/{periodUid}", periodUid);
    }

    private static async Task<IResult> GetAllPeriods(ISender sender, CancellationToken cancellationToken)
    {
        var periods = await sender.Send(new GetAllAcademicPeriodsQuery(), cancellationToken);
        return Results.Ok(periods);
    }

    private static async Task<IResult> GetCurrentPeriod(ISender sender, CancellationToken cancellationToken)
    {
        var period = await sender.Send(new GetCurrentAcademicPeriodQuery(), cancellationToken);
        return period is null ? Results.NotFound() : Results.Ok(period);
    }

    private static async Task<IResult> GetPeriod(Guid periodUid, ISender sender, CancellationToken cancellationToken)
    {
        var period = await sender.Send(new GetAcademicPeriodQuery(periodUid), cancellationToken);
        return Results.Ok(period);
    }

    private static async Task<IResult> UpdatePeriod(Guid periodUid, UpdateAcademicPeriodRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new UpdateAcademicPeriodCommand(periodUid, request.Name, request.Description, request.StartDate, request.EndDate);
        await sender.Send(command, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> SetCurrent(Guid periodUid, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new SetCurrentAcademicPeriodCommand(periodUid), cancellationToken);
        return Results.Ok();
    }
}

public sealed record UpdateAcademicPeriodRequest(string Name, string Description, DateTime StartDate, DateTime EndDate);
