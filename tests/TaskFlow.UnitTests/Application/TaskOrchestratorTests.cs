using TaskFlow.Application;
using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;
using DomainTaskStatus = TaskFlow.Domain.TaskStatus;

namespace TaskFlow.UnitTests.Application;

/// <summary>
/// Tests task orchestration behavior.
/// </summary>
public class TaskOrchestratorTests
{
    /// <summary>
    /// Verifies create task uses current subscription and persists immediately.
    /// </summary>
    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ValidInput_PersistsTask()
    {
        var subscription = CreateSubscription();
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeTaskRepository();
        var historyRepository = new FakeTaskHistoryRepository();
        var orchestrator = new TaskOrchestrator(repository, historyRepository, accessor);

        var created = await orchestrator.CreateAsync(Guid.NewGuid(), "Draft roadmap", TaskPriority.High, "Initial note");

        Assert.Equal(subscription.Id, created.SubscriptionId);
        Assert.Equal(TaskPriority.High, created.Priority);
        Assert.Equal(1, repository.AddCallCount);
    }

    /// <summary>
    /// Verifies unassigned task create stores empty project id.
    /// </summary>
    [Fact]
    public async System.Threading.Tasks.Task CreateUnassignedAsync_ValidInput_PersistsUnassignedTask()
    {
        var subscription = CreateSubscription();
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeTaskRepository();
        var historyRepository = new FakeTaskHistoryRepository();
        var orchestrator = new TaskOrchestrator(repository, historyRepository, accessor);

        var created = await orchestrator.CreateUnassignedAsync("Inbox idea", TaskPriority.Medium, string.Empty);

        Assert.Null(created.ProjectId);
        Assert.True(created.IsUnassigned);
        Assert.Equal(1, repository.AddCallCount);
    }

    /// <summary>
    /// Verifies newly created project task gets next persisted sort order.
    /// </summary>
    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ExistingProjectTasks_AssignsNextSortOrder()
    {
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var existing = new DomainTask(subscription.Id, "Existing", projectId);
        existing.SetSortOrder(3);

        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeTaskRepository(existing);
        var historyRepository = new FakeTaskHistoryRepository();
        var orchestrator = new TaskOrchestrator(repository, historyRepository, accessor);

        var created = await orchestrator.CreateAsync(projectId, "New", TaskPriority.Medium, string.Empty);

        Assert.Equal(4, created.SortOrder);
    }

    /// <summary>
    /// Verifies title updates persist immediately.
    /// </summary>
    [Fact]
    public async System.Threading.Tasks.Task UpdateTitleAsync_ExistingTask_PersistsChange()
    {
        var subscription = CreateSubscription();
        var existing = new DomainTask(subscription.Id, "Old", Guid.NewGuid());
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeTaskRepository(existing);
        var historyRepository = new FakeTaskHistoryRepository();
        var orchestrator = new TaskOrchestrator(repository, historyRepository, accessor);

        var updated = await orchestrator.UpdateTitleAsync(existing.Id, "New");

        Assert.Equal("New", updated.Title);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    /// <summary>
    /// Verifies today bucket includes due-today and today-marked tasks.
    /// </summary>
    [Fact]
    public async System.Threading.Tasks.Task GetTodayAsync_DueAndMarkedTasks_ReturnsMergedUniqueList()
    {
        var subscription = CreateSubscription();
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(subscription.TimeZoneId);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));

        var dueToday = new DomainTask(subscription.Id, "Due today", Guid.NewGuid());
        dueToday.SetDueDate(today);

        var markedToday = new DomainTask(subscription.Id, "Marked today", null);
        markedToday.ToggleTodayMark();

        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeTaskRepository(dueToday, markedToday);
        var historyRepository = new FakeTaskHistoryRepository();
        var orchestrator = new TaskOrchestrator(repository, historyRepository, accessor);

        var tasks = await orchestrator.GetTodayAsync();

        Assert.Equal(2, tasks.Count);
        Assert.Contains(tasks, task => task.Id == dueToday.Id);
        Assert.Contains(tasks, task => task.Id == markedToday.Id);
    }

    /// <summary>
    /// Verifies project task reorder persists requested order.
    /// </summary>
    [Fact]
    public async System.Threading.Tasks.Task ReorderProjectTasksAsync_ValidOrder_PersistsSortOrder()
    {
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();

        var first = new DomainTask(subscription.Id, "First", projectId);
        var second = new DomainTask(subscription.Id, "Second", projectId);
        first.SetSortOrder(0);
        second.SetSortOrder(1);

        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeTaskRepository(first, second);
        var historyRepository = new FakeTaskHistoryRepository();
        var orchestrator = new TaskOrchestrator(repository, historyRepository, accessor);

        var reordered = await orchestrator.ReorderProjectTasksAsync(projectId, [second.Id, first.Id]);

        Assert.Equal(second.Id, reordered[0].Id);
        Assert.Equal(0, reordered[0].SortOrder);
        Assert.Equal(1, reordered[1].SortOrder);
    }

    /// <summary>
    /// Verifies moving a subtask directly is rejected.
    /// </summary>
    [Fact]
    public async System.Threading.Tasks.Task MoveToProjectAsync_SubTask_ThrowsInvalidOperationException()
    {
        var subscription = CreateSubscription();
        var parent = new DomainTask(subscription.Id, "Parent", Guid.NewGuid());
        var child = new DomainTask(subscription.Id, "Child", parent.ProjectId);
        parent.AddSubTask(child);

        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeTaskRepository(parent, child);
        var historyRepository = new FakeTaskHistoryRepository();
        var orchestrator = new TaskOrchestrator(repository, historyRepository, accessor);

        await Assert.ThrowsAsync<InvalidOperationException>(() => orchestrator.MoveToProjectAsync(child.Id, Guid.NewGuid()));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByProjectIdAsync_ReturnsProjectTasks()
    {
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var first = new DomainTask(subscription.Id, "First", projectId);
        var second = new DomainTask(subscription.Id, "Second", projectId);
        var repository = new FakeTaskRepository(first, second);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        var tasks = await orchestrator.GetByProjectIdAsync(projectId);

        Assert.Equal(2, tasks.Count);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetSubTasksAsync_ReturnsSubTasks()
    {
        var subscription = CreateSubscription();
        var parent = new DomainTask(subscription.Id, "Parent", Guid.NewGuid());
        var child = new DomainTask(subscription.Id, "Child", parent.ProjectId);
        parent.AddSubTask(child);
        var repository = new FakeTaskRepository(parent, child);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        var tasks = await orchestrator.GetSubTasksAsync(parent.Id);

        Assert.Single(tasks);
        Assert.Equal(child.Id, tasks[0].Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task SearchAsync_DelegatesToRepository()
    {
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var match = new DomainTask(subscription.Id, "Roadmap draft", projectId);
        var other = new DomainTask(subscription.Id, "Personal note", Guid.NewGuid());
        var repository = new FakeTaskRepository(match, other);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        var tasks = await orchestrator.SearchAsync(projectId, "roadmap");

        Assert.Single(tasks);
        Assert.Equal(match.Id, tasks[0].Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetNameSuggestionsAsync_ReturnsHistorySuggestions()
    {
        var subscription = CreateSubscription();
        var historyRepository = new FakeTaskHistoryRepository(["Draft", "Deploy"]);
        var orchestrator = new TaskOrchestrator(new FakeTaskRepository(), historyRepository, new FakeCurrentSubscriptionAccessor(subscription));

        var suggestions = await orchestrator.GetNameSuggestionsAsync("D", false, 10);

        Assert.Equal(2, suggestions.Count);
        Assert.Equal("Draft", suggestions[0]);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_RegistersHistoryUsage()
    {
        var subscription = CreateSubscription();
        var historyRepository = new FakeTaskHistoryRepository();
        var orchestrator = new TaskOrchestrator(new FakeTaskRepository(), historyRepository, new FakeCurrentSubscriptionAccessor(subscription));

        var created = await orchestrator.CreateAsync(Guid.NewGuid(), "Draft roadmap", TaskPriority.High, "note");

        Assert.Equal("Draft roadmap", historyRepository.LastRegisteredName);
        Assert.False(historyRepository.LastRegisteredWasSubTask);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTitleAsync_SubTask_RegistersSubTaskUsage()
    {
        var subscription = CreateSubscription();
        var parent = new DomainTask(subscription.Id, "Parent", Guid.NewGuid());
        var child = new DomainTask(subscription.Id, "Child", parent.ProjectId);
        parent.AddSubTask(child);
        var historyRepository = new FakeTaskHistoryRepository();
        var orchestrator = new TaskOrchestrator(new FakeTaskRepository(parent, child), historyRepository, new FakeCurrentSubscriptionAccessor(subscription));

        var updated = await orchestrator.UpdateTitleAsync(child.Id, "Updated child");

        Assert.Equal("Updated child", updated.Title);
        Assert.True(historyRepository.LastRegisteredWasSubTask);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateNoteAsync_ExistingTask_PersistsChange()
    {
        var subscription = CreateSubscription();
        var task = new DomainTask(subscription.Id, "Task", Guid.NewGuid());
        var repository = new FakeTaskRepository(task);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        var updated = await orchestrator.UpdateNoteAsync(task.Id, "Updated note");

        Assert.Equal("Updated note", updated.Note);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task SetPriorityAsync_ExistingTask_PersistsChange()
    {
        var subscription = CreateSubscription();
        var task = new DomainTask(subscription.Id, "Task", Guid.NewGuid());
        var repository = new FakeTaskRepository(task);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        var updated = await orchestrator.SetPriorityAsync(task.Id, TaskPriority.Low);

        Assert.Equal(TaskPriority.Low, updated.Priority);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task SetStatusAsync_ExistingTask_PersistsChange()
    {
        var subscription = CreateSubscription();
        var task = new DomainTask(subscription.Id, "Task", Guid.NewGuid());
        var repository = new FakeTaskRepository(task);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        var updated = await orchestrator.SetStatusAsync(task.Id, DomainTaskStatus.Done);

        Assert.Equal(DomainTaskStatus.Done, updated.Status);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task ToggleFocusAsync_ExistingTask_TogglesFocus()
    {
        var subscription = CreateSubscription();
        var task = new DomainTask(subscription.Id, "Task", Guid.NewGuid());
        var repository = new FakeTaskRepository(task);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        var updated = await orchestrator.ToggleFocusAsync(task.Id);

        Assert.True(updated.IsFocused);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task MoveToProjectAsync_ParentTask_AssignsProjectAndSortOrder()
    {
        var subscription = CreateSubscription();
        var task = new DomainTask(subscription.Id, "Task", Guid.NewGuid());
        var targetProjectId = Guid.NewGuid();
        var existingInTarget = new DomainTask(subscription.Id, "Existing", targetProjectId);
        existingInTarget.SetSortOrder(2);
        var repository = new FakeTaskRepository(task, existingInTarget);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        var moved = await orchestrator.MoveToProjectAsync(task.Id, targetProjectId);

        Assert.Equal(targetProjectId, moved.ProjectId);
        Assert.Equal(3, moved.SortOrder);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReorderProjectTasksAsync_EmptyOrder_ReturnsCurrentSortedOrder()
    {
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var first = new DomainTask(subscription.Id, "First", projectId);
        var second = new DomainTask(subscription.Id, "Second", projectId);
        first.SetSortOrder(0);
        second.SetSortOrder(1);
        var repository = new FakeTaskRepository(first, second);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        var reordered = await orchestrator.ReorderProjectTasksAsync(projectId, []);

        Assert.Equal(first.Id, reordered[0].Id);
        Assert.Equal(second.Id, reordered[1].Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReorderProjectTasksAsync_DuplicateId_ThrowsArgumentException()
    {
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var task = new DomainTask(subscription.Id, "Task", projectId);
        var repository = new FakeTaskRepository(task);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        await Assert.ThrowsAsync<ArgumentException>(() => orchestrator.ReorderProjectTasksAsync(projectId, [task.Id, task.Id]));
    }

    [Fact]
    public async System.Threading.Tasks.Task ReorderProjectTasksAsync_UnknownId_ThrowsArgumentException()
    {
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var task = new DomainTask(subscription.Id, "Task", projectId);
        var repository = new FakeTaskRepository(task);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        await Assert.ThrowsAsync<ArgumentException>(() => orchestrator.ReorderProjectTasksAsync(projectId, [Guid.NewGuid()]));
    }

    [Fact]
    public async System.Threading.Tasks.Task ReorderSubTasksAsync_ValidOrder_PersistsSortOrder()
    {
        var subscription = CreateSubscription();
        var parent = new DomainTask(subscription.Id, "Parent", Guid.NewGuid());
        var first = new DomainTask(subscription.Id, "First", parent.ProjectId);
        var second = new DomainTask(subscription.Id, "Second", parent.ProjectId);
        parent.AddSubTask(first);
        parent.AddSubTask(second);
        var repository = new FakeTaskRepository(parent, first, second);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        var reordered = await orchestrator.ReorderSubTasksAsync(parent.Id, [second.Id, first.Id]);

        Assert.Equal(second.Id, reordered[0].Id);
        Assert.Equal(0, reordered[0].SortOrder);
    }

    [Fact]
    public async System.Threading.Tasks.Task UnassignFromProjectAsync_ParentTask_UnassignsAndUpdates()
    {
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var task = new DomainTask(subscription.Id, "Task", projectId);
        var existingUnassigned = new DomainTask(subscription.Id, "Existing unassigned", null);
        existingUnassigned.SetSortOrder(1);
        var repository = new FakeTaskRepository(task, existingUnassigned);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        var updated = await orchestrator.UnassignFromProjectAsync(task.Id);

        Assert.Null(updated.ProjectId);
        Assert.Equal(2, updated.SortOrder);
    }

    [Fact]
    public async System.Threading.Tasks.Task UnassignFromProjectAsync_SubTask_ThrowsInvalidOperationException()
    {
        var subscription = CreateSubscription();
        var parent = new DomainTask(subscription.Id, "Parent", Guid.NewGuid());
        var child = new DomainTask(subscription.Id, "Child", parent.ProjectId);
        parent.AddSubTask(child);
        var repository = new FakeTaskRepository(parent, child);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        await Assert.ThrowsAsync<InvalidOperationException>(() => orchestrator.UnassignFromProjectAsync(child.Id));
    }

    [Fact]
    public async System.Threading.Tasks.Task DueDateMethods_SetAndClear_PersistChanges()
    {
        var subscription = CreateSubscription();
        var task = new DomainTask(subscription.Id, "Task", Guid.NewGuid());
        var repository = new FakeTaskRepository(task);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));
        var dueDate = new DateOnly(2026, 2, 20);

        await orchestrator.SetDueDateAsync(task.Id, dueDate);
        Assert.Equal(dueDate, task.DueDateLocal);

        await orchestrator.SetDueDateTimeAsync(task.Id, dueDate, new TimeOnly(9, 30));
        Assert.Equal(new TimeOnly(9, 30), task.DueTimeLocal);

        await orchestrator.ClearDueDateAsync(task.Id);
        Assert.False(task.HasDueDate);
    }

    [Fact]
    public async System.Threading.Tasks.Task ToggleTodayMarkAsync_ExistingTask_TogglesFlag()
    {
        var subscription = CreateSubscription();
        var task = new DomainTask(subscription.Id, "Task", null);
        var repository = new FakeTaskRepository(task);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        var updated = await orchestrator.ToggleTodayMarkAsync(task.Id);

        Assert.True(updated.IsMarkedForToday);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReminderMethods_AddRemoveRelativeAndDateOnly_PersistChanges()
    {
        var subscription = CreateSubscription();
        var task = new DomainTask(subscription.Id, "Task", null);
        task.SetDueDateTime(new DateOnly(2026, 2, 20), new TimeOnly(10, 0), TimeZoneInfo.FindSystemTimeZoneById(subscription.TimeZoneId));
        var repository = new FakeTaskRepository(task);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        var withRelative = await orchestrator.AddRelativeReminderAsync(task.Id, 15);
        var reminderId = Assert.Single(withRelative.Reminders).Id;
        await orchestrator.RemoveReminderAsync(task.Id, reminderId);
        Assert.Empty(task.Reminders);
        await orchestrator.SetDueDateAsync(task.Id, new DateOnly(2026, 2, 21));
        var withDateOnly = await orchestrator.AddDateOnlyReminderAsync(task.Id, new TimeOnly(8, 0));

        Assert.Single(withDateOnly.Reminders);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetRecentThisWeekUpcomingAndDelete_DelegateToRepository()
    {
        var subscription = CreateSubscription();
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(subscription.TimeZoneId);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        var daysUntilSunday = DayOfWeek.Sunday - today.DayOfWeek;
        var endOfWeek = today.AddDays(daysUntilSunday);

        var recent = new DomainTask(subscription.Id, "Recent", Guid.NewGuid());
        var thisWeek = new DomainTask(subscription.Id, "ThisWeek", Guid.NewGuid());
        var thisWeekDueDate = daysUntilSunday > 0 ? today.AddDays(1) : today.AddDays(8);
        thisWeek.SetDueDate(thisWeekDueDate);
        var upcoming = new DomainTask(subscription.Id, "Upcoming", Guid.NewGuid());
        upcoming.SetDueDate(endOfWeek.AddDays(1));

        var repository = new FakeTaskRepository(recent, thisWeek, upcoming);
        var orchestrator = new TaskOrchestrator(repository, new FakeTaskHistoryRepository(), new FakeCurrentSubscriptionAccessor(subscription));

        var recents = await orchestrator.GetRecentAsync();
        var week = await orchestrator.GetThisWeekAsync();
        var up = await orchestrator.GetUpcomingAsync();
        var deleted = await orchestrator.DeleteAsync(recent.Id);

        Assert.Contains(recents, x => x.Id == recent.Id);
        if (daysUntilSunday > 0)
        {
            Assert.Single(week);
            Assert.Single(up);
        }
        else
        {
            Assert.Empty(week);
            Assert.Equal(2, up.Count);
        }
        Assert.True(deleted);
    }

    private static Subscription CreateSubscription()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
        return subscription;
    }

    private sealed class FakeCurrentSubscriptionAccessor : ICurrentSubscriptionAccessor
    {
        private readonly Subscription subscription;

        public FakeCurrentSubscriptionAccessor(Subscription subscription)
        {
            this.subscription = subscription;
        }

        public Subscription GetCurrentSubscription()
        {
            return this.subscription;
        }
    }

    private sealed class FakeTaskRepository : ITaskRepository
    {
        private readonly Dictionary<Guid, DomainTask> store = [];

        public FakeTaskRepository(params DomainTask[] existing)
        {
            foreach (var task in existing)
            {
                this.store[task.Id] = task;
            }
        }

        public int AddCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public Task<List<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values
                .Where(task => task.ProjectId == projectId && !task.ParentTaskId.HasValue)
                .OrderBy(task => task.SortOrder)
                .ThenBy(task => task.CreatedAt)
                .ToList();
            return System.Threading.Tasks.Task.FromResult(result);
        }

        public Task<List<DomainTask>> GetSubTasksAsync(Guid parentTaskId, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values
                .Where(task => task.ParentTaskId == parentTaskId)
                .OrderBy(task => task.SortOrder)
                .ThenBy(task => task.CreatedAt)
                .ToList();
            return System.Threading.Tasks.Task.FromResult(result);
        }

        public Task<int> GetNextSortOrderAsync(Guid? projectId, Guid? parentTaskId, CancellationToken cancellationToken = default)
        {
            var maxSortOrder = this.store.Values
                .Where(task => task.ProjectId == projectId && task.ParentTaskId == parentTaskId)
                .Select(task => (int?)task.SortOrder)
                .DefaultIfEmpty(null)
                .Max();

            return System.Threading.Tasks.Task.FromResult(maxSortOrder.HasValue ? maxSortOrder.Value + 1 : 0);
        }

        public Task<List<DomainTask>> GetByPriorityAsync(TaskPriority priority, Guid projectId, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values.Where(task => task.ProjectId == projectId && task.Priority == priority).ToList();
            return System.Threading.Tasks.Task.FromResult(result);
        }

        public Task<List<DomainTask>> SearchAsync(string query, Guid projectId, CancellationToken cancellationToken = default)
        {
            var normalized = query.Trim().ToLowerInvariant();
            var result = this.store.Values
                .Where(task => task.ProjectId == projectId)
                .Where(task => task.Title.ToLowerInvariant().Contains(normalized) || (task.Note ?? string.Empty).ToLowerInvariant().Contains(normalized))
                .ToList();
            return System.Threading.Tasks.Task.FromResult(result);
        }

        public Task<List<DomainTask>> GetFocusedAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values.Where(task => task.ProjectId == projectId && task.IsFocused).ToList();
            return System.Threading.Tasks.Task.FromResult(result);
        }

        public Task<List<DomainTask>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.store.Values.ToList());
        }

        public Task<List<DomainTask>> GetRecentAsync(int days, CancellationToken cancellationToken = default)
        {
            var threshold = DateTime.UtcNow.AddDays(-Math.Abs(days));
            var result = this.store.Values.Where(task => task.CreatedAt >= threshold).OrderByDescending(task => task.CreatedAt).ToList();
            return System.Threading.Tasks.Task.FromResult(result);
        }

        public Task<List<DomainTask>> GetUnassignedRecentAsync(int days, CancellationToken cancellationToken = default)
        {
            var threshold = DateTime.UtcNow.AddDays(-Math.Abs(days));
            var result = this.store.Values.Where(task => !task.ProjectId.HasValue && task.CreatedAt >= threshold).OrderByDescending(task => task.CreatedAt).ToList();
            return System.Threading.Tasks.Task.FromResult(result);
        }

        public Task<List<DomainTask>> GetDueOnDateAsync(DateOnly localDate, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values.Where(task => task.HasDueDate && task.DueDateLocal == localDate).ToList();
            return System.Threading.Tasks.Task.FromResult(result);
        }

        public Task<List<DomainTask>> GetDueInRangeAsync(DateOnly localStartInclusive, DateOnly localEndInclusive, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values.Where(task => task.HasDueDate && task.DueDateLocal >= localStartInclusive && task.DueDateLocal <= localEndInclusive).ToList();
            return System.Threading.Tasks.Task.FromResult(result);
        }

        public Task<List<DomainTask>> GetDueAfterDateAsync(DateOnly localDateExclusive, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values.Where(task => task.HasDueDate && task.DueDateLocal > localDateExclusive).ToList();
            return System.Threading.Tasks.Task.FromResult(result);
        }

        public Task<List<DomainTask>> GetByIdsAsync(IEnumerable<Guid> taskIds, CancellationToken cancellationToken = default)
        {
            var ids = taskIds.ToHashSet();
            var result = this.store.Values.Where(task => ids.Contains(task.Id)).ToList();
            return System.Threading.Tasks.Task.FromResult(result);
        }

        public Task<DomainTask> AddAsync(DomainTask task, CancellationToken cancellationToken = default)
        {
            this.AddCallCount++;
            this.store[task.Id] = task;
            return System.Threading.Tasks.Task.FromResult(task);
        }

        public Task<DomainTask> UpdateAsync(DomainTask task, CancellationToken cancellationToken = default)
        {
            this.UpdateCallCount++;
            this.store[task.Id] = task;
            return System.Threading.Tasks.Task.FromResult(task);
        }

        public Task<List<DomainTask>> UpdateRangeAsync(IEnumerable<DomainTask> tasks, CancellationToken cancellationToken = default)
        {
            var updated = tasks.ToList();
            foreach (var task in updated)
            {
                this.store[task.Id] = task;
            }

            this.UpdateCallCount++;
            return System.Threading.Tasks.Task.FromResult(updated);
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.store.Remove(id));
        }

        public Task<DomainTask> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.store[id]);
        }
    }

    private sealed class FakeTaskHistoryRepository : ITaskHistoryRepository
    {
        private readonly List<string> suggestions;

        public FakeTaskHistoryRepository()
            : this([])
        {
        }

        public FakeTaskHistoryRepository(IEnumerable<string> suggestions)
        {
            this.suggestions = suggestions.ToList();
        }

        public string LastRegisteredName { get; private set; } = string.Empty;

        public bool LastRegisteredWasSubTask { get; private set; }

        public System.Threading.Tasks.Task RegisterUsageAsync(string name, bool isSubTaskName, CancellationToken cancellationToken = default)
        {
            this.LastRegisteredName = name;
            this.LastRegisteredWasSubTask = isSubTaskName;
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public Task<List<string>> GetSuggestionsAsync(string prefix, bool isSubTaskName, int take = 20, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.suggestions.Take(take).ToList());
        }
    }
}

