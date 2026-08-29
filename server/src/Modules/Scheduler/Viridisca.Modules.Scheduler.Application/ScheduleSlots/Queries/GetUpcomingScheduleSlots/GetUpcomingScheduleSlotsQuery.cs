using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.Dto;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetUpcomingScheduleSlots;

public sealed record GetUpcomingScheduleSlotsQuery(int Count = 10) : IRequest<List<ScheduleSlotDto>>;
