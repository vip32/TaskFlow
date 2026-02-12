using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

[Trait("Layer", "Domain")]
[Trait("Slice", "Tasks")]
[Trait("Type", "Unit")]
public class TaskReminderTests
{
    [Fact]
    public void CreateRelative_WithoutDueDateTime_ThrowsInvalidOperationException()
    {
        // Arrange

        // Act
        var act = () => TaskReminder.CreateRelative(Guid.NewGuid(), 10, null);

        // Assert
        Should.Throw<InvalidOperationException>(act);
    }

    [Fact]
    public void CreateRelative_NegativeMinutes_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var act = () => TaskReminder.CreateRelative(Guid.NewGuid(), -1, DateTime.UtcNow);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateDateOnlyFallback_NullTimeZone_ThrowsArgumentNullException()
    {
        // Arrange

        // Act
        var act = () => TaskReminder.CreateDateOnlyFallback(Guid.NewGuid(), new DateOnly(2026, 1, 1), new TimeOnly(9, 0), null!);

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    [Fact]
    public void MarkSent_SecondCall_DoesNotOverrideSentAtUtc()
    {
        // Arrange
        var reminder = TaskReminder.CreateRelative(Guid.NewGuid(), 0, DateTime.UtcNow.AddHours(1));
        var first = DateTime.UtcNow;
        var second = first.AddMinutes(1);

        // Act
        reminder.MarkSent(first);
        reminder.MarkSent(second);

        // Assert
        reminder.SentAtUtc.ShouldBe(first);
    }
}


