using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Scheduler.Domain.Repositories;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Commands.UpdateScheduleSlot;

internal sealed class UpdateScheduleSlotCommandHandler : IRequestHandler<UpdateScheduleSlotCommand, bool>
{
    private readonly IScheduleSlotRepository _scheduleSlotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateScheduleSlotCommandHandler(IScheduleSlotRepository scheduleSlotRepository, IUnitOfWork unitOfWork)
    {
        _scheduleSlotRepository = scheduleSlotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateScheduleSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = await _scheduleSlotRepository.GetByUidAsync(request.ScheduleSlotUid, cancellationToken)
            ?? throw new InvalidOperationException($"Слот расписания с ID {request.ScheduleSlotUid} не найден");

        if (!string.IsNullOrWhiteSpace(request.Room))
        {
            var conflicts = await _scheduleSlotRepository.GetRoomConflictsAsync(
                request.Room, request.DayOfWeek, request.StartTime, request.EndTime,
                request.StartDate, request.EndDate, excludeSlotUid: request.ScheduleSlotUid, cancellationToken);

            if (conflicts.Any())
            {
                throw new InvalidOperationException(
                    $"Аудитория {request.Room} уже занята в это время (конфликт со слотом {conflicts[0].Uid})");
            }
        }

        var scheduleResult = slot.UpdateSchedule(request.DayOfWeek, request.StartTime, request.EndTime, request.StartDate, request.EndDate);
        if (scheduleResult.IsFailure)
        {
            throw new InvalidOperationException(scheduleResult.Error.Message);
        }

        slot.UpdateDetails(request.Room, request.Type, request.Notes, request.MaxStudents);

        _scheduleSlotRepository.Update(slot);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
