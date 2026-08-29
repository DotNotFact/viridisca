using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Infrastructure.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("teachers");

        builder.HasKey(t => t.Uid);

        builder.Property(t => t.UserUid).IsRequired();
        builder.HasIndex(t => t.UserUid).IsUnique();

        builder.Property(t => t.EmployeeCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(t => t.EmployeeCode).IsUnique();

        builder.Property(t => t.Specialization).HasMaxLength(256).IsRequired(false);
        builder.Property(t => t.Qualifications).IsRequired(false);
        builder.Property(t => t.Biography).IsRequired(false);

        builder.Property(t => t.Status).IsRequired();

        builder.HasIndex(t => t.DepartmentUid);

        builder.HasMany(t => t.Subjects)
            .WithOne()
            .HasForeignKey(ts => ts.TeacherUid)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Groups)
            .WithOne()
            .HasForeignKey(tg => tg.TeacherUid)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Subjects).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(t => t.Groups).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
