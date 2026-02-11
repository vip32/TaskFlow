using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.UnitTests.Domain;

/// <summary>
/// Tests My Task Flow section rule and manual curation behavior.
/// </summary>
public class MyTaskFlowSectionTests
{
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
}
