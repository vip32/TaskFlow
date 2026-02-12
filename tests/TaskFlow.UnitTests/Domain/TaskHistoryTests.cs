using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

public class TaskHistoryTests
{
    [Fact]
    public void Constructor_EmptySubscriptionId_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var act = () => new TaskHistory(Guid.Empty, "Name", false);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Constructor_WhitespaceName_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var act = () => new TaskHistory(Guid.NewGuid(), " ", true);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void MarkUsed_IncrementsUsageCount()
    {
        // Arrange

        // Act
        var history = new TaskHistory(Guid.NewGuid(), "Plan", false);

        history.MarkUsed();

        // Assert
        history.UsageCount.ShouldBe(2);
    }
}
