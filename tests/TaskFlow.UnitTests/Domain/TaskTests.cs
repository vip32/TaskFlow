using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;
using DomainTaskStatus = TaskFlow.Domain.TaskStatus;

namespace TaskFlow.UnitTests.Domain;

public class TaskTests
{
    [Fact]
    public void Constructor_InvalidArguments_ThrowArgumentException()
    {
        var title = new string('a', 501);

        Assert.Throws<ArgumentException>(() => new DomainTask(Guid.Empty, "Task", null));
        Assert.Throws<ArgumentException>(() => new DomainTask(Guid.NewGuid(), " ", null));
        Assert.Throws<ArgumentException>(() => new DomainTask(Guid.NewGuid(), title, null));
        Assert.Throws<ArgumentException>(() => new DomainTask(Guid.NewGuid(), "Task", Guid.Empty));
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
    public void Constructor_EmptyProjectId_CreatesUnassignedTask()
    {
        var task = new DomainTask(Guid.NewGuid(), "Capture", null);

        Assert.True(task.IsUnassigned);
        Assert.Null(task.ProjectId);
    }

    [Fact]
    public void CompleteAndUncomplete_UpdateState()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", Guid.NewGuid());

        task.Complete();
        Assert.True(task.IsCompleted);
        Assert.Equal(DomainTaskStatus.Done, task.Status);
        Assert.NotNull(task.CompletedAt);

        task.Uncomplete();
        Assert.False(task.IsCompleted);
        Assert.Equal(DomainTaskStatus.Todo, task.Status);
        Assert.Null(task.CompletedAt);
    }

    [Fact]
    public void AddSubTask_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        var parent = new DomainTask(Guid.NewGuid(), "Parent", Guid.NewGuid());
        var subTask = new DomainTask(Guid.NewGuid(), "Child", parent.ProjectId);

        Assert.Throws<InvalidOperationException>(() => parent.AddSubTask(subTask));
    }

    [Fact]
    public void AddSubTask_SelfOrDuplicate_ThrowsInvalidOperationException()
    {
        var subscriptionId = Guid.NewGuid();
        var parent = new DomainTask(subscriptionId, "Parent", Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => parent.AddSubTask(parent));

        var child = new DomainTask(subscriptionId, "Child", parent.ProjectId);
        parent.AddSubTask(child);
        Assert.Throws<InvalidOperationException>(() => parent.AddSubTask(child));
    }

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
    public void MoveToProject_EmptyId_ThrowsArgumentException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        Assert.Throws<ArgumentException>(() => task.MoveToProject(Guid.Empty));
    }

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
    public void SetDueDate_AndClearDueDate_Work()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        task.SetDueDate(new DateOnly(2026, 2, 10));
        Assert.True(task.HasDueDate);
        Assert.False(task.HasDueTime);

        task.ClearDueDate();
        Assert.False(task.HasDueDate);
        Assert.False(task.HasDueTime);
        Assert.Null(task.DueAtUtc);
    }

    [Fact]
    public void SetDueDate_MinValue_ThrowsArgumentException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        Assert.Throws<ArgumentException>(() => task.SetDueDate(DateOnly.MinValue));
    }

    [Fact]
    public void AddRelativeReminder_NoDueDateTime_ThrowsInvalidOperationException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);

        Assert.Throws<InvalidOperationException>(() => task.AddRelativeReminder(15));
    }

    [Fact]
    public void AddRelativeReminder_DueDateTime_SetsExpectedTrigger()
    {
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        task.SetDueDateTime(new DateOnly(2026, 2, 10), new TimeOnly(10, 0), timezone);

        var reminder = task.AddRelativeReminder(15);

        Assert.Equal(task.DueAtUtc!.Value.AddMinutes(-15), reminder.TriggerAtUtc);
        Assert.Contains(task.Reminders, existing => existing.Id == reminder.Id);
    }

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
    public void AddDateOnlyReminder_WithoutDueDate_ThrowsInvalidOperationException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

        Assert.Throws<InvalidOperationException>(() => task.AddDateOnlyReminder(new TimeOnly(9, 0), timezone));
    }

    [Fact]
    public void RemoveAndMarkReminderSent_Work()
    {
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        task.SetDueDateTime(new DateOnly(2026, 2, 10), new TimeOnly(10, 0), timezone);
        var reminder = task.AddOnTimeReminder();

        task.MarkReminderSent(reminder.Id, DateTime.UtcNow);
        Assert.NotNull(task.Reminders.Single().SentAtUtc);

        task.RemoveReminder(reminder.Id);
        Assert.Empty(task.Reminders);
    }

    [Fact]
    public void MarkReminderSent_UnknownId_ThrowsKeyNotFoundException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        Assert.Throws<KeyNotFoundException>(() => task.MarkReminderSent(Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void ToggleImportant_SubTask_ThrowsInvalidOperationException()
    {
        var subscriptionId = Guid.NewGuid();
        var parent = new DomainTask(subscriptionId, "Parent", Guid.NewGuid());
        var child = new DomainTask(subscriptionId, "Child", parent.ProjectId);
        parent.AddSubTask(child);

        Assert.Throws<InvalidOperationException>(() => child.ToggleImportant());
    }

    [Fact]
    public void ToggleFlagsAndStatus_Work()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", Guid.NewGuid());

        task.ToggleFocus();
        task.ToggleImportant();
        task.ToggleTodayMark();
        task.SetStatus(DomainTaskStatus.Doing);

        Assert.True(task.IsFocused);
        Assert.True(task.IsImportant);
        Assert.True(task.IsMarkedForToday);
        Assert.Equal(DomainTaskStatus.Doing, task.Status);

        task.SetStatus(DomainTaskStatus.Done);
        Assert.True(task.IsCompleted);
    }

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
    public void SetSortOrder_Negative_ThrowsArgumentException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Ordered", Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => task.SetSortOrder(-1));
    }

    [Fact]
    public void UpdateTitle_Invalid_ThrowsArgumentException()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        Assert.Throws<ArgumentException>(() => task.UpdateTitle(" "));
    }

    [Fact]
    public void TagOperations_AreCaseInsensitive()
    {
        var task = new DomainTask(Guid.NewGuid(), "Task", null);
        task.AddTag("Study");
        task.AddTag("study");

        Assert.Single(task.Tags);

        task.RemoveTag("STUDY");
        Assert.Empty(task.Tags);
    }

    [Fact]
    public void Rehydrate_PopulatesFields()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var completedAt = DateTime.UtcNow.AddHours(-1);
        var task = DomainTask.Rehydrate(
            Guid.NewGuid(),
            id,
            "Title",
            projectId: null,
            parentTaskId: null,
            note: "note",
            priority: TaskPriority.High,
            isCompleted: true,
            isFocused: true,
            isImportant: true,
            status: DomainTaskStatus.Done,
            createdAt: createdAt,
            completedAt: completedAt,
            sortOrder: -10,
            dueDateLocal: new DateOnly(2026, 2, 10),
            dueTimeLocal: new TimeOnly(9, 0),
            dueAtUtc: DateTime.UtcNow,
            isMarkedForToday: true,
            tags: ["X"]);

        Assert.Equal(id, task.Id);
        Assert.Equal("note", task.Note);
        Assert.Equal(TaskPriority.High, task.Priority);
        Assert.True(task.IsCompleted);
        Assert.True(task.IsFocused);
        Assert.True(task.IsImportant);
        Assert.Equal(DomainTaskStatus.Done, task.Status);
        Assert.Equal(0, task.SortOrder);
        Assert.Single(task.Tags);
    }
}
