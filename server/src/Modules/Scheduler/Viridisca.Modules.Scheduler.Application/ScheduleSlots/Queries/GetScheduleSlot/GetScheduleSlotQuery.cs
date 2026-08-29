using System;
using MediatR;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.Dto;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetScheduleSlot;

public sealed record GetScheduleSlotQuery(Guid ScheduleSlotUid) : IRequest<ScheduleSlotDto>;
