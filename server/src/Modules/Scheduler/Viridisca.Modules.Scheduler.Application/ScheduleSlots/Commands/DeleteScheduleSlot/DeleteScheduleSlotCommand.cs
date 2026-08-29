using System;
using MediatR;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Commands.DeleteScheduleSlot;

public sealed record DeleteScheduleSlotCommand(Guid ScheduleSlotUid) : IRequest<bool>;
