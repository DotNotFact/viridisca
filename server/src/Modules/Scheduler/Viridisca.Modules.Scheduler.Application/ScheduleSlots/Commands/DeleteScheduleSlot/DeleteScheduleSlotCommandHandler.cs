using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Scheduler.Domain.Repositories;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Commands.DeleteScheduleSlot;

internal sealed class DeleteScheduleSlotCommandHandler : IRequestHandler<DeleteScheduleSlotCommand, bool>
{
    private readonly IScheduleSlotRepository _scheduleSlotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteScheduleSlotCommandHandler(IScheduleSlotRepository scheduleSlotRepository, IUnitOfWork unitOfWork)
    {
        _scheduleSlotRepository = scheduleSlotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteScheduleSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = await _scheduleSlotRepository.GetByUidAsync(request.ScheduleSlotUid, cancellationToken)
            ?? throw new InvalidOperationException($"Слот расписания с ID {request.ScheduleSlotUid} не найден");

        _scheduleSlotRepository.Delete(slot);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
