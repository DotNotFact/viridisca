using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Infrastructure.Configurations;

public class StudentParentConfiguration : IEntityTypeConfiguration<StudentParent>
{
    public void Configure(EntityTypeBuilder<StudentParent> builder)
    {
        builder.ToTable("student_parents");

        builder.HasKey(sp => sp.Uid);

        builder.Property(sp => sp.ParentUserUid).IsRequired();
        builder.Property(sp => sp.Relation).IsRequired();

        builder.HasIndex(sp => new { sp.StudentUid, sp.ParentUserUid }).IsUnique();
    }
}
