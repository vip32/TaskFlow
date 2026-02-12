using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.UnitTests.Infrastructure;

public class TaskFlowDataSeederTests
{
    [Fact]
    public void Seed_WhenEmpty_AddsBaselineData()
    {
        // Arrange

        // Act
        using var db = CreateDb();

        TaskFlowDataSeeder.Seed(db);

        // Assert
        db.Subscriptions.ShouldNotBeEmpty();
        db.Projects.Count().ShouldBe(3);
        db.Tasks.Count().ShouldBe(5);
        db.MyTaskFlowSections.Count().ShouldBe(5);
    }

    [Fact]
    public void Seed_WhenDataExists_DoesNothing()
    {
        // Arrange
        using var db = CreateDb();
        TaskFlowDataSeeder.Seed(db);

        var subscriptionsBefore = db.Subscriptions.Count();

        // Act
        var projectsBefore = db.Projects.Count();

        TaskFlowDataSeeder.Seed(db);

        // Assert
        db.Subscriptions.Count().ShouldBe(subscriptionsBefore);
        db.Projects.Count().ShouldBe(projectsBefore);
    }

    [Fact]
    public async System.Threading.Tasks.Task SeedAsync_WhenEmpty_AddsBaselineData()
    {
        // Arrange

        // Act
        await using var db = CreateDb();

        await TaskFlowDataSeeder.SeedAsync(db, CancellationToken.None);

        // Assert
        db.Subscriptions.ShouldNotBeEmpty();
        (await db.Projects.CountAsync()).ShouldBe(3);
        (await db.Tasks.CountAsync()).ShouldBe(5);
        (await db.MyTaskFlowSections.CountAsync()).ShouldBe(5);
    }

    [Fact]
    public async System.Threading.Tasks.Task SeedAsync_WhenDataExists_DoesNothing()
    {
        // Arrange
        await using var db = CreateDb();
        await TaskFlowDataSeeder.SeedAsync(db, CancellationToken.None);

        // Act
        var subscriptionsBefore = await db.Subscriptions.CountAsync();

        await TaskFlowDataSeeder.SeedAsync(db, CancellationToken.None);

        // Assert
        (await db.Subscriptions.CountAsync()).ShouldBe(subscriptionsBefore);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AppDbContext(options);
    }
}
