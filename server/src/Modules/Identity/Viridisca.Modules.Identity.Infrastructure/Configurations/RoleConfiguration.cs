using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Identity.Domain.Models;

namespace Viridisca.Modules.Identity.Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Uid);

        builder.Property(r => r.RoleType)
            .IsRequired();

        builder.HasIndex(r => r.RoleType)
            .IsUnique();

        builder.Property(r => r.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(512)
            .IsRequired();
    }
}
