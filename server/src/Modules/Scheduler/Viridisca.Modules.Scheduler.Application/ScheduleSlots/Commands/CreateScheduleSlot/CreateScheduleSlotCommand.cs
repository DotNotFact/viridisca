using System;
using MediatR;
using Viridisca.Modules.Scheduler.Domain.Models;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Commands.CreateScheduleSlot;

public sealed record CreateScheduleSlotCommand(
    Guid CourseInstanceUid,
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    DateTime StartDate,
    DateTime? EndDate = null,
    string? Room = null,
    ScheduleSlotType Type = ScheduleSlotType.Lecture,
    string Notes = "",
    int? MaxStudents = null) : IRequest<Guid>;
