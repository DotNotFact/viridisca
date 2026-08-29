using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Notifications.Application.Notifications.Queries.Dto;
using Viridisca.Modules.Notifications.Application.Notifications.Queries.GetUnreadNotifications;
using Viridisca.Modules.Notifications.Domain.Repositories;

namespace Viridisca.Modules.Notifications.Application.Notifications.Queries.GetRecentNotifications;

internal sealed class GetRecentNotificationsQueryHandler : IRequestHandler<GetRecentNotificationsQuery, List<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetRecentNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<List<NotificationDto>> Handle(GetRecentNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetRecentAsync(request.Count, cancellationToken);
        return notifications.Select(NotificationMapper.Map).ToList();
    }
}
