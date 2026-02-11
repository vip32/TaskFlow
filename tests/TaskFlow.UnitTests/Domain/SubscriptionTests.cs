using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

/// <summary>
/// Tests subscription aggregate behavior.
/// </summary>
public class SubscriptionTests
{
    [Fact]
    public void Constructor_EmptyId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Subscription(Guid.Empty, "Starter", SubscriptionTier.Free, true));
    }

    [Fact]
    public void Constructor_WhitespaceName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Subscription(Guid.NewGuid(), " ", SubscriptionTier.Free, true));
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

    [Fact]
    public void Constructor_CustomTimezone_TrimsAndStores()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Starter", SubscriptionTier.Free, true, " Europe/Berlin ");

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
    public void SetTimeZone_Whitespace_ThrowsArgumentException()
    {
        var subscription = new Subscription("Starter", SubscriptionTier.Free);

        Assert.Throws<ArgumentException>(() => subscription.SetTimeZone(" "));
    }

    [Fact]
    public void SetTimeZone_ValidValue_UpdatesTimezone()
    {
        var subscription = new Subscription("Starter", SubscriptionTier.Free);

        subscription.SetTimeZone("Europe/Berlin");

        Assert.Equal("Europe/Berlin", subscription.TimeZoneId);
    }

    [Fact]
    public void EnableDisable_TogglesState()
    {
        var subscription = new Subscription("Starter", SubscriptionTier.Free);

        subscription.Disable();
        Assert.False(subscription.IsEnabled);

        subscription.Enable();
        Assert.True(subscription.IsEnabled);
    }

    [Fact]
    public void IsActiveAt_EnabledWithoutSchedules_ReturnsFalse()
    {
        var subscription = new Subscription("Starter", SubscriptionTier.Free);

        var isActive = subscription.IsActiveAt(new DateOnly(2026, 2, 1));

        Assert.False(isActive);
    }

    [Fact]
    public void AddOpenEndedSchedule_AddsSchedule()
    {
        var subscription = new Subscription("Starter", SubscriptionTier.Free);
        var startsOn = new DateOnly(2026, 2, 1);

        subscription.AddOpenEndedSchedule(startsOn);

        var schedule = Assert.Single(subscription.Schedules);
        Assert.True(schedule.IsOpenEnded);
        Assert.Equal(startsOn, schedule.StartsOn);
    }

    [Fact]
    public void AddScheduledWindow_AddsBoundedSchedule()
    {
        var subscription = new Subscription("Starter", SubscriptionTier.Free);
        var startsOn = new DateOnly(2026, 2, 1);
        var endsOn = new DateOnly(2026, 2, 28);

        subscription.AddScheduledWindow(startsOn, endsOn);

        var schedule = Assert.Single(subscription.Schedules);
        Assert.False(schedule.IsOpenEnded);
        Assert.Equal(endsOn, schedule.EndsOn);
    }
}

