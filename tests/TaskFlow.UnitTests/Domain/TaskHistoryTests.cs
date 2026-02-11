using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

public class TaskHistoryTests
{
    [Fact]
    public void Constructor_EmptySubscriptionId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new TaskHistory(Guid.Empty, "Refactor", false));
    }

    [Fact]
    public void Constructor_WhitespaceName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new TaskHistory(Guid.NewGuid(), " ", false));
    }

    [Fact]
    public void Constructor_ValidInput_InitializesEntry()
    {
        var subscriptionId = Guid.NewGuid();

        var history = new TaskHistory(subscriptionId, " Refactor ", true);

        Assert.NotEqual(Guid.Empty, history.Id);
        Assert.Equal(subscriptionId, history.SubscriptionId);
        Assert.Equal("Refactor", history.Name);
        Assert.True(history.IsSubTaskName);
        Assert.Equal(1, history.UsageCount);
    }

    [Fact]
    public void MarkUsed_IncrementsUsageCountAndUpdatesTimestamp()
    {
        var history = new TaskHistory(Guid.NewGuid(), "Refactor", false);
        var lastUsed = history.LastUsedAt;

        history.MarkUsed();

        Assert.Equal(2, history.UsageCount);
        Assert.True(history.LastUsedAt >= lastUsed);
    }
}
