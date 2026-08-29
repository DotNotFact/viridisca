using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Commands.CreateScheduleSlot;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Commands.DeleteScheduleSlot;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Commands.UpdateScheduleSlot;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetAllScheduleSlots;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetScheduleSlot;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetScheduleSlotsByCourseInstance;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetUpcomingScheduleSlots;
using Viridisca.Modules.Scheduler.Domain.Models;

namespace Viridisca.Modules.Scheduler.Presentation.Endpoints.ScheduleSlots;

internal sealed class ScheduleSlotsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/scheduler/schedule-slots").WithTags("ScheduleSlots").RequireAuthorization();

        group.MapPost("/", CreateSlot);
        group.MapGet("/", GetAllSlots);
        group.MapGet("/upcoming", GetUpcomingSlots);
        group.MapGet("/by-course-instance/{courseInstanceUid:guid}", GetSlotsByCourseInstance);
        group.MapGet("/{scheduleSlotUid:guid}", GetSlot);
        group.MapPut("/{scheduleSlotUid:guid}", UpdateSlot);
        group.MapDelete("/{scheduleSlotUid:guid}", DeleteSlot);
    }

    private static async Task<IResult> CreateSlot(CreateScheduleSlotCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var slotUid = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/scheduler/schedule-slots/{slotUid}", slotUid);
    }

    private static async Task<IResult> GetAllSlots(ISender sender, CancellationToken cancellationToken)
    {
        var slots = await sender.Send(new GetAllScheduleSlotsQuery(), cancellationToken);
        return Results.Ok(slots);
    }

    private static async Task<IResult> GetUpcomingSlots(ISender sender, CancellationToken cancellationToken, int count = 10)
    {
        var slots = await sender.Send(new GetUpcomingScheduleSlotsQuery(count), cancellationToken);
        return Results.Ok(slots);
    }

    private static async Task<IResult> GetSlotsByCourseInstance(Guid courseInstanceUid, ISender sender, CancellationToken cancellationToken)
    {
        var slots = await sender.Send(new GetScheduleSlotsByCourseInstanceQuery(courseInstanceUid), cancellationToken);
        return Results.Ok(slots);
    }

    private static async Task<IResult> GetSlot(Guid scheduleSlotUid, ISender sender, CancellationToken cancellationToken)
    {
        var slot = await sender.Send(new GetScheduleSlotQuery(scheduleSlotUid), cancellationToken);
        return Results.Ok(slot);
    }

    private static async Task<IResult> UpdateSlot(Guid scheduleSlotUid, UpdateScheduleSlotRequest request, ISender sender, CancellationToken cancellationToken)
    {
        var command = new UpdateScheduleSlotCommand(
            scheduleSlotUid, request.DayOfWeek, request.StartTime, request.EndTime,
            request.StartDate, request.EndDate, request.Room, request.Type, request.Notes, request.MaxStudents);

        await sender.Send(command, cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> DeleteSlot(Guid scheduleSlotUid, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteScheduleSlotCommand(scheduleSlotUid), cancellationToken);
        return Results.Ok();
    }
}

public sealed record UpdateScheduleSlotRequest(
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    DateTime StartDate,
    DateTime? EndDate,
    string? Room,
    ScheduleSlotType Type,
    string Notes,
    int? MaxStudents);
