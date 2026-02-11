using TaskFlow.Domain;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.UnitTests.Infrastructure;

public class TaskHistoryRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task RegisterUsageAsync_BlankName_DoesNothing()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));
        var repository = new TaskHistoryRepository(factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        await repository.RegisterUsageAsync(" ", false);

        await using var db = await factory.CreateDbContextAsync();
        Assert.Empty(db.TaskHistories);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterUsageAsync_SameNameIncrementsUsageCountCaseInsensitive()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));
        var repository = new TaskHistoryRepository(factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        await repository.RegisterUsageAsync("Plan", false);
        await repository.RegisterUsageAsync("plan", false);

        await using var db = await factory.CreateDbContextAsync();
        var entry = Assert.Single(db.TaskHistories);
        Assert.Equal(2, entry.UsageCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetSuggestionsAsync_FiltersByPrefixAndOrdersByUsage()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));

        await using (var db = await factory.CreateDbContextAsync())
        {
            var top = new TaskHistory(subscriptionId, "Plan", false);
            top.MarkUsed();

            var second = new TaskHistory(subscriptionId, "Plaster", false);

            var ignoredContext = new TaskHistory(subscriptionId, "Plan sub", true);
            var ignoredSubscription = new TaskHistory(Guid.NewGuid(), "Plan foreign", false);

            db.TaskHistories.AddRange(top, second, ignoredContext, ignoredSubscription);
            await db.SaveChangesAsync();
        }

        var repository = new TaskHistoryRepository(factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var suggestions = await repository.GetSuggestionsAsync("Pla", false, 10);

        Assert.Equal(2, suggestions.Count);
        Assert.Equal("Plan", suggestions[0]);
        Assert.Equal("Plaster", suggestions[1]);
    }
}
