using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Curriculum.Domain.Models;

namespace Viridisca.Modules.Curriculum.Infrastructure.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("submissions");

        builder.HasKey(s => s.Uid);

        builder.Property(s => s.Content).IsRequired(false);
        builder.Property(s => s.FilePath).HasMaxLength(1024).IsRequired(false);
        builder.Property(s => s.Feedback).IsRequired(false);

        builder.Property(s => s.Score).HasColumnType("decimal(6,2)");

        builder.Property(s => s.Status).IsRequired();

        // StudentUid/GradedByUid reference the Academic module — plain columns, no FK.
        builder.HasIndex(s => s.StudentUid);
        builder.HasIndex(s => s.GradedByUid);

        builder.HasIndex(s => new { s.AssignmentUid, s.StudentUid }).IsUnique();

        builder.HasOne<Assignment>()
            .WithMany()
            .HasForeignKey(s => s.AssignmentUid)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
