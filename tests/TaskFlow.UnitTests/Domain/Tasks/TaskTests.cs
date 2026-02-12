using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;
using DomainTaskStatus = TaskFlow.Domain.TaskStatus;

namespace TaskFlow.UnitTests.Domain;

[Trait("Layer", "Domain")]
[Trait("Slice", "Tasks")]
[Trait("Type", "Unit")]
public class TaskTests
{
    [Fact]
    public void Constructor_InvalidArguments_ThrowArgumentException()
    {
        // Arrange

        // Act
        var title = new string('a', 501);

        // Assert
        Should.Throw<ArgumentException>(() => new DomainTask(Guid.Empty, "Task", null));
        Should.Throw<ArgumentException>(() => new DomainTask(Guid.NewGuid(), " ", null));
        Should.Throw<ArgumentException>(() => new DomainTask(Guid.NewGuid(), title, null));
        Should.Throw<ArgumentException>(() => new DomainTask(Guid.NewGuid(), "Task", Guid.Empty));
    }

    [Fact]
    public void UpdateNote_Whitespace_ClearsToNull()
    {
        // Arrange
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        // Act
        task.UpdateNote("Some note");

        task.UpdateNote("   ");

        // Assert
        task.Note.ShouldBeNull();
    }

    [Fact]
    public void Constructor_EmptyProjectId_CreatesUnassignedTask()
    {
        // Arrange

        // Act
        var task = new DomainTask(Guid.NewGuid(), "Capture", null);

        // Assert
        task.IsUnassigned.ShouldBeTrue();
        task.ProjectId.ShouldBeNull();
    }

    [Fact]
    public void CompleteAndUncomplete_UpdateState()
    {
        // Arrange

        // Act
        var task = new DomainTask(Guid.NewGuid(), "Task", Guid.NewGuid());

        task.Complete();

        // Assert
        task.IsCompleted.ShouldBeTrue();
        task.Status.ShouldBe(DomainTaskStatus.Done);
        task.CompletedAt.ShouldNotBeNull();

        task.Uncomplete();
        task.IsCompleted.ShouldBeFalse();
        task.Status.ShouldBe(DomainTaskStatus.Todo);
        task.CompletedAt.ShouldBeNull();
    }

    [Fact]
    public void AddSubTask_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        // Arrange

        // Act
        var parent = new DomainTask(Guid.NewGuid(), "Parent", Guid.NewGuid());
        var subTask = new DomainTask(Guid.NewGuid(), "Child", parent.ProjectId);

        // Assert
        Should.Throw<InvalidOperationException>(() => parent.AddSubTask(subTask));
    }

    [Fact]
    public void AddSubTask_SelfOrDuplicate_ThrowsInvalidOperationException()
    {
        // Arrange

        // Act
        var subscriptionId = Guid.NewGuid();
        var parent = new DomainTask(subscriptionId, "Parent", Guid.NewGuid());

        // Assert
        Should.Throw<InvalidOperationException>(() => parent.AddSubTask(parent));

        var child = new DomainTask(subscriptionId, "Child", parent.ProjectId);
        parent.AddSubTask(child);
        Should.Throw<InvalidOperationException>(() => parent.AddSubTask(child));
    }

    [Fact]
    public void AssignAndUnassign_SubTasks_CascadeProjectAssociation()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var parent = new DomainTask(subscriptionId, "Parent", null);
        var child = new DomainTask(subscriptionId, "Child", null);
        parent.AddSubTask(child);

        // Act
        var projectId = Guid.NewGuid();
        parent.AssignToProject(projectId);

        // Assert
        parent.ProjectId.ShouldBe(projectId);
        child.ProjectId.ShouldBe(projectId);

        parent.UnassignFromProject();
        parent.ProjectId.ShouldBeNull();
        child.ProjectId.ShouldBeNull();
    }

    [Fact]
    public void MoveToProject_EmptyId_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        // Assert
        Should.Throw<ArgumentException>(() => task.MoveToProject(Guid.Empty));
    }

    [Fact]
    public void SetDueDateTime_ValidInput_SetsUtcInstant()
    {
        // Arrange
        var task = new DomainTask(Guid.NewGuid(), "Schedule", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var dueDate = new DateOnly(2026, 2, 10);

        // Act
        var dueTime = new TimeOnly(9, 30);

        task.SetDueDateTime(dueDate, dueTime, timezone);

        // Assert
        task.HasDueDate.ShouldBeTrue();
        task.HasDueTime.ShouldBeTrue();
        task.DueDateLocal.ShouldBe(dueDate);
        task.DueTimeLocal.ShouldBe(dueTime);
        task.DueAtUtc.ShouldNotBe(DateTime.MinValue);
    }

    [Fact]
    public void SetDueDate_AndClearDueDate_Work()
    {
        // Arrange

        // Act
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        task.SetDueDate(new DateOnly(2026, 2, 10));

        // Assert
        task.HasDueDate.ShouldBeTrue();
        task.HasDueTime.ShouldBeFalse();

        task.ClearDueDate();
        task.HasDueDate.ShouldBeFalse();
        task.HasDueTime.ShouldBeFalse();
        task.DueAtUtc.ShouldBeNull();
    }

    [Fact]
    public void SetDueDate_MinValue_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        // Assert
        Should.Throw<ArgumentException>(() => task.SetDueDate(DateOnly.MinValue));
    }

    [Fact]
    public void AddRelativeReminder_NoDueDateTime_ThrowsInvalidOperationException()
    {
        // Arrange

        // Act
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);

        // Assert
        Should.Throw<InvalidOperationException>(() => task.AddRelativeReminder(15));
    }

    [Fact]
    public void AddRelativeReminder_DueDateTime_SetsExpectedTrigger()
    {
        // Arrange
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

        // Act
        task.SetDueDateTime(new DateOnly(2026, 2, 10), new TimeOnly(10, 0), timezone);

        var reminder = task.AddRelativeReminder(15);

        // Assert
        reminder.TriggerAtUtc.ShouldBe(task.DueAtUtc!.Value.AddMinutes(-15));
        task.Reminders.Any(existing => existing.Id == reminder.Id).ShouldBeTrue();
    }

    [Fact]
    public void AddDateOnlyReminder_DueDateOnly_SetsReminder()
    {
        // Arrange
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

        // Act
        task.SetDueDate(new DateOnly(2026, 2, 10));

        var reminder = task.AddDateOnlyReminder(new TimeOnly(9, 0), timezone);

        // Assert
        reminder.Mode.ShouldBe(TaskReminderMode.DateOnlyFallbackTime);
        reminder.FallbackLocalTime.ShouldBe(new TimeOnly(9, 0));
    }

    [Fact]
    public void AddDateOnlyReminder_WithoutDueDate_ThrowsInvalidOperationException()
    {
        // Arrange

        // Act
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

        // Assert
        Should.Throw<InvalidOperationException>(() => task.AddDateOnlyReminder(new TimeOnly(9, 0), timezone));
    }

    [Fact]
    public void RemoveAndMarkReminderSent_Work()
    {
        // Arrange
        var task = new DomainTask(Guid.NewGuid(), "Reminder", null);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        task.SetDueDateTime(new DateOnly(2026, 2, 10), new TimeOnly(10, 0), timezone);

        // Act
        var reminder = task.AddOnTimeReminder();

        task.MarkReminderSent(reminder.Id, DateTime.UtcNow);

        // Assert
        task.Reminders.Single().SentAtUtc.ShouldNotBeNull();

        task.RemoveReminder(reminder.Id);
        task.Reminders.ShouldBeEmpty();
    }

    [Fact]
    public void MarkReminderSent_UnknownId_ThrowsEntityNotFoundException()
    {
        // Arrange

        // Act
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        // Assert
        Should.Throw<EntityNotFoundException>(() => task.MarkReminderSent(Guid.NewGuid(), DateTime.UtcNow));
    }

    [Fact]
    public void ToggleImportant_SubTask_ThrowsInvalidOperationException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var parent = new DomainTask(subscriptionId, "Parent", Guid.NewGuid());

        // Act
        var child = new DomainTask(subscriptionId, "Child", parent.ProjectId);
        parent.AddSubTask(child);

        // Assert
        Should.Throw<InvalidOperationException>(() => child.ToggleImportant());
    }

    [Fact]
    public void ToggleFlagsAndStatus_Work()
    {
        // Arrange
        var task = new DomainTask(Guid.NewGuid(), "Task", Guid.NewGuid());

        task.ToggleFocus();
        task.ToggleImportant();

        // Act
        task.ToggleTodayMark();
        task.SetStatus(DomainTaskStatus.Doing);

        // Assert
        task.IsFocused.ShouldBeTrue();
        task.IsImportant.ShouldBeTrue();
        task.IsMarkedForToday.ShouldBeTrue();
        task.Status.ShouldBe(DomainTaskStatus.Doing);

        task.SetStatus(DomainTaskStatus.Done);
        task.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    public void AddSubTask_AppendsWithIncrementalSortOrder()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var parent = new DomainTask(subscriptionId, "Parent", Guid.NewGuid());
        var first = new DomainTask(subscriptionId, "First", parent.ProjectId);
        var second = new DomainTask(subscriptionId, "Second", parent.ProjectId);

        // Act
        parent.AddSubTask(first);
        parent.AddSubTask(second);

        // Assert
        first.SortOrder.ShouldBe(0);
        second.SortOrder.ShouldBe(1);
    }

    [Fact]
    public void SetSortOrder_Negative_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var task = new DomainTask(Guid.NewGuid(), "Ordered", Guid.NewGuid());

        // Assert
        Should.Throw<ArgumentException>(() => task.SetSortOrder(-1));
    }

    [Fact]
    public void UpdateTitle_Invalid_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        // Assert
        Should.Throw<ArgumentException>(() => task.UpdateTitle(" "));
    }

    [Fact]
    public void TagOperations_AreCaseInsensitive()
    {
        // Arrange
        var task = new DomainTask(Guid.NewGuid(), "Task", null);

        // Act
        task.AddTag("Study");
        task.AddTag("study");

        // Assert
        task.Tags.ShouldHaveSingleItem();

        task.RemoveTag("STUDY");
        task.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void Rehydrate_PopulatesFields()
    {
        // Arrange
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

        // Act
            isMarkedForToday: true,
            tags: ["X"]);

        // Assert
        task.Id.ShouldBe(id);
        task.Note.ShouldBe("note");
        task.Priority.ShouldBe(TaskPriority.High);
        task.IsCompleted.ShouldBeTrue();
        task.IsFocused.ShouldBeTrue();
        task.IsImportant.ShouldBeTrue();
        task.Status.ShouldBe(DomainTaskStatus.Done);
        task.SortOrder.ShouldBe(0);
        task.Tags.ShouldHaveSingleItem();
    }
}


