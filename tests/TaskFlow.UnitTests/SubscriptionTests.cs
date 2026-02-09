using TaskFlow.Domain;

namespace TaskFlow.UnitTests;

/// <summary>
/// Tests subscription aggregate behavior.
/// </summary>
public class SubscriptionTests
{
    /// <summary>
    /// Verifies tier transitions work as expected.
    /// </summary>
    [Fact]
    public void TierTransitions_UpgradeAndDowngrade_ChangesTier()
    {
        var subscription = new Subscription("Starter", SubscriptionTier.Free);

        subscription.UpgradeToPlus();
        Assert.Equal(SubscriptionTier.Plus, subscription.Tier);

        subscription.UpgradeToPro();
        Assert.Equal(SubscriptionTier.Pro, subscription.Tier);

        subscription.DowngradeToFree();
        Assert.Equal(SubscriptionTier.Free, subscription.Tier);
    }

    /// <summary>
    /// Verifies disabled subscriptions are never active.
    /// </summary>
    [Fact]
    public void IsActiveAt_DisabledSubscription_ReturnsFalse()
    {
        var subscription = new Subscription("Starter", SubscriptionTier.Free);
        subscription.AddOpenEndedSchedule(new DateOnly(2026, 1, 1));
        subscription.Disable();

        var isActive = subscription.IsActiveAt(new DateOnly(2026, 1, 10));

        Assert.False(isActive);
    }

    /// <summary>
    /// Verifies subscriptions are active when enabled and a schedule matches.
    /// </summary>
    [Fact]
    public void IsActiveAt_EnabledWithMatchingSchedule_ReturnsTrue()
    {
        var subscription = new Subscription("Starter", SubscriptionTier.Free);
        subscription.AddScheduledWindow(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

        var isActive = subscription.IsActiveAt(new DateOnly(2026, 1, 15));

        Assert.True(isActive);
    }
}
