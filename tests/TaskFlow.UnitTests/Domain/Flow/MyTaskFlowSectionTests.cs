using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;
using DomainTaskStatus = TaskFlow.Domain.TaskStatus;

namespace TaskFlow.UnitTests.Domain;

/// <summary>
/// Tests My Task Flow section rule and manual curation behavior.
/// </summary>
[Trait("Layer", "Domain")]
[Trait("Slice", "Flow")]
[Trait("Type", "Unit")]
public class MyTaskFlowSectionTests
{
    [Fact]
    public void Constructor_InvalidInput_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var emptySubscriptionAct = () => new MyTaskFlowSection(Guid.Empty, "Name", 1);
        var emptyNameAct = () => new MyTaskFlowSection(Guid.NewGuid(), " ", 1);

        // Assert
        Should.Throw<ArgumentException>(emptySubscriptionAct);
        Should.Throw<ArgumentException>(emptyNameAct);
    }

    [Fact]
    public void RenameAndReorder_UpdatesValues()
    {
        // Arrange
        var section = new MyTaskFlowSection(Guid.NewGuid(), "Old", 10);

        // Act
        section.Rename("New");
        section.Reorder(4);

        // Assert
        section.Name.ShouldBe("New");
        section.SortOrder.ShouldBe(4);
    }

    [Fact]
    public void UpdateRule_UpdatesAllFlags()
    {
        // Arrange

        // Act
        var section = new MyTaskFlowSection(Guid.NewGuid(), "Custom", 10);

        section.UpdateRule(TaskFlowDueBucket.NoDueDate, includeAssignedTasks: false, includeUnassignedTasks: true, includeDoneTasks: true, includeCancelledTasks: true);

        // Assert
        section.DueBucket.ShouldBe(TaskFlowDueBucket.NoDueDate);
        section.IncludeAssignedTasks.ShouldBeFalse();
        section.IncludeUnassignedTasks.ShouldBeTrue();
        section.IncludeDoneTasks.ShouldBeTrue();
        section.IncludeCancelledTasks.ShouldBeTrue();
    }

    [Fact]
    public void IncludeTask_DuplicateIgnored_AndRemoveTask_Works()
    {
        // Arrange
        var section = new MyTaskFlowSection(Guid.NewGuid(), "Custom", 1);
        var taskId = Guid.NewGuid();

        // Act
        section.IncludeTask(taskId);
        section.IncludeTask(taskId);

        // Assert
        section.ManualTasks.ShouldHaveSingleItem();

        section.RemoveTask(taskId);
        section.ManualTasks.ShouldBeEmpty();
    }

    /// <summary>
    /// Verifies manually included tasks always match.
    /// </summary>
    [Fact]
    public void Matches_ManualInclude_ReturnsTrue()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var section = new MyTaskFlowSection(subscriptionId, "Two-minute", 10);
        var task = new DomainTask(subscriptionId, "Quick fix", null);
        section.IncludeTask(task.Id);

        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;

        // Act
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));

        var matches = section.Matches(task, today, today.AddDays(6), nowUtc, timezone);

        // Assert
        matches.ShouldBeTrue();
    }

    /// <summary>
    /// Verifies today due bucket includes due-today and marked-today tasks.
    /// </summary>
    [Fact]
    public void Matches_TodayBucket_DueOrMarkedTask_ReturnsTrue()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var section = MyTaskFlowSection.CreateSystem(subscriptionId, "Today", 1, TaskFlowDueBucket.Today);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));

        var dueTask = new DomainTask(subscriptionId, "Due", Guid.NewGuid());
        dueTask.SetDueDate(today);

        var markedTask = new DomainTask(subscriptionId, "Marked", null);
        markedTask.ToggleTodayMark();

        // Act
        var dueMatches = section.Matches(dueTask, today, today.AddDays(6), nowUtc, timezone);
        var markedMatches = section.Matches(markedTask, today, today.AddDays(6), nowUtc, timezone);

        // Assert
        dueMatches.ShouldBeTrue();
        markedMatches.ShouldBeTrue();
    }

    /// <summary>
    /// Verifies important due bucket only includes starred tasks.
    /// </summary>
    [Fact]
    public void Matches_ImportantBucket_OnlyImportantTasksMatch()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var section = MyTaskFlowSection.CreateSystem(subscriptionId, "Important", 2, TaskFlowDueBucket.Important);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));

        var importantTask = new DomainTask(subscriptionId, "Starred", Guid.NewGuid());
        importantTask.ToggleImportant();

        var regularTask = new DomainTask(subscriptionId, "Regular", Guid.NewGuid());

        // Act
        var importantMatches = section.Matches(importantTask, today, today.AddDays(6), nowUtc, timezone);
        var regularMatches = section.Matches(regularTask, today, today.AddDays(6), nowUtc, timezone);

        // Assert
        importantMatches.ShouldBeTrue();
        regularMatches.ShouldBeFalse();
    }

    /// <summary>
    /// Verifies legacy Important system sections still behave as important-only.
    /// </summary>
    [Fact]
    public void Matches_LegacyImportantByName_OnlyImportantTasksMatch()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var section = MyTaskFlowSection.CreateSystem(subscriptionId, "Important", 2, TaskFlowDueBucket.Any);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));

        var importantTask = new DomainTask(subscriptionId, "Starred", Guid.NewGuid());

        // Act
        importantTask.ToggleImportant();
        var regularTask = new DomainTask(subscriptionId, "Regular", Guid.NewGuid());

        // Assert
        section.Matches(importantTask, today, today.AddDays(6), nowUtc, timezone).ShouldBeTrue();
        section.Matches(regularTask, today, today.AddDays(6), nowUtc, timezone).ShouldBeFalse();
    }

    [Fact]
    public void Matches_AssignedFiltering_Respected()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var section = new MyTaskFlowSection(subscriptionId, "Custom", 1);
        section.UpdateRule(TaskFlowDueBucket.Any, includeAssignedTasks: false, includeUnassignedTasks: true, includeDoneTasks: false, includeCancelledTasks: false);
        var task = new DomainTask(subscriptionId, "Assigned", Guid.NewGuid());
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;

        // Act
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));

        var matches = section.Matches(task, today, today.AddDays(6), nowUtc, timezone);

        // Assert
        matches.ShouldBeFalse();
    }

    [Fact]
    public void Matches_DoneTask_ExcludedByDefault()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var section = new MyTaskFlowSection(subscriptionId, "Custom", 1);
        var task = new DomainTask(subscriptionId, "Done", null);
        task.SetStatus(DomainTaskStatus.Done);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;

        // Act
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));

        var matches = section.Matches(task, today, today.AddDays(6), nowUtc, timezone);

        // Assert
        matches.ShouldBeFalse();
    }

    [Fact]
    public void Matches_UpcomingAndNoDueDateBuckets_WorkAsExpected()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));
        var endOfWeek = today.AddDays(6);

        var upcoming = MyTaskFlowSection.CreateSystem(subscriptionId, "Upcoming", 1, TaskFlowDueBucket.Upcoming);
        var noDue = MyTaskFlowSection.CreateSystem(subscriptionId, "No due", 2, TaskFlowDueBucket.NoDueDate);

        var futureTask = new DomainTask(subscriptionId, "Future", null);

        // Act
        futureTask.SetDueDate(endOfWeek.AddDays(3));
        var noDueTask = new DomainTask(subscriptionId, "NoDue", null);

        // Assert
        upcoming.Matches(futureTask, today, endOfWeek, nowUtc, timezone).ShouldBeTrue();
        noDue.Matches(noDueTask, today, endOfWeek, nowUtc, timezone).ShouldBeTrue();
    }
}


