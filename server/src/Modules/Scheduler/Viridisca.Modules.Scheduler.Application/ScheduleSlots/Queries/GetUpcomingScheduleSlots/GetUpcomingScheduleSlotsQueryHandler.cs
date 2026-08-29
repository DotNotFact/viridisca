using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.Dto;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetScheduleSlot;
using Viridisca.Modules.Scheduler.Domain.Repositories;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetUpcomingScheduleSlots;

internal sealed class GetUpcomingScheduleSlotsQueryHandler : IRequestHandler<GetUpcomingScheduleSlotsQuery, List<ScheduleSlotDto>>
{
    private readonly IScheduleSlotRepository _scheduleSlotRepository;

    public GetUpcomingScheduleSlotsQueryHandler(IScheduleSlotRepository scheduleSlotRepository)
    {
        _scheduleSlotRepository = scheduleSlotRepository;
    }

    public async Task<List<ScheduleSlotDto>> Handle(GetUpcomingScheduleSlotsQuery request, CancellationToken cancellationToken)
    {
        var slots = await _scheduleSlotRepository.GetUpcomingAsync(request.Count, cancellationToken);
        return slots.Select(ScheduleSlotMapper.Map).ToList();
    }
}
