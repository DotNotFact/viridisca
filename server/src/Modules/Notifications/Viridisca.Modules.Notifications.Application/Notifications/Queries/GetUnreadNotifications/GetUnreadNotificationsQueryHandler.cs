using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Notifications.Application.Notifications.Queries.Dto;
using Viridisca.Modules.Notifications.Domain.Models;
using Viridisca.Modules.Notifications.Domain.Repositories;

namespace Viridisca.Modules.Notifications.Application.Notifications.Queries.GetUnreadNotifications;

internal sealed class GetUnreadNotificationsQueryHandler : IRequestHandler<GetUnreadNotificationsQuery, List<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetUnreadNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<List<NotificationDto>> Handle(GetUnreadNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetUnreadByRecipientAsync(request.RecipientUid, cancellationToken);
        return notifications.Select(NotificationMapper.Map).ToList();
    }
}

internal static class NotificationMapper
{
    public static NotificationDto Map(Notification notification) => new()
    {
        Uid = notification.Uid,
        RecipientUid = notification.RecipientUid,
        Title = notification.Title,
        Message = notification.Message,
        Type = notification.Type.ToString(),
        Priority = notification.Priority.ToString(),
        Category = notification.Category,
        ActionUrl = notification.ActionUrl,
        IsRead = notification.IsRead,
        SentAtUtc = notification.SentAtUtc,
        ReadAtUtc = notification.ReadAtUtc,
    };
}
