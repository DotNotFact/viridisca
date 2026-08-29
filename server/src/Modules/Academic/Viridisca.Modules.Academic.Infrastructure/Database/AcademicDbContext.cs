using Microsoft.EntityFrameworkCore;
using Viridisca.Common.Application.Data;
using Viridisca.Common.Infrastructure.EF;
using Viridisca.Modules.Academic.Domain.Groups;
using Viridisca.Modules.Academic.Domain.Models;

namespace Viridisca.Modules.Academic.Infrastructure.Database;

public sealed class AcademicDbContext(DbContextOptions<AcademicDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Student> Students { get; set; }

    public DbSet<Teacher> Teachers { get; set; }

    public DbSet<Subject> Subjects { get; set; }

    public DbSet<Group> Groups { get; set; }

    public DbSet<StudentParent> StudentParents { get; set; }

    public DbSet<TeacherGroup> TeacherGroups { get; set; }

    public DbSet<TeacherSubject> TeacherSubjects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Academic);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AcademicDbContext).Assembly);
    }
}
