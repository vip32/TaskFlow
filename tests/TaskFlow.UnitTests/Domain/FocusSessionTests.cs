using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

public class FocusSessionTests
{
    [Fact]
    public void Constructor_EmptySubscriptionId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new FocusSession(Guid.Empty));
    }

    [Fact]
    public void End_CalledTwice_KeepsFirstEndTime()
    {
        var session = new FocusSession(Guid.NewGuid());

        session.End();
        var firstEnd = session.EndedAt;
        session.End();

        Assert.Equal(firstEnd, session.EndedAt);
    }

    [Fact]
    public void AttachToTask_EmptyTaskId_ThrowsArgumentException()
    {
        var session = new FocusSession(Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => session.AttachToTask(Guid.Empty));
    }
}
