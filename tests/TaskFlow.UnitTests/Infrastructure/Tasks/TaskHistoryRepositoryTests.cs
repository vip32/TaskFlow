using Microsoft.Extensions.Logging.Abstractions;
using TaskFlow.Domain;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.UnitTests.Infrastructure;

[Trait("Layer", "Infrastructure")]
[Trait("Slice", "Tasks")]
[Trait("Type", "Unit")]
public class TaskHistoryRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task RegisterUsageAsync_BlankName_DoesNothing()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));

        // Act
        var sut = new TaskHistoryRepository(NullLogger<TaskHistoryRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        await sut.RegisterUsageAsync(" ", false);

        await using var db = await factory.CreateDbContextAsync();

        // Assert
        db.TaskHistories.ShouldBeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task RegisterUsageAsync_SameNameIncrementsUsageCountCaseInsensitive()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));

        // Act
        var sut = new TaskHistoryRepository(NullLogger<TaskHistoryRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        await sut.RegisterUsageAsync("Plan", false);
        await sut.RegisterUsageAsync("plan", false);

        await using var db = await factory.CreateDbContextAsync();

        // Assert
        var entry = db.TaskHistories.ShouldHaveSingleItem();
        entry.UsageCount.ShouldBe(2);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetSuggestionsAsync_FiltersByPrefixAndOrdersByUsage()
    {
        // Arrange
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

        // Act
        var sut = new TaskHistoryRepository(NullLogger<TaskHistoryRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var suggestions = await sut.GetSuggestionsAsync("Pla", false, 10);

        // Assert
        suggestions.Count.ShouldBe(2);
        suggestions[0].ShouldBe("Plan");
        suggestions[1].ShouldBe("Plaster");
    }
}


