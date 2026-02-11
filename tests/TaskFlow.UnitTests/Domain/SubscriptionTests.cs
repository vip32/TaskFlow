using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

/// <summary>
/// Tests subscription aggregate behavior.
/// </summary>
public class SubscriptionTests
{
    [Fact]
    public void Constructor_InvalidArguments_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Subscription(Guid.Empty, "Name", SubscriptionTier.Free, true));
        Assert.Throws<ArgumentException>(() => new Subscription(Guid.NewGuid(), " ", SubscriptionTier.Free, true));
        Assert.Throws<ArgumentException>(() => new Subscription(Guid.NewGuid(), "Name", SubscriptionTier.Free, true, " "));
    }

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

    /// <summary>
    /// Verifies default timezone is Europe/Berlin.
    /// </summary>
    [Fact]
    public void Constructor_DefaultTimezone_IsEuropeBerlin()
    {
        var subscription = new Subscription("Starter", SubscriptionTier.Free);

        Assert.Equal("Europe/Berlin", subscription.TimeZoneId);
    }

    /// <summary>
    /// Verifies timezone update rejects unknown ids.
    /// </summary>
    [Fact]
    public void SetTimeZone_UnknownTimeZone_ThrowsArgumentException()
    {
        var subscription = new Subscription("Starter", SubscriptionTier.Free);

        Assert.Throws<ArgumentException>(() => subscription.SetTimeZone("invalid/timezone"));
    }

    [Fact]
    public void EnableAfterDisable_ActiveAgainWhenScheduleMatches()
    {
        var subscription = new Subscription("Starter", SubscriptionTier.Free);
        subscription.AddOpenEndedSchedule(new DateOnly(2026, 1, 1));
        subscription.Disable();
        subscription.Enable();

        var isActive = subscription.IsActiveAt(new DateOnly(2026, 1, 2));

        Assert.True(isActive);
    }

    [Fact]
    public void IsActiveAt_NoSchedules_ReturnsFalse()
    {
        var subscription = new Subscription("Starter", SubscriptionTier.Free);

        var isActive = subscription.IsActiveAt(new DateOnly(2026, 1, 10));

        Assert.False(isActive);
    }
}
