using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Curriculum.Domain.Models;

namespace Viridisca.Modules.Curriculum.Infrastructure.Configurations;

public class CourseInstanceConfiguration : IEntityTypeConfiguration<CourseInstance>
{
    public void Configure(EntityTypeBuilder<CourseInstance> builder)
    {
        builder.ToTable("course_instances");

        builder.HasKey(c => c.Uid);

        builder.Property(c => c.Name).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Code).HasMaxLength(20).IsRequired(false);
        builder.Property(c => c.Description).IsRequired(false);

        builder.Property(c => c.Status).IsRequired();

        // SubjectUid/GroupUid/TeacherUid reference the Academic module (different schema,
        // different DbContext) — plain columns, no FK constraint, same treatment Academic
        // itself uses for its own cross-module references (e.g. Student.UserUid -> Identity).
        builder.HasIndex(c => c.SubjectUid);
        builder.HasIndex(c => c.GroupUid);
        builder.HasIndex(c => c.TeacherUid);

        // AcademicPeriod lives in this same DbContext/schema, so this is a real FK.
        builder.HasOne<AcademicPeriod>()
            .WithMany()
            .HasForeignKey(c => c.AcademicPeriodUid)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
