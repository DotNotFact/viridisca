using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Notifications.Domain.Repositories;

namespace Viridisca.Modules.Notifications.Application.Notifications.Commands.DeleteNotification;

internal sealed class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, bool>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNotificationCommandHandler(INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByUidAsync(request.NotificationUid, cancellationToken)
            ?? throw new InvalidOperationException($"Уведомление с ID {request.NotificationUid} не найдено");

        _notificationRepository.Delete(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
