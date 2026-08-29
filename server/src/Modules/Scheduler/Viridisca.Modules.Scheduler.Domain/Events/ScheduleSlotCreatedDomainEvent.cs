using System;
using Viridisca.Common.Domain;

namespace Viridisca.Modules.Scheduler.Domain.Models;

public sealed class ScheduleSlotCreatedDomainEvent : DomainEvent
{
    public Guid ScheduleSlotUid { get; }

    public ScheduleSlotCreatedDomainEvent(Guid scheduleSlotUid)
    {
        ScheduleSlotUid = scheduleSlotUid;
    }
}
