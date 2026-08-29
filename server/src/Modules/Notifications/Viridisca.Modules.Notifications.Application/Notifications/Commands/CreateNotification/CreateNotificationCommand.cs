using System;
using MediatR;
using Viridisca.Modules.Notifications.Domain.Models;

namespace Viridisca.Modules.Notifications.Application.Notifications.Commands.CreateNotification;

public sealed record CreateNotificationCommand(
    Guid RecipientUid,
    string Title,
    string Message,
    NotificationType Type = NotificationType.Info,
    NotificationPriority Priority = NotificationPriority.Normal,
    string? Category = null,
    string? ActionUrl = null) : IRequest<Guid>;
