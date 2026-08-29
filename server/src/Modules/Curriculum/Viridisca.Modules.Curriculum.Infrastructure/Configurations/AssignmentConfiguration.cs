using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Curriculum.Domain.Models;

namespace Viridisca.Modules.Curriculum.Infrastructure.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("assignments");

        builder.HasKey(a => a.Uid);

        builder.Property(a => a.Title).HasMaxLength(256).IsRequired();
        builder.Property(a => a.Description).IsRequired(false);
        builder.Property(a => a.Instructions).IsRequired(false);

        builder.Property(a => a.MaxScore).HasColumnType("decimal(6,2)").IsRequired();

        builder.Property(a => a.Type).IsRequired();
        builder.Property(a => a.Difficulty).IsRequired();
        builder.Property(a => a.Status).IsRequired();

        builder.HasOne<CourseInstance>()
            .WithMany()
            .HasForeignKey(a => a.CourseInstanceUid)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
