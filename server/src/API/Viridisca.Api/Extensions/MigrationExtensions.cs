using Microsoft.EntityFrameworkCore;
using Viridisca.Modules.Academic.Infrastructure.Database;
using Viridisca.Modules.Curriculum.Infrastructure.Database;
using Viridisca.Modules.Grading.Infrastructure.EF;
using Viridisca.Modules.Identity.Infrastructure.Database;
using Viridisca.Modules.Scheduler.Infrastructure.Database;
using Viridisca.Modules.Notifications.Infrastructure.Database;

namespace Viridisca.Api.Extensions;

internal static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        ApplyMigration<IdentityDbContext>(scope);
        ApplyMigration<AcademicDbContext>(scope);
        ApplyMigration<GradingDbContext>(scope);
        ApplyMigration<CurriculumDbContext>(scope);
        ApplyMigration<SchedulerDbContext>(scope);
        ApplyMigration<NotificationsDbContext>(scope);
    }

    private static void ApplyMigration<TDbContext>(IServiceScope scope) where TDbContext : DbContext
    {
        using TDbContext context = scope.ServiceProvider.GetRequiredService<TDbContext>();

        context.Database.Migrate();
    }
}
