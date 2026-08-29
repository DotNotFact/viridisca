using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Viridisca.Modules.Grading.Infrastructure.EF;

/// <summary>
/// Design-time factory for EF Core tooling (migrations). Bypasses full app DI so
/// unrelated modules' incomplete registrations don't block Grading migrations.
/// </summary>
public sealed class GradingDesignTimeDbContextFactory : IDesignTimeDbContextFactory<GradingDbContext>
{
    public GradingDbContext CreateDbContext(string[] args)
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

        DbContextOptionsBuilder<GradingDbContext> optionsBuilder = new();
        optionsBuilder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        return new GradingDbContext(optionsBuilder.Options);
    }
}
