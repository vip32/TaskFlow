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
    public void Constructor_EmptySubscriptionId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MyTaskFlowSection(Guid.Empty, "Today", 1));
    }

    [Fact]
    public void Constructor_WhitespaceName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MyTaskFlowSection(Guid.NewGuid(), " ", 1));
    }

    [Fact]
    public void CreateSystem_Recent_InitializesAsSystemSection()
    {
        var section = MyTaskFlowSection.CreateSystem(Guid.NewGuid(), "Recent", 4, TaskFlowDueBucket.Recent);

        Assert.True(section.IsSystemSection);
        Assert.Equal(TaskFlowDueBucket.Recent, section.DueBucket);
        Assert.True(section.IncludeAssignedTasks);
        Assert.True(section.IncludeUnassignedTasks);
    }

    [Fact]
    public void Rename_Whitespace_ThrowsArgumentException()
    {
        var section = new MyTaskFlowSection(Guid.NewGuid(), "Inbox", 1);

        Assert.Throws<ArgumentException>(() => section.Rename(" "));
    }

    [Fact]
    public void RenameAndReorder_ValidInput_UpdatesState()
    {
        var section = new MyTaskFlowSection(Guid.NewGuid(), "Inbox", 1);

        section.Rename(" Updated ");
        section.Reorder(10);

        Assert.Equal("Updated", section.Name);
        Assert.Equal(10, section.SortOrder);
    }

    [Fact]
    public void UpdateRule_ChangesAllFlags()
    {
        var section = new MyTaskFlowSection(Guid.NewGuid(), "Inbox", 1);

        section.UpdateRule(TaskFlowDueBucket.NoDueDate, false, true, true, true);

        Assert.Equal(TaskFlowDueBucket.NoDueDate, section.DueBucket);
        Assert.False(section.IncludeAssignedTasks);
        Assert.True(section.IncludeUnassignedTasks);
        Assert.True(section.IncludeDoneTasks);
        Assert.True(section.IncludeCancelledTasks);
    }

    [Fact]
    public void IncludeTask_EmptyTaskId_ThrowsArgumentException()
    {
        var section = new MyTaskFlowSection(Guid.NewGuid(), "Inbox", 1);

        Assert.Throws<ArgumentException>(() => section.IncludeTask(Guid.Empty));
    }

    [Fact]
    public void IncludeTask_DuplicateTask_IgnoresSecondAdd()
    {
        var section = new MyTaskFlowSection(Guid.NewGuid(), "Inbox", 1);
        var taskId = Guid.NewGuid();

        section.IncludeTask(taskId);
        section.IncludeTask(taskId);

        Assert.Single(section.ManualTasks);
    }

    [Fact]
    public void RemoveTask_RemovesManualMembership()
    {
        var section = new MyTaskFlowSection(Guid.NewGuid(), "Inbox", 1);
        var taskId = Guid.NewGuid();
        section.IncludeTask(taskId);

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

    [Fact]
    public void Matches_AssignedExcluded_ReturnsFalse()
    {
        var subscriptionId = Guid.NewGuid();
        var section = new MyTaskFlowSection(subscriptionId, "Any", 1);
        section.UpdateRule(TaskFlowDueBucket.Any, false, true, false, false);
        var task = new DomainTask(subscriptionId, "Assigned", Guid.NewGuid());
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz));

        var matches = section.Matches(task, today, today.AddDays(6), nowUtc, tz);

        Assert.False(matches);
    }

    [Fact]
    public void Matches_DoneTaskExcluded_ReturnsFalse()
    {
        var subscriptionId = Guid.NewGuid();
        var section = new MyTaskFlowSection(subscriptionId, "Any", 1);
        section.UpdateRule(TaskFlowDueBucket.Any, true, true, false, false);
        var task = new DomainTask(subscriptionId, "Done", null);
        task.SetStatus(DomainTaskStatus.Done);
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz));

        var matches = section.Matches(task, today, today.AddDays(6), nowUtc, tz);

        Assert.False(matches);
    }

    [Fact]
    public void Matches_CancelledTaskExcluded_ReturnsFalse()
    {
        var subscriptionId = Guid.NewGuid();
        var section = new MyTaskFlowSection(subscriptionId, "Any", 1);
        section.UpdateRule(TaskFlowDueBucket.Any, true, true, true, false);
        var task = new DomainTask(subscriptionId, "Cancelled", null);
        task.SetStatus(DomainTaskStatus.Cancelled);
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz));

        var matches = section.Matches(task, today, today.AddDays(6), nowUtc, tz);

        Assert.False(matches);
    }

    [Fact]
    public void Matches_ThisWeekBucket_OnlyMatchesInsideWindow()
    {
        var subscriptionId = Guid.NewGuid();
        var section = MyTaskFlowSection.CreateSystem(subscriptionId, "Week", 1, TaskFlowDueBucket.ThisWeek);
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz));
        var endOfWeek = today.AddDays(6);

        var tomorrow = new DomainTask(subscriptionId, "Tomorrow", null);
        tomorrow.SetDueDate(today.AddDays(1));
        var todayTask = new DomainTask(subscriptionId, "Today", null);
        todayTask.SetDueDate(today);

        Assert.True(section.Matches(tomorrow, today, endOfWeek, nowUtc, tz));
        Assert.False(section.Matches(todayTask, today, endOfWeek, nowUtc, tz));
    }

    [Fact]
    public void Matches_UpcomingBucket_MatchesAfterWeekEnd()
    {
        var subscriptionId = Guid.NewGuid();
        var section = MyTaskFlowSection.CreateSystem(subscriptionId, "Upcoming", 1, TaskFlowDueBucket.Upcoming);
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz));
        var endOfWeek = today.AddDays(6);
        var upcoming = new DomainTask(subscriptionId, "Next Week", null);
        upcoming.SetDueDate(endOfWeek.AddDays(1));

        var matches = section.Matches(upcoming, today, endOfWeek, nowUtc, tz);

        Assert.True(matches);
    }

    [Fact]
    public void Matches_NoDueDateBucket_MatchesWhenDueDateMissing()
    {
        var subscriptionId = Guid.NewGuid();
        var section = MyTaskFlowSection.CreateSystem(subscriptionId, "No Due", 1, TaskFlowDueBucket.NoDueDate);
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz));
        var noDue = new DomainTask(subscriptionId, "No Due", null);
        var withDue = new DomainTask(subscriptionId, "With Due", null);
        withDue.SetDueDate(today);

        Assert.True(section.Matches(noDue, today, today.AddDays(6), nowUtc, tz));
        Assert.False(section.Matches(withDue, today, today.AddDays(6), nowUtc, tz));
    }

    [Fact]
    public void Matches_RecentBucket_UsesSevenDayWindow()
    {
        var subscriptionId = Guid.NewGuid();
        var section = MyTaskFlowSection.CreateSystem(subscriptionId, "Recent", 1, TaskFlowDueBucket.Recent);
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz));
        var task = new DomainTask(subscriptionId, "Recent task", null);

        var matches = section.Matches(task, today, today.AddDays(6), nowUtc, tz);

        Assert.True(matches);
    }
}

