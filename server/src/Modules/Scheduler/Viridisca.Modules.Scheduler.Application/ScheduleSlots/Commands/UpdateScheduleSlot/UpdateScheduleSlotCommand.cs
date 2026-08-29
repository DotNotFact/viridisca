using System;
using MediatR;
using Viridisca.Modules.Scheduler.Domain.Models;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Commands.UpdateScheduleSlot;

public sealed record UpdateScheduleSlotCommand(
    Guid ScheduleSlotUid,
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    DateTime StartDate,
    DateTime? EndDate,
    string? Room,
    ScheduleSlotType Type,
    string Notes,
    int? MaxStudents) : IRequest<bool>;
