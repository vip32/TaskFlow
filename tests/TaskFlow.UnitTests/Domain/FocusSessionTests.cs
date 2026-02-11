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
    public void Constructor_ValidInput_InitializesRunningSession()
    {
        var session = new FocusSession(Guid.NewGuid());

        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.Equal(Guid.Empty, session.TaskId);
        Assert.False(session.IsCompleted);
        Assert.Equal(DateTime.MinValue, session.EndedAt);
    }

    [Fact]
    public void Constructor_WithTaskId_AssignsTask()
    {
        var taskId = Guid.NewGuid();

        var session = new FocusSession(Guid.NewGuid(), taskId);

        Assert.Equal(taskId, session.TaskId);
    }

    [Fact]
    public void End_WhenRunning_MarksAsCompleted()
    {
        var session = new FocusSession(Guid.NewGuid());

        session.End();

        Assert.True(session.IsCompleted);
        Assert.NotEqual(DateTime.MinValue, session.EndedAt);
    }

    [Fact]
    public void End_WhenAlreadyCompleted_DoesNotChangeEndTime()
    {
        var session = new FocusSession(Guid.NewGuid());
        session.End();
        var endedAt = session.EndedAt;

        session.End();

        Assert.Equal(endedAt, session.EndedAt);
    }

    [Fact]
    public void AttachToTask_EmptyTaskId_ThrowsArgumentException()
    {
        var session = new FocusSession(Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => session.AttachToTask(Guid.Empty));
    }

    [Fact]
    public void AttachToTask_ValidTaskId_SetsTaskId()
    {
        var session = new FocusSession(Guid.NewGuid());
        var taskId = Guid.NewGuid();

        session.AttachToTask(taskId);

        Assert.Equal(taskId, session.TaskId);
    }

    [Fact]
    public void Duration_WhenCompleted_UsesEndedAt()
    {
        var session = new FocusSession(Guid.NewGuid());
        session.End();

        var duration = session.Duration;

        Assert.True(duration >= TimeSpan.Zero);
    }
}

