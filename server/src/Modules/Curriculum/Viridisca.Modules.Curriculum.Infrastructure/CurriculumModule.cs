using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Viridisca.Common.Infrastructure;
using Viridisca.Common.Infrastructure.EF;
using Viridisca.Common.Infrastructure.Outbox;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Modules.Curriculum.Domain.Repositories;
using Viridisca.Modules.Curriculum.Infrastructure.Database;
using Viridisca.Modules.Curriculum.Infrastructure.Repositories;
using Viridisca.Modules.Curriculum.Presentation;

namespace Viridisca.Modules.Curriculum.Infrastructure;

public static class CurriculumModule
{
    public static IServiceCollection AddCurriculumModule(
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

        services.AddDbContext<CurriculumDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    databaseConnectionString,
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Curriculum))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>()));

        services.AddDbContextToUnitOfWork<CurriculumDbContext>();

        services.AddScoped<IAcademicPeriodRepository, AcademicPeriodRepository>();
        services.AddScoped<ICourseInstanceRepository, CourseInstanceRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
    }
}
