using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Viridisca.Common.Infrastructure;
using Viridisca.Common.Infrastructure.EF;
using Viridisca.Common.Infrastructure.Outbox;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Notifications.Domain.Repositories;
using Viridisca.Modules.Notifications.Infrastructure.Database;
using Viridisca.Modules.Notifications.Infrastructure.Repositories;
using Viridisca.Modules.Notifications.Presentation;

namespace Viridisca.Modules.Notifications.Infrastructure;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(
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

        services.AddDbContext<NotificationsDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    databaseConnectionString,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Notifications))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>()));

        services.AddDbContextToUnitOfWork<NotificationsDbContext>();

        services.AddScoped<INotificationRepository, NotificationRepository>();
    }
}
