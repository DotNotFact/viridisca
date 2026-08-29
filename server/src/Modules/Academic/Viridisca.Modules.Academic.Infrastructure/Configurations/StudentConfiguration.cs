using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Academic.Domain.Groups;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Infrastructure.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");

        builder.HasKey(s => s.Uid);

        builder.Property(s => s.UserUid).IsRequired();
        builder.HasIndex(s => s.UserUid).IsUnique();

        builder.Property(s => s.FirstName).HasMaxLength(128).IsRequired();
        builder.Property(s => s.LastName).HasMaxLength(128).IsRequired();
        builder.Property(s => s.MiddleName).HasMaxLength(128).IsRequired(false);
        builder.Property(s => s.Email).HasMaxLength(256).IsRequired();
        builder.Property(s => s.PhoneNumber).HasMaxLength(32).IsRequired(false);

        builder.Property(s => s.StudentCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(s => s.StudentCode).IsUnique();

        builder.Property(s => s.EmergencyContactName).HasMaxLength(256).IsRequired(false);
        builder.Property(s => s.EmergencyContactPhone).HasMaxLength(32).IsRequired(false);
        builder.Property(s => s.MedicalInformation).IsRequired(false);

        builder.Property(s => s.Status).IsRequired();

        builder.HasIndex(s => s.GroupUid);

        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(s => s.GroupUid)
            .OnDelete(DeleteBehavior.SetNull);

        // StudentParent has no back-navigation to Student (only a StudentUid FK column),
        // so this relationship is configured from the Student side only.
        builder.HasMany(s => s.Parents)
            .WithOne()
            .HasForeignKey(sp => sp.StudentUid)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Parents).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
