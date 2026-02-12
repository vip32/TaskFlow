using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

/// <summary>
/// Tests subscription schedule domain behavior.
/// </summary>
public class SubscriptionScheduleTests
{
    /// <summary>
    /// Verifies open-ended schedules are marked correctly.
    /// </summary>
    [Fact]
    public void CreateOpenEnded_ValidInput_SetsOpenEndedSchedule()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        // Act
        var startsOn = new DateOnly(2026, 1, 1);

        var schedule = SubscriptionSchedule.CreateOpenEnded(subscriptionId, startsOn);

        // Assert
        schedule.SubscriptionId.ShouldBe(subscriptionId);
        schedule.StartsOn.ShouldBe(startsOn);
        schedule.EndsOn.ShouldBeNull();
        schedule.IsOpenEnded.ShouldBeTrue();
    }

    /// <summary>
    /// Verifies bounded schedules reject invalid date windows.
    /// </summary>
    [Fact]
    public void CreateWindow_EndBeforeStart_ThrowsArgumentException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        // Act
        var startsOn = new DateOnly(2026, 2, 1);
        var endsOn = new DateOnly(2026, 1, 31);

        // Assert
        Should.Throw<ArgumentException>(() => SubscriptionSchedule.CreateWindow(subscriptionId, startsOn, endsOn));
    }

    /// <summary>
    /// Verifies activity checks are inclusive of start and end dates.
    /// </summary>
    [Fact]
    public void IsActiveAt_BoundedSchedule_IsInclusiveOnBoundaries()
    {
        // Arrange
        var schedule = SubscriptionSchedule.CreateWindow(
            Guid.NewGuid(),

        // Act
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));

        // Assert
        schedule.IsActiveAt(new DateOnly(2025, 12, 31)).ShouldBeFalse();
        schedule.IsActiveAt(new DateOnly(2026, 1, 1)).ShouldBeTrue();
        schedule.IsActiveAt(new DateOnly(2026, 1, 31)).ShouldBeTrue();
        schedule.IsActiveAt(new DateOnly(2026, 2, 1)).ShouldBeFalse();
    }
}
