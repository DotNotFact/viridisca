using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Notifications.Application.Notifications.Queries.Dto;

namespace Viridisca.Modules.Notifications.Application.Notifications.Queries.GetRecentNotifications;

public sealed record GetRecentNotificationsQuery(int Count = 10) : IRequest<List<NotificationDto>>;
