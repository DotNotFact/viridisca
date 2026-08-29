using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Identity.Domain.Models;

namespace Viridisca.Modules.Identity.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Uid);

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Username)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.Property(u => u.FirstName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.MiddleName)
            .HasMaxLength(128)
            .IsRequired(false);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(32)
            .IsRequired(false);

        builder.Property(u => u.ProfileImageUrl)
            .IsRequired(false);

        builder.Navigation(u => u.UserRoles).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(u => u.RefreshTokens).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
