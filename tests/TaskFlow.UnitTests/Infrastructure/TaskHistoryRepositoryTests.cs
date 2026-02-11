using TaskFlow.Domain;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.UnitTests.Infrastructure;

public class TaskHistoryRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task RegisterUsageAsync_WhitespaceName_DoesNothing()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(RegisterUsageAsync_WhitespaceName_DoesNothing));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new TaskHistoryRepository(factory, accessor);

        await repository.RegisterUsageAsync("   ", false);

        await using var db = factory.CreateDbContext();
        Assert.Empty(db.TaskHistories);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterUsageAsync_NewName_AddsEntry()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(RegisterUsageAsync_NewName_AddsEntry));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new TaskHistoryRepository(factory, accessor);

        await repository.RegisterUsageAsync("Draft", false);

        await using var db = factory.CreateDbContext();
        var entry = Assert.Single(db.TaskHistories);
        Assert.Equal("Draft", entry.Name);
        Assert.Equal(1, entry.UsageCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterUsageAsync_ExistingNameCaseInsensitive_IncrementsUsage()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(RegisterUsageAsync_ExistingNameCaseInsensitive_IncrementsUsage));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new TaskHistoryRepository(factory, accessor);

        await repository.RegisterUsageAsync("Draft", false);
        await repository.RegisterUsageAsync(" draft ", false);

        await using var db = factory.CreateDbContext();
        var entry = Assert.Single(db.TaskHistories);
        Assert.Equal(2, entry.UsageCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetSuggestionsAsync_FiltersAndOrdersByUsage()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(GetSuggestionsAsync_FiltersAndOrdersByUsage));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new TaskHistoryRepository(factory, accessor);

        await repository.RegisterUsageAsync("Deploy", false);
        await repository.RegisterUsageAsync("Deploy", false);
        await repository.RegisterUsageAsync("Draft", false);
        await repository.RegisterUsageAsync("Design", true);

        var suggestions = await repository.GetSuggestionsAsync("D", false, take: 10);

        Assert.Equal(2, suggestions.Count);
        Assert.Equal("Deploy", suggestions[0]);
        Assert.Equal("Draft", suggestions[1]);
    }
}

