using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.UnitTests.Infrastructure;

public class TaskFlowDataSeederTests
{
    [Fact]
    public void Seed_WhenEmpty_AddsBaselineData()
    {
        using var db = CreateDb();

        TaskFlowDataSeeder.Seed(db);

        Assert.NotEmpty(db.Subscriptions);
        Assert.Equal(3, db.Projects.Count());
        Assert.Equal(5, db.Tasks.Count());
        Assert.Equal(5, db.MyTaskFlowSections.Count());
    }

    [Fact]
    public void Seed_WhenDataExists_DoesNothing()
    {
        using var db = CreateDb();
        TaskFlowDataSeeder.Seed(db);

        var subscriptionsBefore = db.Subscriptions.Count();
        var projectsBefore = db.Projects.Count();

        TaskFlowDataSeeder.Seed(db);

        Assert.Equal(subscriptionsBefore, db.Subscriptions.Count());
        Assert.Equal(projectsBefore, db.Projects.Count());
    }

    [Fact]
    public async System.Threading.Tasks.Task SeedAsync_WhenEmpty_AddsBaselineData()
    {
        await using var db = CreateDb();

        await TaskFlowDataSeeder.SeedAsync(db, CancellationToken.None);

        Assert.NotEmpty(db.Subscriptions);
        Assert.Equal(3, await db.Projects.CountAsync());
        Assert.Equal(5, await db.Tasks.CountAsync());
        Assert.Equal(5, await db.MyTaskFlowSections.CountAsync());
    }

    [Fact]
    public async System.Threading.Tasks.Task SeedAsync_WhenDataExists_DoesNothing()
    {
        await using var db = CreateDb();
        await TaskFlowDataSeeder.SeedAsync(db, CancellationToken.None);

        var subscriptionsBefore = await db.Subscriptions.CountAsync();

        await TaskFlowDataSeeder.SeedAsync(db, CancellationToken.None);

        Assert.Equal(subscriptionsBefore, await db.Subscriptions.CountAsync());
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new AppDbContext(options);
    }
}
