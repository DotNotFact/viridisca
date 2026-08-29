using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Scheduler.Domain.Models;

namespace Viridisca.Modules.Scheduler.Infrastructure.Configurations;

public class ScheduleSlotConfiguration : IEntityTypeConfiguration<ScheduleSlot>
{
    public void Configure(EntityTypeBuilder<ScheduleSlot> builder)
    {
        builder.ToTable("schedule_slots");

        builder.HasKey(s => s.Uid);

        // CourseInstanceUid references the Curriculum module (different schema, different
        // DbContext) — plain indexed column, no FK constraint, same treatment Curriculum
        // itself uses for its own cross-module references to Academic.
        builder.HasIndex(s => new { s.CourseInstanceUid, s.DayOfWeek, s.StartTime });

        builder.Property(s => s.Room).HasMaxLength(100).IsRequired(false);
        builder.Property(s => s.Notes).IsRequired(false);
        builder.Property(s => s.Type).IsRequired();
    }
}
