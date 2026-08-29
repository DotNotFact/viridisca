using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Academic.Domain.Groups;

namespace Viridisca.Modules.Academic.Infrastructure.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups");

        builder.HasKey(g => g.Uid);

        builder.Property(g => g.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(g => g.Code).IsUnique();

        builder.Property(g => g.Name).HasMaxLength(256).IsRequired();
        builder.Property(g => g.Description).IsRequired(false);

        builder.Property(g => g.Status).IsRequired();

        builder.HasIndex(g => g.DepartmentUid);
        builder.HasIndex(g => g.CuratorUid);
    }
}
