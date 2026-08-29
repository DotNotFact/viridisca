using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Academic.Domain.Groups;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Infrastructure.Configurations;

public class TeacherGroupConfiguration : IEntityTypeConfiguration<TeacherGroup>
{
    public void Configure(EntityTypeBuilder<TeacherGroup> builder)
    {
        builder.ToTable("teacher_groups");

        builder.HasKey(tg => tg.Uid);

        builder.HasIndex(tg => new { tg.TeacherUid, tg.GroupUid, tg.SubjectUid }).IsUnique();

        // Teacher <-> TeacherGroup is configured in TeacherConfiguration. Group has no
        // navigation to TeacherGroup, so that FK is configured here (safe: no dangling
        // navigation on either side for EF to independently rediscover).
        builder.HasOne<Group>()
            .WithMany()
            .HasForeignKey(tg => tg.GroupUid)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Subject>()
            .WithMany()
            .HasForeignKey(tg => tg.SubjectUid)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(tg => tg.IsCurator).IsRequired();
        builder.Property(tg => tg.IsActive).IsRequired();
    }
}
