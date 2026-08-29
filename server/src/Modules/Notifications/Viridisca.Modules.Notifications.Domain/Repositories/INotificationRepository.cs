using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Viridisca.Modules.Notifications.Domain.Models;

namespace Viridisca.Modules.Notifications.Domain.Repositories;

public interface INotificationRepository
{
    Task<Notification> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetUnreadByRecipientAsync(Guid recipientUid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    void Insert(Notification notification);
    void Update(Notification notification);
    void Delete(Notification notification);
}
