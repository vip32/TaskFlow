using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

/// <summary>
/// Tests subscription aggregate behavior.
/// </summary>
[Trait("Layer", "Domain")]
[Trait("Slice", "Subscription")]
[Trait("Type", "Unit")]
public class SubscriptionTests
{
    [Fact]
    public void Constructor_InvalidArguments_Throws()
    {
        // Arrange

        // Act
        var emptyIdAct = () => new Subscription(Guid.Empty, "Name", SubscriptionTier.Free, true);
        var emptyNameAct = () => new Subscription(Guid.NewGuid(), " ", SubscriptionTier.Free, true);
        var emptyTimeZoneAct = () => new Subscription(Guid.NewGuid(), "Name", SubscriptionTier.Free, true, " ");

        // Assert
        Should.Throw<ArgumentException>(emptyIdAct);
        Should.Throw<ArgumentException>(emptyNameAct);
        Should.Throw<ArgumentException>(emptyTimeZoneAct);
    }

    /// <summary>
    /// Verifies tier transitions work as expected.
    /// </summary>
    [Fact]
    public void TierTransitions_UpgradeAndDowngrade_ChangesTier()
    {
        // Arrange

        // Act
        var subscription = new Subscription("Starter", SubscriptionTier.Free);

        subscription.UpgradeToPlus();

        // Assert
        subscription.Tier.ShouldBe(SubscriptionTier.Plus);

        subscription.UpgradeToPro();
        subscription.Tier.ShouldBe(SubscriptionTier.Pro);

        subscription.DowngradeToFree();
        subscription.Tier.ShouldBe(SubscriptionTier.Free);
    }

    /// <summary>
    /// Verifies disabled subscriptions are never active.
    /// </summary>
    [Fact]
    public void IsActiveAt_DisabledSubscription_ReturnsFalse()
    {
        // Arrange
        var subscription = new Subscription("Starter", SubscriptionTier.Free);
        subscription.AddOpenEndedSchedule(new DateOnly(2026, 1, 1));

        // Act
        subscription.Disable();

        var isActive = subscription.IsActiveAt(new DateOnly(2026, 1, 10));

        // Assert
        isActive.ShouldBeFalse();
    }

    /// <summary>
    /// Verifies subscriptions are active when enabled and a schedule matches.
    /// </summary>
    [Fact]
    public void IsActiveAt_EnabledWithMatchingSchedule_ReturnsTrue()
    {
        // Arrange
        var subscription = new Subscription("Starter", SubscriptionTier.Free);

        // Act
        subscription.AddScheduledWindow(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));

        var isActive = subscription.IsActiveAt(new DateOnly(2026, 1, 15));

        // Assert
        isActive.ShouldBeTrue();
    }

    /// <summary>
    /// Verifies default timezone is Europe/Berlin.
    /// </summary>
    [Fact]
    public void Constructor_DefaultTimezone_IsEuropeBerlin()
    {
        // Arrange

        // Act
        var subscription = new Subscription("Starter", SubscriptionTier.Free);

        // Assert
        subscription.TimeZoneId.ShouldBe("Europe/Berlin");
    }

    /// <summary>
    /// Verifies timezone update rejects unknown ids.
    /// </summary>
    [Fact]
    public void SetTimeZone_UnknownTimeZone_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var subscription = new Subscription("Starter", SubscriptionTier.Free);

        // Assert
        Should.Throw<ArgumentException>(() => subscription.SetTimeZone("invalid/timezone"));
    }

    [Fact]
    public void EnableAfterDisable_ActiveAgainWhenScheduleMatches()
    {
        // Arrange
        var subscription = new Subscription("Starter", SubscriptionTier.Free);
        subscription.AddOpenEndedSchedule(new DateOnly(2026, 1, 1));
        subscription.Disable();

        // Act
        subscription.Enable();

        var isActive = subscription.IsActiveAt(new DateOnly(2026, 1, 2));

        // Assert
        isActive.ShouldBeTrue();
    }

    [Fact]
    public void IsActiveAt_NoSchedules_ReturnsFalse()
    {
        // Arrange

        // Act
        var subscription = new Subscription("Starter", SubscriptionTier.Free);

        var isActive = subscription.IsActiveAt(new DateOnly(2026, 1, 10));

        // Assert
        isActive.ShouldBeFalse();
    }
}


