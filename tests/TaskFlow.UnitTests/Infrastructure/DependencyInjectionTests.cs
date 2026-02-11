using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Infrastructure;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.UnitTests.Infrastructure;

public class DependencyInjectionTests
{
    [Fact]
    public void AddTaskFlowInfrastructure_EmptyConnectionString_ThrowsArgumentException()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentException>(() => services.AddTaskFlowInfrastructure(" "));
    }

    [Fact]
    public void AddTaskFlowInfrastructure_RegistersDbContextFactory()
    {
        var services = new ServiceCollection();
        var dbPath = Path.Combine(Path.GetTempPath(), $"taskflow-{Guid.NewGuid():N}.db");

        services.AddTaskFlowInfrastructure($"Data Source={dbPath}");
        using var provider = services.BuildServiceProvider();

        var factory = provider.GetService<IDbContextFactory<AppDbContext>>();

        Assert.NotNull(factory);
    }

    [Fact]
    public async System.Threading.Tasks.Task InitializeTaskFlowDatabaseAsync_RunsMigrations()
    {
        var services = new ServiceCollection();
        var dbPath = Path.Combine(Path.GetTempPath(), $"taskflow-{Guid.NewGuid():N}.db");

        services.AddTaskFlowInfrastructure($"Data Source={dbPath}");
        await using var provider = services.BuildServiceProvider();

        await provider.InitializeTaskFlowDatabaseAsync();

        await using var scope = provider.CreateAsyncScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync();
        Assert.True(await db.Subscriptions.AnyAsync());
    }
}
