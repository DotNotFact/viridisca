using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Viridisca.Common.Application.Data;
using Viridisca.Common.Application.Identity;
using Viridisca.Common.Infrastructure;
using Viridisca.Common.Infrastructure.EF;
using Viridisca.Common.Infrastructure.Identity;
using Viridisca.Common.Infrastructure.Outbox;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Identity.Domain.Repositories;
using Viridisca.Modules.Identity.Domain.Services;
using Viridisca.Modules.Identity.Infrastructure.Authentication;
using Viridisca.Modules.Identity.Infrastructure.Database;
using Viridisca.Modules.Identity.Infrastructure.Roles;
using Viridisca.Modules.Identity.Infrastructure.Users;
using Viridisca.Modules.Identity.Presentation;

namespace Viridisca.Modules.Identity.Infrastructure;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEndpoints(AssemblyReference.Assembly);

        services.AddInfrastructure(configuration);

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        string databaseConnectionString = configuration.GetConnectionString("Database")!;

        services.AddDbContext<IdentityDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    databaseConnectionString,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>()));

        services.AddDbContextToUnitOfWork<IdentityDbContext>();

        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IRoleRepository, RoleRepository>();

        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddScoped<IJwtProvider, JwtProvider>();
    }
}
