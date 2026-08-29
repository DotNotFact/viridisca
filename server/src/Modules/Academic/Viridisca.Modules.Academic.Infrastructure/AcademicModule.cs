using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Viridisca.Common.Application.Data;
using Viridisca.Common.Infrastructure;
using Viridisca.Common.Infrastructure.EF;
using Viridisca.Common.Infrastructure.Outbox;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Academic.Application.Common.Interfaces;
using Viridisca.Modules.Academic.Domain.Groups;
using Viridisca.Modules.Academic.Domain.Models;
using Viridisca.Modules.Academic.Infrastructure.Database;
using Viridisca.Modules.Academic.Infrastructure.Repositories;
using Viridisca.Modules.Academic.Infrastructure.Services;
using Viridisca.Modules.Academic.Presentation;

namespace Viridisca.Modules.Academic.Infrastructure;

public static class AcademicModule
{
    public static IServiceCollection AddAcademicModule(
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

        services.AddDbContext<AcademicDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    databaseConnectionString,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Academic))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>()));

        services.AddDbContextToUnitOfWork<AcademicDbContext>();

        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();

        services.AddScoped<IUserInfoService, UserInfoService>();
    }
}
