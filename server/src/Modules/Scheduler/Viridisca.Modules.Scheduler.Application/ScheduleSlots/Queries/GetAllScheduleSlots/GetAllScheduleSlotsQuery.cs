using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.Dto;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetAllScheduleSlots;

public sealed record GetAllScheduleSlotsQuery : IRequest<List<ScheduleSlotDto>>;
