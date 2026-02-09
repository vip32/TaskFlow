using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure;

/// <summary>
/// Registers infrastructure services for TaskFlow.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds EF Core and repository implementations to the service collection.
    /// </summary>
    /// <param name="services">Service collection instance.</param>
    /// <param name="connectionString">SQLite connection string.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddTaskFlowInfrastructure(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));
        }

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite(connectionString)
                .UseSeeding((dbContext, _) =>
                {
                    TaskFlowDataSeeder.Seed((AppDbContext)dbContext);
                })
                .UseAsyncSeeding(async (dbContext, _, cancellationToken) =>
                {
                    await TaskFlowDataSeeder.SeedAsync((AppDbContext)dbContext, cancellationToken);
                }));

        services.AddTaskFlowInfrastructureServices();

        return services;
    }

    /// <summary>
    /// Applies pending database migrations for TaskFlow.
    /// </summary>
    /// <param name="serviceProvider">Application service provider.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A task that completes when migrations are applied.</returns>
    public static async Task InitializeTaskFlowDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();

        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        await db.Database.MigrateAsync(cancellationToken);
    }
}
