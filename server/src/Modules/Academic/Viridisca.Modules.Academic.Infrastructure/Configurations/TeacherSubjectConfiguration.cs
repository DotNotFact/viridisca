using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Infrastructure.Configurations;

public class TeacherSubjectConfiguration : IEntityTypeConfiguration<TeacherSubject>
{
    public void Configure(EntityTypeBuilder<TeacherSubject> builder)
    {
        builder.ToTable("teacher_subjects");

        builder.HasKey(ts => ts.Uid);

        builder.HasIndex(ts => new { ts.TeacherUid, ts.SubjectUid }).IsUnique();

        // TeacherSubject has no back-navigation to either side (only TeacherUid/SubjectUid FK
        // columns) — Teacher <-> TeacherSubject is configured in TeacherConfiguration,
        // Subject <-> TeacherSubject is configured in SubjectConfiguration. This class only
        // configures the entity's own scalar properties to avoid configuring either FK twice.
        builder.Property(ts => ts.IsMainTeacher).IsRequired();
        builder.Property(ts => ts.IsActive).IsRequired();
    }
}
