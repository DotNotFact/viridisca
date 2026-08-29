using Microsoft.EntityFrameworkCore;
using Viridisca.Common.Application.Data;
using Viridisca.Common.Infrastructure.EF;
using Viridisca.Modules.Curriculum.Domain.Models;

namespace Viridisca.Modules.Curriculum.Infrastructure.Database;

public sealed class CurriculumDbContext(DbContextOptions<CurriculumDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<AcademicPeriod> AcademicPeriods { get; set; }

    public DbSet<CourseInstance> CourseInstances { get; set; }

    public DbSet<Assignment> Assignments { get; set; }

    public DbSet<Submission> Submissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Curriculum);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CurriculumDbContext).Assembly);
    }
}
