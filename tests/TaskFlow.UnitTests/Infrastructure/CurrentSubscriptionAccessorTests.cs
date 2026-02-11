using Microsoft.Extensions.Configuration;
using TaskFlow.Infrastructure;

namespace TaskFlow.UnitTests.Infrastructure;

public class CurrentSubscriptionAccessorTests
{
    [Fact]
    public void Constructor_UsesDefaultsWhenConfigurationMissing()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();

        var accessor = new CurrentSubscriptionAccessor(config);
        var subscription = accessor.GetCurrentSubscription();

        Assert.Equal(Guid.Parse("00000000-0000-0000-0000-000000000001"), subscription.Id);
        Assert.Equal("Default", subscription.Name);
        Assert.NotEmpty(subscription.Schedules);
    }

    [Fact]
    public void Constructor_UsesConfiguredValuesIncludingWindow()
    {
        var dict = new Dictionary<string, string>
        {
            ["Subscription:CurrentSubscriptionId"] = Guid.NewGuid().ToString(),
            ["Subscription:CurrentSubscriptionName"] = "Workspace",
            ["Subscription:CurrentSubscriptionTier"] = "Pro",
            ["Subscription:IsEnabled"] = "false",
            ["Subscription:TimeZoneId"] = "Europe/Berlin",
            ["Subscription:Schedule:StartsOn"] = "2026-01-01",
            ["Subscription:Schedule:EndsOn"] = "2026-12-31",
        };

        var config = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();

        var accessor = new CurrentSubscriptionAccessor(config);
        var subscription = accessor.GetCurrentSubscription();

        Assert.Equal("Workspace", subscription.Name);
        Assert.Equal(TaskFlow.Domain.SubscriptionTier.Pro, subscription.Tier);
        Assert.False(subscription.IsEnabled);
        Assert.Single(subscription.Schedules);
        Assert.False(subscription.Schedules.Single().IsOpenEnded);
    }

    [Fact]
    public void Constructor_InvalidScheduleEnd_FallsBackToOpenEnded()
    {
        var dict = new Dictionary<string, string>
        {
            ["Subscription:Schedule:StartsOn"] = "2026-01-01",
            ["Subscription:Schedule:EndsOn"] = "not-a-date",
        };

        var config = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();

        var accessor = new CurrentSubscriptionAccessor(config);

        Assert.True(accessor.GetCurrentSubscription().Schedules.Single().IsOpenEnded);
    }
}
