using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Auth;
using Infrastructure.BackgroundJobs;
using Infrastructure.Common;
using Infrastructure.Cors;
using Infrastructure.FileStorage;
using Infrastructure.Localization;
using Infrastructure.Mailing;
using Infrastructure.Mapping;
using Infrastructure.Middleware;

using Infrastructure.Notifications;
using Infrastructure.OpenApi;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Initialization;
using Infrastructure.SecurityHeaders;
using System.Reflection;

namespace Infrastructure;
public static class Startup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        MapsterSettings.Configure();
        return services
            .AddApiVersioning()
            .AddAuth(config)
            .AddBackgroundJobs(config)
            .AddCorsPolicy(config)
            .AddExceptionMiddleware()
            .AddHealthCheck()
            .AddLocalization(config)
            .AddMailing(config)
            .AddMediatR(Assembly.GetExecutingAssembly())
            .AddNotifications(config)
            .AddOpenApiDocumentation(config)
            .AddPersistence(config)
            .AddRequestLogging(config)
            .AddRouting(options => options.LowercaseUrls = true)
            .AddServices();
    }

    private static IServiceCollection AddApiVersioning(this IServiceCollection services) =>
        services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
        });

    private static IServiceCollection AddHealthCheck(this IServiceCollection services) =>
        services.AddHealthChecks().Services;

    public static async Task InitializeDatabasesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        // Create a new scope to retrieve scoped services
        using var scope = services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>()
            .InitializeDatabasesAsync(cancellationToken);
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder, IConfiguration config) =>
        builder
            .UseLocalization(config)
            .UseStaticFiles()
            .UseSecurityHeaders(config)
            .UseFileStorage()
            .UseExceptionMiddleware()
            .UseRouting()
            .UseCorsPolicy()
            .UseAuthentication()
            .UseCurrentUser()
            .UseAuthorization()
            .UseRequestLogging(config)
            .UseHangfireDashboard(config)
            .UseOpenApiDocumentation(config);

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapControllers().RequireAuthorization();
        builder.MapHealthCheck();
        builder.MapNotifications();
        return builder;
    }

    private static IEndpointConventionBuilder MapHealthCheck(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapHealthChecks("/api/health").RequireAuthorization();
}