using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Notifications.Domain.Models;

namespace Viridisca.Modules.Notifications.Infrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Uid);

        // RecipientUid references Identity's User (different schema, different DbContext) —
        // plain indexed column, no FK constraint, same treatment as other cross-module refs.
        builder.HasIndex(n => new { n.RecipientUid, n.IsRead });

        builder.Property(n => n.Title).HasMaxLength(256).IsRequired();
        builder.Property(n => n.Message).IsRequired(false);
        builder.Property(n => n.Category).HasMaxLength(100).IsRequired(false);
        builder.Property(n => n.ActionUrl).IsRequired(false);
        builder.Property(n => n.Type).IsRequired();
        builder.Property(n => n.Priority).IsRequired();
    }
}
