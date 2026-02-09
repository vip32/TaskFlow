using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.UnitTests;

/// <summary>
/// Tests subscription boundary invariants in domain aggregates.
/// </summary>
public class DomainIsolationTests
{
    /// <summary>
    /// Verifies projects reject tasks from a different subscription.
    /// </summary>
    [Fact]
    public void ProjectAddTask_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        var project = new Project(Guid.NewGuid(), "Work", "#40E0D0", "work");
        var task = new DomainTask(Guid.NewGuid(), "Task", project.Id);

        Action act = () => project.AddTask(task);

        Assert.Throws<InvalidOperationException>(act);
    }

    /// <summary>
    /// Verifies tasks reject subtasks from a different subscription.
    /// </summary>
    [Fact]
    public void TaskAddSubTask_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        var parent = new DomainTask(Guid.NewGuid(), "Parent", Guid.NewGuid());
        var subTask = new DomainTask(Guid.NewGuid(), "Child", parent.ProjectId);

        Action act = () => parent.AddSubTask(subTask);

        Assert.Throws<InvalidOperationException>(act);
    }

    /// <summary>
    /// Verifies focus sessions require a valid subscription id.
    /// </summary>
    [Fact]
    public void FocusSession_EmptySubscriptionId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new FocusSession(Guid.Empty));
    }
}
