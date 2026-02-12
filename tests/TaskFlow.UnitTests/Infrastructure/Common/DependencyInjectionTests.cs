using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Infrastructure;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.UnitTests.Infrastructure;

[Trait("Layer", "Infrastructure")]
[Trait("Slice", "Common")]
[Trait("Type", "Unit")]
public class DependencyInjectionTests
{
    [Fact]
    public void AddTaskFlowInfrastructure_EmptyConnectionString_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var services = new ServiceCollection();

        // Assert
        Should.Throw<ArgumentException>(() => services.AddTaskFlowInfrastructure(" "));
    }

    [Fact]
    public void AddTaskFlowInfrastructure_RegistersDbContextFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var dbPath = Path.Combine(Path.GetTempPath(), $"taskflow-{Guid.NewGuid():N}.db");

        services.AddTaskFlowInfrastructure($"Data Source={dbPath}");

        // Act
        using var provider = services.BuildServiceProvider();

        var factory = provider.GetService<IDbContextFactory<AppDbContext>>();

        // Assert
        factory.ShouldNotBeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task InitializeTaskFlowDatabaseAsync_RunsMigrations()
    {
        // Arrange
        var services = new ServiceCollection();
        var dbPath = Path.Combine(Path.GetTempPath(), $"taskflow-{Guid.NewGuid():N}.db");

        services.AddTaskFlowInfrastructure($"Data Source={dbPath}");
        await using var provider = services.BuildServiceProvider();

        await provider.InitializeTaskFlowDatabaseAsync();

        await using var scope = provider.CreateAsyncScope();

        // Act
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync();

        // Assert
        (await db.Subscriptions.AnyAsync()).ShouldBeTrue();
    }
}


