using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Notifications.Application.Notifications.Commands.CreateNotification;
using Viridisca.Modules.Notifications.Application.Notifications.Commands.DeleteNotification;
using Viridisca.Modules.Notifications.Application.Notifications.Commands.MarkAllNotificationsAsRead;
using Viridisca.Modules.Notifications.Application.Notifications.Commands.MarkNotificationAsRead;
using Viridisca.Modules.Notifications.Application.Notifications.Queries.GetRecentNotifications;
using Viridisca.Modules.Notifications.Application.Notifications.Queries.GetUnreadNotifications;

namespace Viridisca.Modules.Notifications.Presentation.Endpoints.Notifications;

internal sealed class NotificationsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/notifications").WithTags("Notifications").RequireAuthorization();

        group.MapPost("/", CreateNotification);
        group.MapGet("/recent", GetRecent);
        group.MapGet("/unread/{recipientUid:guid}", GetUnread);
        group.MapPut("/{notificationUid:guid}/read", MarkAsRead);
        group.MapPut("/read-all/{recipientUid:guid}", MarkAllAsRead);
        group.MapDelete("/{notificationUid:guid}", DeleteNotification);
    }

    private static async Task<IResult> CreateNotification(CreateNotificationCommand command, ISender sender, CancellationToken cancellationToken)
    {
        var notificationUid = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/notifications/{notificationUid}", notificationUid);
    }

    private static async Task<IResult> GetRecent(ISender sender, CancellationToken cancellationToken, int count = 10)
    {
        var notifications = await sender.Send(new GetRecentNotificationsQuery(count), cancellationToken);
        return Results.Ok(notifications);
    }

    private static async Task<IResult> GetUnread(Guid recipientUid, ISender sender, CancellationToken cancellationToken)
    {
        var notifications = await sender.Send(new GetUnreadNotificationsQuery(recipientUid), cancellationToken);
        return Results.Ok(notifications);
    }

    private static async Task<IResult> MarkAsRead(Guid notificationUid, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new MarkNotificationAsReadCommand(notificationUid), cancellationToken);
        return Results.Ok();
    }

    private static async Task<IResult> MarkAllAsRead(Guid recipientUid, ISender sender, CancellationToken cancellationToken)
    {
        var count = await sender.Send(new MarkAllNotificationsAsReadCommand(recipientUid), cancellationToken);
        return Results.Ok(count);
    }

    private static async Task<IResult> DeleteNotification(Guid notificationUid, ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteNotificationCommand(notificationUid), cancellationToken);
        return Results.Ok();
    }
}
