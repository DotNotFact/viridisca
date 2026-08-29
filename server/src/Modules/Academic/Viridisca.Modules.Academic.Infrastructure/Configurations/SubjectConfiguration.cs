using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Infrastructure.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("subjects");

        builder.HasKey(s => s.Uid);

        builder.Property(s => s.Name).HasMaxLength(256).IsRequired();

        builder.Property(s => s.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(s => s.Code).IsUnique();

        builder.Property(s => s.Description).IsRequired(false);
        builder.Property(s => s.Syllabus).IsRequired(false);

        builder.Property(s => s.Type).IsRequired();
        builder.Property(s => s.Difficulty).IsRequired();

        builder.HasIndex(s => s.DepartmentUid);

        builder.HasMany(s => s.Teachers)
            .WithOne()
            .HasForeignKey(ts => ts.SubjectUid)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(s => s.Teachers).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
