using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

[Trait("Layer", "Domain")]
[Trait("Slice", "Focus")]
[Trait("Type", "Unit")]
public class FocusSessionTests
{
    [Fact]
    public void Constructor_EmptySubscriptionId_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var act = () => new FocusSession(Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void End_CalledTwice_KeepsFirstEndTime()
    {
        // Arrange
        var sut = new FocusSession(Guid.NewGuid());
        sut.End();

        // Act
        var firstEnd = sut.EndedAt;
        sut.End();

        // Assert
        sut.EndedAt.ShouldBe(firstEnd);
    }

    [Fact]
    public void AttachToTask_EmptyTaskId_ThrowsArgumentException()
    {
        // Arrange
        var sut = new FocusSession(Guid.NewGuid());

        // Act
        var act = () => sut.AttachToTask(Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Constructor_WithTask_AssignsTaskId()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        // Act
        var sut = new FocusSession(Guid.NewGuid(), taskId);

        // Assert
        sut.TaskId.ShouldBe(taskId);
        sut.IsCompleted.ShouldBeFalse();
    }

    [Fact]
    public void End_MarksSessionCompleted()
    {
        // Arrange
        var sut = new FocusSession(Guid.NewGuid());

        // Act
        sut.End();

        // Assert
        sut.IsCompleted.ShouldBeTrue();
        sut.EndedAt.ShouldNotBe(DateTime.MinValue);
    }

    [Fact]
    public void AttachToTask_AssignsTaskId()
    {
        // Arrange
        var sut = new FocusSession(Guid.NewGuid());
        var taskId = Guid.NewGuid();

        // Act
        sut.AttachToTask(taskId);

        // Assert
        sut.TaskId.ShouldBe(taskId);
    }
}


