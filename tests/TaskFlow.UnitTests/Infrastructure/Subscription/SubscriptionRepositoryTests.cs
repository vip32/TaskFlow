using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.UnitTests.Infrastructure;

[Trait("Layer", "Infrastructure")]
[Trait("Slice", "Subscription")]
[Trait("Type", "Unit")]
public class SubscriptionRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetCurrentAsync_ExistingSubscription_LoadsSettingsThroughAggregate()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(GetCurrentAsync_ExistingSubscription_LoadsSettingsThroughAggregate));
        await using (var db = await factory.CreateDbContextAsync())
        {
            var subscription = new Subscription(subscriptionId, "Test", SubscriptionTier.Free, true, "Europe/Berlin");
            subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
            subscription.SetAlwaysShowCompletedTasks(true);
            db.Subscriptions.Add(subscription);
            await db.SaveChangesAsync();
        }

        var sut = new SubscriptionRepository(
            NullLogger<SubscriptionRepository>.Instance,
            factory,
            new TestCurrentSubscriptionAccessor(subscriptionId));

        // Act
        var current = await sut.GetCurrentAsync();

        // Assert
        current.Id.ShouldBe(subscriptionId);
        current.Settings.ShouldNotBeNull();
        current.Settings.AlwaysShowCompletedTasks.ShouldBeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateAsync_SettingsChanged_PersistsAggregateUpdate()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(UpdateAsync_SettingsChanged_PersistsAggregateUpdate));
        await using (var db = await factory.CreateDbContextAsync())
        {
            var subscription = new Subscription(subscriptionId, "Test", SubscriptionTier.Free, true, "Europe/Berlin");
            subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
            db.Subscriptions.Add(subscription);
            await db.SaveChangesAsync();
        }

        var sut = new SubscriptionRepository(
            NullLogger<SubscriptionRepository>.Instance,
            factory,
            new TestCurrentSubscriptionAccessor(subscriptionId));

        var current = await sut.GetCurrentAsync();
        current.SetAlwaysShowCompletedTasks(true);

        // Act
        var updated = await sut.UpdateAsync(current);

        // Assert
        updated.Settings.AlwaysShowCompletedTasks.ShouldBeTrue();

        await using var assertDb = await factory.CreateDbContextAsync();
        var persisted = await assertDb.Subscriptions.Include(x => x.Settings).FirstAsync(x => x.Id == subscriptionId);
        persisted.Settings.ShouldNotBeNull();
        persisted.Settings.AlwaysShowCompletedTasks.ShouldBeTrue();
    }
}
