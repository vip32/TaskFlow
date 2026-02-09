using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.UnitTests;

/// <summary>
/// Tests task aggregate domain behavior.
/// </summary>
public class TaskDomainTests
{
    [Fact]
    public void Constructor_TitleTooLong_ThrowsArgumentException()
    {
        var title = new string('a', 501);

        Assert.Throws<ArgumentException>(() => new DomainTask(Guid.NewGuid(), title, null));
    }

    [Fact]
    public void UpdateNote_Whitespace_ClearsToNull()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);
        task.UpdateNote("Some note");

        task.UpdateNote("   ");

        Assert.Null(task.Note);
    }

    /// <summary>
    /// Verifies task can be created unassigned.
    /// </summary>
    [Fact]
    public void Constructor_EmptyProjectId_CreatesUnassignedTask()
    {
        var task = new DomainTask(Guid.NewGuid(), "Capture", null);

        Assert.True(task.IsUnassigned);
        Assert.Null(task.ProjectId);
    }

    /// <summary>
    /// Verifies assigning and unassigning cascades to subtasks.
    /// </summary>
    [Fact]
    public void AssignAndUnassign_SubTasks_CascadeProjectAssociation()
    {
        var subscriptionId = Guid.NewGuid();
        var parent = new DomainTask(subscriptionId, "Parent", null);
        var child = new DomainTask(subscriptionId, "Child", null);
        parent.AddSubTask(child);

        var projectId = Guid.NewGuid();
        parent.AssignToProject(projectId);
        Assert.Equal(projectId, parent.ProjectId);
        Assert.Equal(projectId, child.ProjectId);

        parent.UnassignFromProject();
        Assert.Null(parent.ProjectId);
        Assert.Null(child.ProjectId);
    }

    /// <summary>
    /// Verifies due date-time is converted to UTC.
    /// </summary>
    [Fact]
    public void SetDueDateTime_ValidInput_SetsUtcInstant()
    {
        var task = new DomainTask(Guid.NewGuid(), "Schedule", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var dueDate = new DateOnly(2026, 2, 10);
        var dueTime = new TimeOnly(9, 30);

        task.SetDueDateTime(dueDate, dueTime, timezone);

        Assert.True(task.HasDueDate);
        Assert.True(task.HasDueTime);
        Assert.Equal(dueDate, task.DueDateLocal);
        Assert.Equal(dueTime, task.DueTimeLocal);
        Assert.NotEqual(DateTime.MinValue, task.DueAtUtc);
    }

    /// <summary>
    /// Verifies relative reminder requires due date-time.
    /// </summary>
    [Fact]
    public void AddRelativeReminder_NoDueDateTime_ThrowsInvalidOperationException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);

        Assert.Throws<InvalidOperationException>(() => task.AddRelativeReminder(15));
    }

    /// <summary>
    /// Verifies relative reminder trigger is computed from due UTC instant.
    /// </summary>
    [Fact]
    public void AddRelativeReminder_DueDateTime_SetsExpectedTrigger()
    {
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        task.SetDueDateTime(new DateOnly(2026, 2, 10), new TimeOnly(10, 0), timezone);

        var reminder = task.AddRelativeReminder(15);

        Assert.Equal(task.DueAtUtc.Value.AddMinutes(-15), reminder.TriggerAtUtc);
        Assert.Contains(task.Reminders, existing => existing.Id == reminder.Id);
    }

    /// <summary>
    /// Verifies date-only reminder mode uses fallback local time.
    /// </summary>
    [Fact]
    public void AddDateOnlyReminder_DueDateOnly_SetsReminder()
    {
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        task.SetDueDate(new DateOnly(2026, 2, 10));

        var reminder = task.AddDateOnlyReminder(new TimeOnly(9, 0), timezone);

        Assert.Equal(TaskReminderMode.DateOnlyFallbackTime, reminder.Mode);
        Assert.Equal(new TimeOnly(9, 0), reminder.FallbackLocalTime);
    }

    /// <summary>
    /// Verifies new subtasks receive incremental sibling sort order.
    /// </summary>
    [Fact]
    public void AddSubTask_AppendsWithIncrementalSortOrder()
    {
        var subscriptionId = Guid.NewGuid();
        var parent = new DomainTask(subscriptionId, "Parent", Guid.NewGuid());
        var first = new DomainTask(subscriptionId, "First", parent.ProjectId);
        var second = new DomainTask(subscriptionId, "Second", parent.ProjectId);

        parent.AddSubTask(first);
        parent.AddSubTask(second);

        Assert.Equal(0, first.SortOrder);
        Assert.Equal(1, second.SortOrder);
    }

    /// <summary>
    /// Verifies sort order cannot be negative.
    /// </summary>
    [Fact]
    public void SetSortOrder_Negative_ThrowsArgumentException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Ordered", Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => task.SetSortOrder(-1));
    }
}
