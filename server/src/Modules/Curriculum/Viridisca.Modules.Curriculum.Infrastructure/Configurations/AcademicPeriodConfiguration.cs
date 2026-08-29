using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viridisca.Modules.Curriculum.Domain.Models;

namespace Viridisca.Modules.Curriculum.Infrastructure.Configurations;

public class AcademicPeriodConfiguration : IEntityTypeConfiguration<AcademicPeriod>
{
    public void Configure(EntityTypeBuilder<AcademicPeriod> builder)
    {
        builder.ToTable("academic_periods");

        builder.HasKey(p => p.Uid);

        builder.Property(p => p.Name).HasMaxLength(256).IsRequired();

        builder.Property(p => p.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(p => p.Code).IsUnique();

        builder.Property(p => p.Description).IsRequired(false);

        builder.Property(p => p.Type).IsRequired();
        builder.Property(p => p.Status).IsRequired();
    }
}
