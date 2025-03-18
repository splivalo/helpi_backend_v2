using Hangfire;
using Hangfire.PostgreSql;
using Helpi.Infrastructure.BackgroundJobs.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class HangfireConfig
{
    public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Get connection string from configuration
        var connectionString = configuration.GetConnectionString("Default");



        // Configure Hangfire with PostgreSQL
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
            {
                options.UseNpgsqlConnection(connectionString);
            }));

        // Add Hangfire server
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 4;
            options.Queues = new[] { "default", "critical", "notifications" };
        });

        return services;
    }

    public static WebApplication UseHangfireDashboard(this WebApplication app)
    {
        // Configure the dashboard
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            // Optional: Configure authorization
            // Authorization = new[]
            // {
            //     new HangfireBasicAuthenticationFilter
            //     {
            //         User = app.Configuration["Hangfire:Dashboard:Username"] ?? "admin",
            //         Pass = app.Configuration["Hangfire:Dashboard:Password"] ?? "Password123!"
            //     }
            // },
            DashboardTitle = "Helpi Background Jobs"
        });

        return app;
    }
}