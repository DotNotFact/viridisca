using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Notifications.Domain.Models;
using Viridisca.Modules.Notifications.Domain.Repositories;

namespace Viridisca.Modules.Notifications.Application.Notifications.Commands.CreateNotification;

internal sealed class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Guid>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNotificationCommandHandler(INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var notificationResult = Notification.Create(
            request.RecipientUid,
            request.Title,
            request.Message,
            request.Type,
            request.Priority,
            request.Category,
            request.ActionUrl);

        if (notificationResult.IsFailure)
        {
            throw new InvalidOperationException(notificationResult.Error.Message);
        }

        var notification = notificationResult.Value;

        _notificationRepository.Insert(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return notification.Uid;
    }
}
