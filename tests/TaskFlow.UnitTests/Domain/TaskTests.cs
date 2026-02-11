using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;
using DomainTaskStatus = TaskFlow.Domain.TaskStatus;

namespace TaskFlow.UnitTests.Domain;

/// <summary>
/// Tests task aggregate domain behavior.
/// </summary>
public class TaskTests
{
    [Fact]
    public void Constructor_EmptySubscriptionId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new DomainTask(Guid.Empty, "Task", null));
    }

    [Fact]
    public void Constructor_WhitespaceTitle_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new DomainTask(Guid.NewGuid(), " ", null));
    }

    [Fact]
    public void Constructor_EmptyProjectIdValue_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new DomainTask(Guid.NewGuid(), "Task", Guid.Empty));
    }

    [Fact]
    public void Constructor_TitleTooLong_ThrowsArgumentException()
    {
        var title = new string('a', 501);

        Assert.Throws<ArgumentException>(() => new DomainTask(Guid.NewGuid(), title, null));
    }

    [Fact]
    public void Constructor_ValidInput_InitializesDefaults()
    {
        var task = new DomainTask(Guid.NewGuid(), " Task ", null);

        Assert.Equal("Task", task.Title);
        Assert.Equal(TaskPriority.Medium, task.Priority);
        Assert.Equal(DomainTaskStatus.New, task.Status);
        Assert.False(task.IsCompleted);
        Assert.False(task.IsFocused);
        Assert.False(task.IsMarkedForToday);
        Assert.Equal(0, task.SortOrder);
    }

    [Fact]
    public void UpdateNote_Whitespace_ClearsToNull()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);
        task.UpdateNote("Some note");

        task.UpdateNote("   ");

        Assert.Null(task.Note);
    }

    [Fact]
    public void CompleteAndUncomplete_UpdatesState()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        task.Complete();
        Assert.True(task.IsCompleted);
        Assert.Equal(DomainTaskStatus.Done, task.Status);
        Assert.NotNull(task.CompletedAt);

        task.Uncomplete();
        Assert.False(task.IsCompleted);
        Assert.Null(task.CompletedAt);
        Assert.Equal(DomainTaskStatus.New, task.Status);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_DoesNothing()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);
        task.Complete();
        var completedAt = task.CompletedAt;

        task.Complete();

        Assert.Equal(completedAt, task.CompletedAt);
    }

    [Fact]
    public void Complete_CascadesToSubtasks()
    {
        var subscriptionId = Guid.NewGuid();
        var parent = new DomainTask(subscriptionId, "Parent", null);
        var child = new DomainTask(subscriptionId, "Child", null);
        parent.AddSubTask(child);

        parent.Complete();

        Assert.True(parent.IsCompleted);
        Assert.True(child.IsCompleted);
    }

    [Fact]
    public void Uncomplete_WhenNotCompleted_DoesNothing()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        task.Uncomplete();

        Assert.False(task.IsCompleted);
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

    [Fact]
    public void MoveToProject_DelegatesToAssign()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);
        var projectId = Guid.NewGuid();

        task.MoveToProject(projectId);

        Assert.Equal(projectId, task.ProjectId);
    }

    [Fact]
    public void AssignToProject_EmptyId_ThrowsArgumentException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        Assert.Throws<ArgumentException>(() => task.AssignToProject(Guid.Empty));
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

    [Fact]
    public void SetPriority_UpdatesPriority()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        task.SetPriority(TaskPriority.High);

        Assert.Equal(TaskPriority.High, task.Priority);
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

    [Fact]
    public void SetDueDate_MinValue_ThrowsArgumentException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        Assert.Throws<ArgumentException>(() => task.SetDueDate(DateOnly.MinValue));
    }

    [Fact]
    public void SetDueDateTime_NullTimeZone_ThrowsArgumentNullException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        Assert.Throws<ArgumentNullException>(() => task.SetDueDateTime(new DateOnly(2026, 2, 10), new TimeOnly(9, 30), null!));
    }

    [Fact]
    public void SetDueDateTime_MinDate_ThrowsArgumentException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

        Assert.Throws<ArgumentException>(() => task.SetDueDateTime(DateOnly.MinValue, new TimeOnly(9, 30), timezone));
    }

    [Fact]
    public void ClearDueDate_ResetsAllDueFields()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        task.SetDueDateTime(new DateOnly(2026, 2, 10), new TimeOnly(9, 30), timezone);

        task.ClearDueDate();

        Assert.False(task.HasDueDate);
        Assert.False(task.HasDueTime);
        Assert.Null(task.DueAtUtc);
    }

    [Fact]
    public void ToggleFocus_AndTodayMark_ToggleFlags()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        task.ToggleFocus();
        task.ToggleTodayMark();

        Assert.True(task.IsFocused);
        Assert.True(task.IsMarkedForToday);
    }

    [Fact]
    public void UpdateTitle_Whitespace_ThrowsArgumentException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        Assert.Throws<ArgumentException>(() => task.UpdateTitle(" "));
    }

    [Fact]
    public void UpdateTitle_TooLong_ThrowsArgumentException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        Assert.Throws<ArgumentException>(() => task.UpdateTitle(new string('x', 501)));
    }

    [Fact]
    public void UpdateTitle_ValidTitle_TrimsAndSets()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        task.UpdateTitle(" Updated ");

        Assert.Equal("Updated", task.Title);
    }

    [Fact]
    public void SetStatus_Done_CompletesTask()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        task.SetStatus(DomainTaskStatus.Done);

        Assert.True(task.IsCompleted);
        Assert.Equal(DomainTaskStatus.Done, task.Status);
        Assert.NotNull(task.CompletedAt);
    }

    [Fact]
    public void SetStatus_InProgress_AfterDone_UncompletesTask()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);
        task.SetStatus(DomainTaskStatus.Done);

        task.SetStatus(DomainTaskStatus.InProgress);

        Assert.False(task.IsCompleted);
        Assert.Equal(DomainTaskStatus.InProgress, task.Status);
        Assert.Null(task.CompletedAt);
    }

    [Fact]
    public void SetStatus_Cancelled_ClearsCompletion()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);
        task.SetStatus(DomainTaskStatus.Done);

        task.SetStatus(DomainTaskStatus.Cancelled);

        Assert.False(task.IsCompleted);
        Assert.Equal(DomainTaskStatus.Cancelled, task.Status);
        Assert.Null(task.CompletedAt);
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

    [Fact]
    public void AddOnTimeReminder_UsesZeroMinutesBefore()
    {
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        task.SetDueDateTime(new DateOnly(2026, 2, 10), new TimeOnly(10, 0), timezone);

        var reminder = task.AddOnTimeReminder();

        Assert.Equal(0, reminder.MinutesBefore);
        Assert.Equal(task.DueAtUtc, reminder.TriggerAtUtc);
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

    [Fact]
    public void AddDateOnlyReminder_NoDueDate_ThrowsInvalidOperationException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

        Assert.Throws<InvalidOperationException>(() => task.AddDateOnlyReminder(new TimeOnly(9, 0), timezone));
    }

    [Fact]
    public void RemoveReminder_RemovesMatchingReminder()
    {
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        task.SetDueDateTime(new DateOnly(2026, 2, 10), new TimeOnly(10, 0), timezone);
        var reminder = task.AddOnTimeReminder();

        task.RemoveReminder(reminder.Id);

        Assert.Empty(task.Reminders);
    }

    [Fact]
    public void MarkReminderSent_UnknownReminder_ThrowsKeyNotFoundException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);

        Assert.Throws<KeyNotFoundException>(() => task.MarkReminderSent(Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void MarkReminderSent_ExistingReminder_MarksAsSent()
    {
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        task.SetDueDateTime(new DateOnly(2026, 2, 10), new TimeOnly(10, 0), timezone);
        var reminder = task.AddOnTimeReminder();
        var sentAt = DateTime.UtcNow;

        task.MarkReminderSent(reminder.Id, sentAt);

        var updated = Assert.Single(task.Reminders);
        Assert.Equal(sentAt, updated.SentAtUtc);
    }

    [Fact]
    public void Tags_AddRemoveAndSet_AreNormalizedAndDistinct()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        task.AddTag(" urgent ");
        task.AddTag("URGENT");
        task.AddTag("backend");
        task.RemoveTag(" UrGent ");
        task.SetTags([" alpha ", "ALPHA", " beta "]);

        Assert.Equal(2, task.Tags.Count);
        Assert.Contains("alpha", task.Tags);
        Assert.Contains("beta", task.Tags);
    }

    [Fact]
    public void AddTag_Whitespace_ThrowsArgumentException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        Assert.Throws<ArgumentException>(() => task.AddTag(" "));
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

    [Fact]
    public void AddSubTask_SelfReference_ThrowsInvalidOperationException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        Assert.Throws<InvalidOperationException>(() => task.AddSubTask(task));
    }

    [Fact]
    public void AddSubTask_Duplicate_ThrowsInvalidOperationException()
    {
        var subscriptionId = Guid.NewGuid();
        var parent = new DomainTask(subscriptionId, "Parent", null);
        var child = new DomainTask(subscriptionId, "Child", null);
        parent.AddSubTask(child);

        Assert.Throws<InvalidOperationException>(() => parent.AddSubTask(child));
    }

    [Fact]
    public void AddSubTask_ToUnassignedParent_UnassignsChild()
    {
        var subscriptionId = Guid.NewGuid();
        var parent = new DomainTask(subscriptionId, "Parent", null);
        var child = new DomainTask(subscriptionId, "Child", Guid.NewGuid());

        parent.AddSubTask(child);

        Assert.Null(child.ProjectId);
        Assert.Equal(parent.Id, child.ParentTaskId);
    }

    /// <summary>
    /// Verifies tasks reject subtasks from a different subscription.
    /// </summary>
    [Fact]
    public void AddSubTask_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        var parent = new DomainTask(Guid.NewGuid(), "Parent", Guid.NewGuid());
        var subTask = new DomainTask(Guid.NewGuid(), "Child", parent.ProjectId);

        Action act = () => parent.AddSubTask(subTask);

        Assert.Throws<InvalidOperationException>(act);
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

    [Fact]
    public void SetSortOrder_Valid_UpdatesSortOrder()
    {
        var task = new DomainTask(Guid.NewGuid(), "Ordered", Guid.NewGuid());

        task.SetSortOrder(5);

        Assert.Equal(5, task.SortOrder);
    }
}

