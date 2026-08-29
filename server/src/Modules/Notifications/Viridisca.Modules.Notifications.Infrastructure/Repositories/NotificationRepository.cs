using Microsoft.EntityFrameworkCore;
using Viridisca.Modules.Notifications.Domain.Models;
using Viridisca.Modules.Notifications.Domain.Repositories;
using Viridisca.Modules.Notifications.Infrastructure.Database;

namespace Viridisca.Modules.Notifications.Infrastructure.Repositories;

public class NotificationRepository(NotificationsDbContext dbContext) : INotificationRepository
{
    private readonly NotificationsDbContext _dbContext = dbContext;

    public async Task<Notification> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default)
        => await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Uid == uid, cancellationToken);

    public async Task<IReadOnlyList<Notification>> GetUnreadByRecipientAsync(Guid recipientUid, CancellationToken cancellationToken = default)
        => await _dbContext.Notifications
            .Where(n => n.RecipientUid == recipientUid && !n.IsRead)
            .OrderByDescending(n => n.SentAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Notification>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
        => await _dbContext.Notifications
            .OrderByDescending(n => n.SentAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);

    public void Insert(Notification notification) => _dbContext.Notifications.Add(notification);

    public void Update(Notification notification) => _dbContext.Notifications.Update(notification);

    public void Delete(Notification notification) => _dbContext.Notifications.Remove(notification);
}
