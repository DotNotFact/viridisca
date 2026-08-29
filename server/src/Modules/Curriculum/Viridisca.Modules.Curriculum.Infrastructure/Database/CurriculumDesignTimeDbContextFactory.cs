using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Viridisca.Modules.Curriculum.Infrastructure.Database;

/// <summary>
/// Design-time factory for EF Core tooling (migrations). Bypasses full app DI so
/// unrelated modules' incomplete registrations don't block Curriculum migrations.
/// </summary>
public sealed class CurriculumDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CurriculumDbContext>
{
    public CurriculumDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../../API/Viridisca.Api"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "No database connection string configured. Set ConnectionStrings:Database in appsettings.Development.json.");

        DbContextOptionsBuilder<CurriculumDbContext> optionsBuilder = new();
        optionsBuilder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        return new CurriculumDbContext(optionsBuilder.Options);
    }
}
