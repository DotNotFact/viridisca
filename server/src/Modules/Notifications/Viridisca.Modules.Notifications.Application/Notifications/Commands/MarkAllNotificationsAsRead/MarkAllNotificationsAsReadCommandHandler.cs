using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Notifications.Domain.Repositories;

namespace Viridisca.Modules.Notifications.Application.Notifications.Commands.MarkAllNotificationsAsRead;

internal sealed class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, int>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllNotificationsAsReadCommandHandler(INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await _notificationRepository.GetUnreadByRecipientAsync(request.RecipientUid, cancellationToken);

        foreach (var notification in unread)
        {
            notification.MarkAsRead();
            _notificationRepository.Update(notification);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return unread.Count;
    }
}
