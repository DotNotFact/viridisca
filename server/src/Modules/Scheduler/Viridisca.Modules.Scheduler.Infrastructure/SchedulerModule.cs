using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Viridisca.Common.Infrastructure;
using Viridisca.Common.Infrastructure.EF;
using Viridisca.Common.Infrastructure.Outbox;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Scheduler.Domain.Repositories;
using Viridisca.Modules.Scheduler.Infrastructure.Database;
using Viridisca.Modules.Scheduler.Infrastructure.Repositories;
using Viridisca.Modules.Scheduler.Presentation;

namespace Viridisca.Modules.Scheduler.Infrastructure;

public static class SchedulerModule
{
    public static IServiceCollection AddSchedulerModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEndpoints(AssemblyReference.Assembly);

        services.AddInfrastructure(configuration);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string databaseConnectionString = configuration.GetConnectionString("Database")!;

        services.AddDbContext<SchedulerDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    databaseConnectionString,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Scheduler))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>()));

        services.AddDbContextToUnitOfWork<SchedulerDbContext>();

        services.AddScoped<IScheduleSlotRepository, ScheduleSlotRepository>();
    }
}
