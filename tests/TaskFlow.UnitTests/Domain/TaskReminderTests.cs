using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

public class TaskReminderTests
{
    [Fact]
    public void CreateRelative_NoDueDateTime_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => TaskReminder.CreateRelative(Guid.NewGuid(), 15, null));
    }

    [Fact]
    public void CreateRelative_NegativeMinutes_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => TaskReminder.CreateRelative(Guid.NewGuid(), -1, DateTime.UtcNow));
    }

    [Fact]
    public void CreateRelative_EmptyTaskId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => TaskReminder.CreateRelative(Guid.Empty, 0, DateTime.UtcNow));
    }

    [Fact]
    public void CreateRelative_ValidInput_CreatesReminder()
    {
        var dueAt = new DateTime(2026, 2, 10, 10, 0, 0, DateTimeKind.Utc);

        var reminder = TaskReminder.CreateRelative(Guid.NewGuid(), 15, dueAt);

        Assert.Equal(TaskReminderMode.RelativeToDueDateTime, reminder.Mode);
        Assert.Equal(15, reminder.MinutesBefore);
        Assert.Equal(dueAt.AddMinutes(-15), reminder.TriggerAtUtc);
        Assert.Null(reminder.SentAtUtc);
    }

    [Fact]
    public void CreateDateOnlyFallback_NullTimeZone_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TaskReminder.CreateDateOnlyFallback(Guid.NewGuid(), new DateOnly(2026, 2, 10), new TimeOnly(9, 0), null!));
    }

    [Fact]
    public void CreateDateOnlyFallback_MinDate_ThrowsInvalidOperationException()
    {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

        Assert.Throws<InvalidOperationException>(() =>
            TaskReminder.CreateDateOnlyFallback(Guid.NewGuid(), DateOnly.MinValue, new TimeOnly(9, 0), timezone));
    }

    [Fact]
    public void CreateDateOnlyFallback_EmptyTaskId_ThrowsArgumentException()
    {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

        Assert.Throws<ArgumentException>(() =>
            TaskReminder.CreateDateOnlyFallback(Guid.Empty, new DateOnly(2026, 2, 10), new TimeOnly(9, 0), timezone));
    }

    [Fact]
    public void CreateDateOnlyFallback_ValidInput_CreatesReminder()
    {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

        var reminder = TaskReminder.CreateDateOnlyFallback(
            Guid.NewGuid(),
            new DateOnly(2026, 2, 10),
            new TimeOnly(9, 0),
            timezone);

        Assert.Equal(TaskReminderMode.DateOnlyFallbackTime, reminder.Mode);
        Assert.Equal(new TimeOnly(9, 0), reminder.FallbackLocalTime);
        Assert.Null(reminder.SentAtUtc);
    }

    [Fact]
    public void MarkSent_MinValue_ThrowsArgumentException()
    {
        var reminder = TaskReminder.CreateRelative(Guid.NewGuid(), 0, DateTime.UtcNow);

        Assert.Throws<ArgumentException>(() => reminder.MarkSent(DateTime.MinValue));
    }

    [Fact]
    public void MarkSent_SecondCall_IsIgnored()
    {
        var reminder = TaskReminder.CreateRelative(Guid.NewGuid(), 0, DateTime.UtcNow);
        var first = DateTime.UtcNow;
        reminder.MarkSent(first);

        reminder.MarkSent(first.AddMinutes(1));

        Assert.Equal(first, reminder.SentAtUtc);
    }
}
