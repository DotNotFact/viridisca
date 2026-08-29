using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.Dto;
using Viridisca.Modules.Scheduler.Domain.Models;
using Viridisca.Modules.Scheduler.Domain.Repositories;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.GetScheduleSlot;

internal sealed class GetScheduleSlotQueryHandler : IRequestHandler<GetScheduleSlotQuery, ScheduleSlotDto>
{
    private readonly IScheduleSlotRepository _scheduleSlotRepository;

    public GetScheduleSlotQueryHandler(IScheduleSlotRepository scheduleSlotRepository)
    {
        _scheduleSlotRepository = scheduleSlotRepository;
    }

    public async Task<ScheduleSlotDto> Handle(GetScheduleSlotQuery request, CancellationToken cancellationToken)
    {
        var slot = await _scheduleSlotRepository.GetByUidAsync(request.ScheduleSlotUid, cancellationToken)
            ?? throw new InvalidOperationException($"Слот расписания с ID {request.ScheduleSlotUid} не найден");

        return ScheduleSlotMapper.Map(slot);
    }
}

internal static class ScheduleSlotMapper
{
    public static ScheduleSlotDto Map(ScheduleSlot slot) => new()
    {
        Uid = slot.Uid,
        CourseInstanceUid = slot.CourseInstanceUid,
        DayOfWeek = slot.DayOfWeek.ToString(),
        StartTime = slot.StartTime,
        EndTime = slot.EndTime,
        Room = slot.Room,
        StartDate = slot.StartDate,
        EndDate = slot.EndDate,
        Type = slot.Type.ToString(),
        IsActive = slot.IsActive,
        Notes = slot.Notes,
        MaxStudents = slot.MaxStudents,
        CreatedAtUtc = slot.CreatedAtUtc,
    };
}
