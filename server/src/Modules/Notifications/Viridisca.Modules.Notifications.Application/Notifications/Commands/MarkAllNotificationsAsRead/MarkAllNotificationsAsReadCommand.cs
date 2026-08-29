using System;
using MediatR;

namespace Viridisca.Modules.Notifications.Application.Notifications.Commands.MarkAllNotificationsAsRead;

public sealed record MarkAllNotificationsAsReadCommand(Guid RecipientUid) : IRequest<int>;
