using Microsoft.EntityFrameworkCore;
using Viridisca.Common.Application.Data;
using Viridisca.Common.Infrastructure.EF;
using Viridisca.Modules.Notifications.Domain.Models;

namespace Viridisca.Modules.Notifications.Infrastructure.Database;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Notifications);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);
    }
}
