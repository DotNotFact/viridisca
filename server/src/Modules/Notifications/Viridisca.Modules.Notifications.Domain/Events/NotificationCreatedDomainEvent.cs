using System;
using Viridisca.Common.Domain;

namespace Viridisca.Modules.Notifications.Domain.Models;

public sealed class NotificationCreatedDomainEvent : DomainEvent
{
    public Guid NotificationUid { get; }
    public Guid RecipientUid { get; }

    public NotificationCreatedDomainEvent(Guid notificationUid, Guid recipientUid)
    {
        NotificationUid = notificationUid;
        RecipientUid = recipientUid;
    }
}
