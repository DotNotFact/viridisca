using System;
using MediatR;

namespace Viridisca.Modules.Notifications.Application.Notifications.Commands.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(Guid NotificationUid) : IRequest<bool>;
