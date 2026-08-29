using Microsoft.EntityFrameworkCore;
using Viridisca.Common.Application.Data;
using Viridisca.Common.Infrastructure.EF;
using Viridisca.Modules.Scheduler.Domain.Models;

namespace Viridisca.Modules.Scheduler.Infrastructure.Database;

public sealed class SchedulerDbContext(DbContextOptions<SchedulerDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<ScheduleSlot> ScheduleSlots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Scheduler);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchedulerDbContext).Assembly);
    }
}
