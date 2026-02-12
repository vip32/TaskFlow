using Microsoft.Extensions.Configuration;
using TaskFlow.Infrastructure;

namespace TaskFlow.UnitTests.Infrastructure;

[Trait("Layer", "Infrastructure")]
[Trait("Slice", "Subscription")]
[Trait("Type", "Unit")]
public class CurrentSubscriptionAccessorTests
{
    [Fact]
    public void Constructor_UsesDefaultsWhenConfigurationMissing()
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();

        // Act
        var sut = new CurrentSubscriptionAccessor(config);
        var subscription = sut.GetCurrentSubscription();

        // Assert
        subscription.Id.ShouldBe(Guid.Parse("00000000-0000-0000-0000-000000000001"));
        subscription.Name.ShouldBe("Default");
        subscription.Schedules.ShouldNotBeEmpty();
    }

    [Fact]
    public void Constructor_UsesConfiguredValuesIncludingWindow()
    {
        // Arrange
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

        // Act
        var sut = new CurrentSubscriptionAccessor(config);
        var subscription = sut.GetCurrentSubscription();

        // Assert
        subscription.Name.ShouldBe("Workspace");
        subscription.Tier.ShouldBe(TaskFlow.Domain.SubscriptionTier.Pro);
        subscription.IsEnabled.ShouldBeFalse();
        subscription.Schedules.ShouldHaveSingleItem();
        subscription.Schedules.Single().IsOpenEnded.ShouldBeFalse();
    }

    [Fact]
    public void Constructor_InvalidScheduleEnd_FallsBackToOpenEnded()
    {
        // Arrange
        var dict = new Dictionary<string, string>
        {
            ["Subscription:Schedule:StartsOn"] = "2026-01-01",
            ["Subscription:Schedule:EndsOn"] = "not-a-date",
        };

        var config = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();

        // Act
        var sut = new CurrentSubscriptionAccessor(config);

        // Assert
        sut.GetCurrentSubscription().Schedules.Single().IsOpenEnded.ShouldBeTrue();
    }
}


