using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

public class TaskHistoryTests
{
    [Fact]
    public void Constructor_EmptySubscriptionId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new TaskHistory(Guid.Empty, "Name", false));
    }

    [Fact]
    public void Constructor_WhitespaceName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new TaskHistory(Guid.NewGuid(), " ", true));
    }

    [Fact]
    public void MarkUsed_IncrementsUsageCount()
    {
        var history = new TaskHistory(Guid.NewGuid(), "Plan", false);

        history.MarkUsed();

        Assert.Equal(2, history.UsageCount);
    }
}
