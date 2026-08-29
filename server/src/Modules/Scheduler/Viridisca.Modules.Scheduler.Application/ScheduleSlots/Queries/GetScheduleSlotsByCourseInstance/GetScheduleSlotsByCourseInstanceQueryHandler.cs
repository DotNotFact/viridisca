using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.Dto;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetScheduleSlot;
using Viridisca.Modules.Scheduler.Domain.Repositories;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetScheduleSlotsByCourseInstance;

internal sealed class GetScheduleSlotsByCourseInstanceQueryHandler : IRequestHandler<GetScheduleSlotsByCourseInstanceQuery, List<ScheduleSlotDto>>
{
    private readonly IScheduleSlotRepository _scheduleSlotRepository;

    public GetScheduleSlotsByCourseInstanceQueryHandler(IScheduleSlotRepository scheduleSlotRepository)
    {
        _scheduleSlotRepository = scheduleSlotRepository;
    }

    public async Task<List<ScheduleSlotDto>> Handle(GetScheduleSlotsByCourseInstanceQuery request, CancellationToken cancellationToken)
    {
        var slots = await _scheduleSlotRepository.GetByCourseInstanceAsync(request.CourseInstanceUid, cancellationToken);
        return slots.Select(ScheduleSlotMapper.Map).ToList();
    }
}
