using System;
using System.Collections.Generic;
using MediatR;
using Viridisca.Modules.Notifications.Application.Notifications.Queries.Dto;

namespace Viridisca.Modules.Notifications.Application.Notifications.Queries.GetUnreadNotifications;

public sealed record GetUnreadNotificationsQuery(Guid RecipientUid) : IRequest<List<NotificationDto>>;
