using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.Dto;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetScheduleSlot;
using Viridisca.Modules.Scheduler.Domain.Repositories;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetAllScheduleSlots;

internal sealed class GetAllScheduleSlotsQueryHandler : IRequestHandler<GetAllScheduleSlotsQuery, List<ScheduleSlotDto>>
{
    private readonly IScheduleSlotRepository _scheduleSlotRepository;

    public GetAllScheduleSlotsQueryHandler(IScheduleSlotRepository scheduleSlotRepository)
    {
        _scheduleSlotRepository = scheduleSlotRepository;
    }

    public async Task<List<ScheduleSlotDto>> Handle(GetAllScheduleSlotsQuery request, CancellationToken cancellationToken)
    {
        var slots = await _scheduleSlotRepository.GetAllAsync(cancellationToken);
        return slots.Select(ScheduleSlotMapper.Map).ToList();
    }
}
