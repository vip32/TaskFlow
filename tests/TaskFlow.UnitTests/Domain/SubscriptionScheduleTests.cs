using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

/// <summary>
/// Tests subscription schedule domain behavior.
/// </summary>
public class SubscriptionScheduleTests
{
    [Fact]
    public void CreateOpenEnded_EmptySubscriptionId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SubscriptionSchedule.CreateOpenEnded(Guid.Empty, new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void CreateWindow_EmptySubscriptionId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SubscriptionSchedule.CreateWindow(Guid.Empty, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2)));
    }

    /// <summary>
    /// Verifies open-ended schedules are marked correctly.
    /// </summary>
    [Fact]
    public void CreateOpenEnded_ValidInput_SetsOpenEndedSchedule()
    {
        var subscriptionId = Guid.NewGuid();
        var startsOn = new DateOnly(2026, 1, 1);

        var schedule = SubscriptionSchedule.CreateOpenEnded(subscriptionId, startsOn);

        Assert.Equal(subscriptionId, schedule.SubscriptionId);
        Assert.Equal(startsOn, schedule.StartsOn);
        Assert.Null(schedule.EndsOn);
        Assert.True(schedule.IsOpenEnded);
    }

    /// <summary>
    /// Verifies bounded schedules reject invalid date windows.
    /// </summary>
    [Fact]
    public void CreateWindow_EndBeforeStart_ThrowsArgumentException()
    {
        var subscriptionId = Guid.NewGuid();
        var startsOn = new DateOnly(2026, 2, 1);
        var endsOn = new DateOnly(2026, 1, 31);

        Assert.Throws<ArgumentException>(() => SubscriptionSchedule.CreateWindow(subscriptionId, startsOn, endsOn));
    }

    /// <summary>
    /// Verifies activity checks are inclusive of start and end dates.
    /// </summary>
    [Fact]
    public void IsActiveAt_BoundedSchedule_IsInclusiveOnBoundaries()
    {
        var schedule = SubscriptionSchedule.CreateWindow(
            Guid.NewGuid(),
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 1, 31));

        Assert.False(schedule.IsActiveAt(new DateOnly(2025, 12, 31)));
        Assert.True(schedule.IsActiveAt(new DateOnly(2026, 1, 1)));
        Assert.True(schedule.IsActiveAt(new DateOnly(2026, 1, 31)));
        Assert.False(schedule.IsActiveAt(new DateOnly(2026, 2, 1)));
    }
}

