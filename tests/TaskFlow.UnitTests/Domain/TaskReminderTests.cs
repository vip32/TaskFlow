using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

public class TaskReminderTests
{
    [Fact]
    public void CreateRelative_WithoutDueDateTime_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => TaskReminder.CreateRelative(Guid.NewGuid(), 10, null));
    }

    [Fact]
    public void CreateRelative_NegativeMinutes_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => TaskReminder.CreateRelative(Guid.NewGuid(), -1, DateTime.UtcNow));
    }

    [Fact]
    public void CreateDateOnlyFallback_NullTimeZone_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => TaskReminder.CreateDateOnlyFallback(Guid.NewGuid(), new DateOnly(2026, 1, 1), new TimeOnly(9, 0), null!));
    }

    [Fact]
    public void MarkSent_SecondCall_DoesNotOverrideSentAtUtc()
    {
        var reminder = TaskReminder.CreateRelative(Guid.NewGuid(), 0, DateTime.UtcNow.AddHours(1));
        var first = DateTime.UtcNow;
        var second = first.AddMinutes(1);

        reminder.MarkSent(first);
        reminder.MarkSent(second);

        Assert.Equal(first, reminder.SentAtUtc);
    }
}
