using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Scheduler.Domain.Models;
using Viridisca.Modules.Scheduler.Domain.Repositories;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Commands.CreateScheduleSlot;

internal sealed class CreateScheduleSlotCommandHandler : IRequestHandler<CreateScheduleSlotCommand, Guid>
{
    private readonly IScheduleSlotRepository _scheduleSlotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateScheduleSlotCommandHandler(IScheduleSlotRepository scheduleSlotRepository, IUnitOfWork unitOfWork)
    {
        _scheduleSlotRepository = scheduleSlotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateScheduleSlotCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Room))
        {
            var conflicts = await _scheduleSlotRepository.GetRoomConflictsAsync(
                request.Room, request.DayOfWeek, request.StartTime, request.EndTime,
                request.StartDate, request.EndDate, excludeSlotUid: null, cancellationToken);

            if (conflicts.Any())
            {
                throw new InvalidOperationException(
                    $"Аудитория {request.Room} уже занята в это время (конфликт со слотом {conflicts[0].Uid})");
            }
        }

        var slotResult = ScheduleSlot.Create(
            request.CourseInstanceUid,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime,
            request.StartDate,
            request.EndDate,
            request.Room,
            request.Type,
            request.Notes,
            request.MaxStudents);

        if (slotResult.IsFailure)
        {
            throw new InvalidOperationException(slotResult.Error.Message);
        }

        var slot = slotResult.Value;

        _scheduleSlotRepository.Insert(slot);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return slot.Uid;
    }
}
