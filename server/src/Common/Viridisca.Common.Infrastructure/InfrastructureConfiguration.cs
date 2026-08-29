using Viridisca.Common.Application.Clock;
using Viridisca.Common.Application.Data;
using Viridisca.Common.Infrastructure.Clock;
using Viridisca.Common.Infrastructure.Data;
using Viridisca.Common.Infrastructure.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Dapper;

namespace Viridisca.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string databaseConnectionString)
    {
        // services.AddAuthenticationInternal();
        // services.AddAuthorizationInternal();

        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        // services.TryAddSingleton<IEventBus, EventBus.EventBus>(); 
        // services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

        NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString).Build();
        services.TryAddSingleton(npgsqlDataSource);

        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

        // Registered once, centrally: each module adds its own DbContext to the pool
        // (see AddDbContextToUnitOfWork below) instead of registering IUnitOfWork itself,
        // so multiple modules' DbContexts don't collide on the same DI slot.
        services.AddScoped<IUnitOfWork, CompositeUnitOfWork>();
        // SqlMapper.AddTypeHandler(new GenericArrayHandler<string>());

        // services.AddQuartz(configurator =>
        // {
        //     var scheduler = Guid.NewGuid();
        //     configurator.SchedulerId = $"default-id-{scheduler}";
        //     configurator.SchedulerName = $"default-name-{scheduler}";
        // });
        //  services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
           
        // try
        // {
        //     IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
        //     services.AddSingleton(connectionMultiplexer);
        //     services.AddStackExchangeRedisCache(options =>
        //         options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer));
        // }
        // catch
        // {
        //     services.AddDistributedMemoryCache();
        // }
         
        // services.TryAddSingleton<ICacheService, CacheService>();
           
        // services.AddMassTransit(configure =>
        // {
        //     foreach (Action<IRegistrationConfigurator> configureConsumers in moduleConfigureConsumers)
        //     {
        //         configureConsumers(configure);
        //     }
           
        //     configure.SetKebabCaseEndpointNameFormatter();
           
        //     configure.UsingInMemory((context, cfg) =>
        //     {
        //         cfg.ConfigureEndpoints(context);
        //     });
        // });
           
        // services
        //     .AddOpenTelemetry()
        //     .ConfigureResource(resource => resource.AddService(serviceName))
        //     .WithTracing(tracing =>
        //     {
        //         tracing
        //             .AddAspNetCoreInstrumentation()
        //             .AddHttpClientInstrumentation()
        //             .AddEntityFrameworkCoreInstrumentation()
        //             .AddRedisInstrumentation()
        //             .AddNpgsql()
        //             .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName);
           
        //         tracing.AddOtlpExporter();
        //     });

        services.TryAddSingleton<PublishDomainEventsInterceptor>();

        return services;
    }

    /// <summary>
    /// Registers a module's DbContext with <see cref="CompositeUnitOfWork"/> so
    /// SaveChangesAsync via the shared IUnitOfWork actually reaches it. Call this from each
    /// module's AddInfrastructure instead of registering IUnitOfWork directly.
    /// </summary>
    public static IServiceCollection AddDbContextToUnitOfWork<TDbContext>(this IServiceCollection services)
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        services.AddScoped<Microsoft.EntityFrameworkCore.DbContext>(sp => sp.GetRequiredService<TDbContext>());

        return services;
    }
}


//internal static class AuthenticationExtensions
//{
//    internal static IServiceCollection AddAuthenticationInternal(this IServiceCollection services)
//    {
//        services.AddAuthorization();

//        services.AddAuthentication().AddJwtBearer();

//        services.AddHttpContextAccessor();

//        services.ConfigureOptions<JwtBearerConfigureOptions>();

//        return services;
//    }
//}

//internal static class AuthorizationExtensions
//{
//    internal static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
//    {
//        services.AddTransient<IClaimsTransformation, CustomClaimsTransformation>();

//        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

//        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

//        return services;
//    }
//}
