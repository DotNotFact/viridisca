using System;
using MediatR;

namespace Viridisca.Modules.Notifications.Application.Notifications.Commands.DeleteNotification;

public sealed record DeleteNotificationCommand(Guid NotificationUid) : IRequest<bool>;
