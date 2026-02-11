using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;
using DomainTaskStatus = TaskFlow.Domain.TaskStatus;

namespace TaskFlow.UnitTests.Domain;

/// <summary>
/// Tests My Task Flow section rule and manual curation behavior.
/// </summary>
public class MyTaskFlowSectionTests
{
    [Fact]
    public void Constructor_InvalidInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MyTaskFlowSection(Guid.Empty, "Name", 1));
        Assert.Throws<ArgumentException>(() => new MyTaskFlowSection(Guid.NewGuid(), " ", 1));
    }

    [Fact]
    public void RenameAndReorder_UpdatesValues()
    {
        var section = new MyTaskFlowSection(Guid.NewGuid(), "Old", 10);

        section.Rename("New");
        section.Reorder(4);

        Assert.Equal("New", section.Name);
        Assert.Equal(4, section.SortOrder);
    }

    [Fact]
    public void UpdateRule_UpdatesAllFlags()
    {
        var section = new MyTaskFlowSection(Guid.NewGuid(), "Custom", 10);

        section.UpdateRule(TaskFlowDueBucket.NoDueDate, includeAssignedTasks: false, includeUnassignedTasks: true, includeDoneTasks: true, includeCancelledTasks: true);

        Assert.Equal(TaskFlowDueBucket.NoDueDate, section.DueBucket);
        Assert.False(section.IncludeAssignedTasks);
        Assert.True(section.IncludeUnassignedTasks);
        Assert.True(section.IncludeDoneTasks);
        Assert.True(section.IncludeCancelledTasks);
    }

    [Fact]
    public void IncludeTask_DuplicateIgnored_AndRemoveTask_Works()
    {
        var section = new MyTaskFlowSection(Guid.NewGuid(), "Custom", 1);
        var taskId = Guid.NewGuid();

        section.IncludeTask(taskId);
        section.IncludeTask(taskId);
        Assert.Single(section.ManualTasks);

        section.RemoveTask(taskId);
        Assert.Empty(section.ManualTasks);
    }

    /// <summary>
    /// Verifies manually included tasks always match.
    /// </summary>
    [Fact]
    public void Matches_ManualInclude_ReturnsTrue()
    {
        var subscriptionId = Guid.NewGuid();
        var section = new MyTaskFlowSection(subscriptionId, "Two-minute", 10);
        var task = new DomainTask(subscriptionId, "Quick fix", null);
        section.IncludeTask(task.Id);

        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));

        var matches = section.Matches(task, today, today.AddDays(6), nowUtc, timezone);

        Assert.True(matches);
    }

    /// <summary>
    /// Verifies today due bucket includes due-today and marked-today tasks.
    /// </summary>
    [Fact]
    public void Matches_TodayBucket_DueOrMarkedTask_ReturnsTrue()
    {
        var subscriptionId = Guid.NewGuid();
        var section = MyTaskFlowSection.CreateSystem(subscriptionId, "Today", 1, TaskFlowDueBucket.Today);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));

        var dueTask = new DomainTask(subscriptionId, "Due", Guid.NewGuid());
        dueTask.SetDueDate(today);

        var markedTask = new DomainTask(subscriptionId, "Marked", null);
        markedTask.ToggleTodayMark();

        var dueMatches = section.Matches(dueTask, today, today.AddDays(6), nowUtc, timezone);
        var markedMatches = section.Matches(markedTask, today, today.AddDays(6), nowUtc, timezone);

        Assert.True(dueMatches);
        Assert.True(markedMatches);
    }

    /// <summary>
    /// Verifies important due bucket only includes starred tasks.
    /// </summary>
    [Fact]
    public void Matches_ImportantBucket_OnlyImportantTasksMatch()
    {
        var subscriptionId = Guid.NewGuid();
        var section = MyTaskFlowSection.CreateSystem(subscriptionId, "Important", 2, TaskFlowDueBucket.Important);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));

        var importantTask = new DomainTask(subscriptionId, "Starred", Guid.NewGuid());
        importantTask.ToggleImportant();

        var regularTask = new DomainTask(subscriptionId, "Regular", Guid.NewGuid());

        var importantMatches = section.Matches(importantTask, today, today.AddDays(6), nowUtc, timezone);
        var regularMatches = section.Matches(regularTask, today, today.AddDays(6), nowUtc, timezone);

        Assert.True(importantMatches);
        Assert.False(regularMatches);
    }

    /// <summary>
    /// Verifies legacy Important system sections still behave as important-only.
    /// </summary>
    [Fact]
    public void Matches_LegacyImportantByName_OnlyImportantTasksMatch()
    {
        var subscriptionId = Guid.NewGuid();
        var section = MyTaskFlowSection.CreateSystem(subscriptionId, "Important", 2, TaskFlowDueBucket.Any);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));

        var importantTask = new DomainTask(subscriptionId, "Starred", Guid.NewGuid());
        importantTask.ToggleImportant();
        var regularTask = new DomainTask(subscriptionId, "Regular", Guid.NewGuid());

        Assert.True(section.Matches(importantTask, today, today.AddDays(6), nowUtc, timezone));
        Assert.False(section.Matches(regularTask, today, today.AddDays(6), nowUtc, timezone));
    }

    [Fact]
    public void Matches_AssignedFiltering_Respected()
    {
        var subscriptionId = Guid.NewGuid();
        var section = new MyTaskFlowSection(subscriptionId, "Custom", 1);
        section.UpdateRule(TaskFlowDueBucket.Any, includeAssignedTasks: false, includeUnassignedTasks: true, includeDoneTasks: false, includeCancelledTasks: false);
        var task = new DomainTask(subscriptionId, "Assigned", Guid.NewGuid());
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));

        var matches = section.Matches(task, today, today.AddDays(6), nowUtc, timezone);

        Assert.False(matches);
    }

    [Fact]
    public void Matches_DoneTask_ExcludedByDefault()
    {
        var subscriptionId = Guid.NewGuid();
        var section = new MyTaskFlowSection(subscriptionId, "Custom", 1);
        var task = new DomainTask(subscriptionId, "Done", null);
        task.SetStatus(DomainTaskStatus.Done);
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));

        var matches = section.Matches(task, today, today.AddDays(6), nowUtc, timezone);

        Assert.False(matches);
    }

    [Fact]
    public void Matches_UpcomingAndNoDueDateBuckets_WorkAsExpected()
    {
        var subscriptionId = Guid.NewGuid();
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timezone));
        var endOfWeek = today.AddDays(6);

        var upcoming = MyTaskFlowSection.CreateSystem(subscriptionId, "Upcoming", 1, TaskFlowDueBucket.Upcoming);
        var noDue = MyTaskFlowSection.CreateSystem(subscriptionId, "No due", 2, TaskFlowDueBucket.NoDueDate);

        var futureTask = new DomainTask(subscriptionId, "Future", null);
        futureTask.SetDueDate(endOfWeek.AddDays(3));
        var noDueTask = new DomainTask(subscriptionId, "NoDue", null);

        Assert.True(upcoming.Matches(futureTask, today, endOfWeek, nowUtc, timezone));
        Assert.True(noDue.Matches(noDueTask, today, endOfWeek, nowUtc, timezone));
    }
}
