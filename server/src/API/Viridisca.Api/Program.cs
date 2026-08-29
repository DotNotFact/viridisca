using Viridisca.Modules.Identity.Infrastructure;
using Viridisca.Modules.Academic.Infrastructure;
using Viridisca.Modules.Grading.Infrastructure;
using Viridisca.Modules.Curriculum.Infrastructure;
using Viridisca.Modules.Scheduler.Infrastructure;
using Viridisca.Modules.Notifications.Infrastructure;
using Viridisca.Common.Presentation.Endpoints;
using Viridisca.Common.Infrastructure;
// using HealthChecks.UI.Client;
using Viridisca.Common.Application;
using Viridisca.Api.Extensions;
using Viridisca.Api.Middleware;
using System.Reflection;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console());

// services.AddExceptionHandler<GlobalExceptionHandler>();
services.AddProblemDetails();

services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    options.SerializerOptions.Converters.Add(new Viridisca.Api.Serialization.UtcDateTimeConverter());
    options.SerializerOptions.Converters.Add(new Viridisca.Api.Serialization.UtcNullableDateTimeConverter());
});

services.AddEndpointsApiExplorer();
services.AddSwaggerDocumentation();

services.AddAuthentication(configuration);

services.AddAuthorizationBuilder();
services.AddAuthorizationWithPolicies();

Assembly[] moduleApplicationAssemblies = [
    Viridisca.Modules.Identity.Application.AssemblyReference.Assembly,
    Viridisca.Modules.Academic.Application.AssemblyReference.Assembly,
    Viridisca.Modules.Grading.Application.AssemblyReference.Assembly,
    Viridisca.Modules.Curriculum.Application.AssemblyReference.Assembly,
    Viridisca.Modules.Scheduler.Application.AssemblyReference.Assembly,
    Viridisca.Modules.Notifications.Application.AssemblyReference.Assembly,
];

services.AddApplication(moduleApplicationAssemblies);

string databaseConnectionString = configuration.GetConnectionStringOrThrow("Database");
// string redisConnectionString = configuration.GetConnectionStringOrThrow("Cache");

services.AddInfrastructure(
    // DiagnosticsConfig.ServiceName,
    //[
    //    LessonModele.ConfigureConsumers(redisConnectionString),
    //    // TicketingModule.ConfigureConsumers,
    //    // AttendanceModule.ConfigureConsumers
    //],
    databaseConnectionString);
// redisConnectionString);

// Uri keyCloakHealthUrl = configuration.GetKeyCloakHealthUrl();

// services
// .AddHealthChecks()
// .AddNpgSql(databaseConnectionString);
// .AddRedis(redisConnectionString)
// .AddKeyCloak(keyCloakHealthUrl);

builder.Configuration.AddModuleConfiguration(["identity",]);

services.AddIdentityModule(configuration);
services.AddAcademicModule(configuration);
services.AddGradingModule(configuration);
services.AddCurriculumModule(configuration);
services.AddSchedulerModule(configuration);
services.AddNotificationsModule(configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
}

// app.MapHealthChecks("health", new HealthCheckOptions
// {
//     ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
// });

app.UseLogContext();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

app.Run();

internal partial class Program;