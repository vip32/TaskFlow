using TaskFlow.Application;
using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;
using DomainTaskStatus = TaskFlow.Domain.TaskStatus;

namespace TaskFlow.UnitTests.Application;

public class TaskOrchestratorTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetByProjectIdAsync_ForwardsToRepository()
    {
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var existing = new DomainTask(subscription.Id, "A", projectId);

        var repository = new FakeTaskRepository(existing);
        var sut = CreateSut(subscription, repository, new FakeTaskHistoryRepository());

        var result = await sut.GetByProjectIdAsync(projectId);

        Assert.Single(result);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetNameSuggestionsAsync_ForwardsToHistoryRepository()
    {
        var subscription = CreateSubscription();
        var history = new FakeTaskHistoryRepository
        {
            Suggestions = ["Plan", "Planning"],
        };

        var sut = CreateSut(subscription, new FakeTaskRepository(), history);
        var result = await sut.GetNameSuggestionsAsync("Pl", false, 5);

        Assert.Equal(2, result.Count);
        Assert.Equal("Pl", history.LastPrefix);
        Assert.False(history.LastIsSubTaskName);
        Assert.Equal(5, history.LastTake);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ValidInput_PersistsTaskAndRegistersHistory()
    {
        var subscription = CreateSubscription();
        var history = new FakeTaskHistoryRepository();
        var repository = new FakeTaskRepository();
        var sut = CreateSut(subscription, repository, history);

        var created = await sut.CreateAsync(Guid.NewGuid(), "Draft roadmap", TaskPriority.High, "Initial note");

        Assert.Equal(subscription.Id, created.SubscriptionId);
        Assert.Equal(TaskPriority.High, created.Priority);
        Assert.Equal(1, repository.AddCallCount);
        Assert.Single(history.Registered);
        Assert.False(history.Registered[0].IsSubTaskName);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateUnassignedAsync_ValidInput_PersistsUnassignedTask()
    {
        var subscription = CreateSubscription();
        var repository = new FakeTaskRepository();
        var sut = CreateSut(subscription, repository, new FakeTaskHistoryRepository());

        var created = await sut.CreateUnassignedAsync("Inbox idea", TaskPriority.Medium, string.Empty);

        Assert.Null(created.ProjectId);
        Assert.True(created.IsUnassigned);
        Assert.Equal(1, repository.AddCallCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateSubTaskAsync_ValidInput_InheritsParentProjectAndRegistersSubtaskHistory()
    {
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var parent = new DomainTask(subscription.Id, "Parent", projectId);
        var repository = new FakeTaskRepository(parent);
        var history = new FakeTaskHistoryRepository();
        var sut = CreateSut(subscription, repository, history);

        var created = await sut.CreateSubTaskAsync(parent.Id, "Child", TaskPriority.Low, "n");

        Assert.Equal(parent.Id, created.ParentTaskId);
        Assert.Equal(projectId, created.ProjectId);
        Assert.True(history.Registered[0].IsSubTaskName);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTitleAsync_ExistingTask_PersistsChangeAndRegistersHistory()
    {
        var subscription = CreateSubscription();
        var existing = new DomainTask(subscription.Id, "Old", Guid.NewGuid());
        var history = new FakeTaskHistoryRepository();
        var repository = new FakeTaskRepository(existing);
        var sut = CreateSut(subscription, repository, history);

        var updated = await sut.UpdateTitleAsync(existing.Id, "New");

        Assert.Equal("New", updated.Title);
        Assert.Equal(1, repository.UpdateCallCount);
        Assert.Single(history.Registered);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateNoteSetPrioritySetStatus_ToggleFlags_PersistChanges()
    {
        var subscription = CreateSubscription();
        var existing = new DomainTask(subscription.Id, "Task", Guid.NewGuid());
        var repository = new FakeTaskRepository(existing);
        var sut = CreateSut(subscription, repository, new FakeTaskHistoryRepository());

        await sut.UpdateNoteAsync(existing.Id, "note");
        await sut.SetPriorityAsync(existing.Id, TaskPriority.Low);
        await sut.SetStatusAsync(existing.Id, DomainTaskStatus.Doing);
        await sut.ToggleFocusAsync(existing.Id);

        var important = await sut.ToggleImportantAsync(existing.Id);

        Assert.Equal("note", important.Note);
        Assert.Equal(TaskPriority.Low, important.Priority);
        Assert.Equal(DomainTaskStatus.Doing, important.Status);
        Assert.True(important.IsFocused);
        Assert.True(important.IsImportant);
    }

    [Fact]
    public async System.Threading.Tasks.Task MoveToProjectAsync_SubTask_ThrowsInvalidOperationException()
    {
        var subscription = CreateSubscription();
        var parent = new DomainTask(subscription.Id, "Parent", Guid.NewGuid());
        var child = new DomainTask(subscription.Id, "Child", parent.ProjectId);
        parent.AddSubTask(child);

        var repository = new FakeTaskRepository(parent, child);
        var sut = CreateSut(subscription, repository, new FakeTaskHistoryRepository());

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.MoveToProjectAsync(child.Id, Guid.NewGuid()));
    }

    [Fact]
    public async System.Threading.Tasks.Task MoveToProjectAsync_TopLevelTask_AssignsProjectAndSortOrder()
    {
        var subscription = CreateSubscription();
        var existingInTarget = new DomainTask(subscription.Id, "Existing", Guid.NewGuid());
        existingInTarget.SetSortOrder(3);
        var moving = new DomainTask(subscription.Id, "Move", Guid.NewGuid());

        var repository = new FakeTaskRepository(existingInTarget, moving);
        var sut = CreateSut(subscription, repository, new FakeTaskHistoryRepository());

        var moved = await sut.MoveToProjectAsync(moving.Id, existingInTarget.ProjectId!.Value);

        Assert.Equal(existingInTarget.ProjectId, moved.ProjectId);
        Assert.Equal(4, moved.SortOrder);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReorderProjectTasksAsync_ValidOrder_PersistsSortOrder()
    {
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();

        var first = new DomainTask(subscription.Id, "First", projectId);
        var second = new DomainTask(subscription.Id, "Second", projectId);
        first.SetSortOrder(0);
        second.SetSortOrder(1);

        var repository = new FakeTaskRepository(first, second);
        var sut = CreateSut(subscription, repository, new FakeTaskHistoryRepository());

        var reordered = await sut.ReorderProjectTasksAsync(projectId, [second.Id, first.Id]);

        Assert.Equal(second.Id, reordered[0].Id);
        Assert.Equal(0, reordered[0].SortOrder);
        Assert.Equal(1, reordered[1].SortOrder);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReorderProjectTasksAsync_DuplicateId_ThrowsArgumentException()
    {
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var first = new DomainTask(subscription.Id, "First", projectId);

        var sut = CreateSut(subscription, new FakeTaskRepository(first), new FakeTaskHistoryRepository());

        await Assert.ThrowsAsync<ArgumentException>(() => sut.ReorderProjectTasksAsync(projectId, [first.Id, first.Id]));
    }

    [Fact]
    public async System.Threading.Tasks.Task ReorderProjectTasksAsync_UnknownId_ThrowsArgumentException()
    {
        var subscription = CreateSubscription();
        var projectId = Guid.NewGuid();
        var first = new DomainTask(subscription.Id, "First", projectId);

        var sut = CreateSut(subscription, new FakeTaskRepository(first), new FakeTaskHistoryRepository());

        await Assert.ThrowsAsync<ArgumentException>(() => sut.ReorderProjectTasksAsync(projectId, [Guid.NewGuid()]));
    }

    [Fact]
    public async System.Threading.Tasks.Task UnassignFromProjectAsync_SubTask_ThrowsInvalidOperationException()
    {
        var subscription = CreateSubscription();
        var parent = new DomainTask(subscription.Id, "Parent", Guid.NewGuid());
        var child = new DomainTask(subscription.Id, "Child", parent.ProjectId);
        parent.AddSubTask(child);

        var sut = CreateSut(subscription, new FakeTaskRepository(parent, child), new FakeTaskHistoryRepository());

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UnassignFromProjectAsync(child.Id));
    }

    [Fact]
    public async System.Threading.Tasks.Task UnassignFromProjectAsync_TopLevelTask_ClearsProject()
    {
        var subscription = CreateSubscription();
        var task = new DomainTask(subscription.Id, "Task", Guid.NewGuid());
        var sut = CreateSut(subscription, new FakeTaskRepository(task), new FakeTaskHistoryRepository());

        var updated = await sut.UnassignFromProjectAsync(task.Id);

        Assert.Null(updated.ProjectId);
    }

    [Fact]
    public async System.Threading.Tasks.Task DueDateAndReminderFlows_PersistChanges()
    {
        var subscription = CreateSubscription();
        var task = new DomainTask(subscription.Id, "Task", Guid.NewGuid());
        var sut = CreateSut(subscription, new FakeTaskRepository(task), new FakeTaskHistoryRepository());

        await sut.SetDueDateAsync(task.Id, new DateOnly(2026, 2, 11));
        await sut.SetDueDateTimeAsync(task.Id, new DateOnly(2026, 2, 11), new TimeOnly(10, 0));

        var withRelativeReminder = await sut.AddRelativeReminderAsync(task.Id, 10);
        var reminderId = withRelativeReminder.Reminders.Single().Id;
        await sut.RemoveReminderAsync(task.Id, reminderId);

        await sut.AddDateOnlyReminderAsync(task.Id, new TimeOnly(8, 30));
        await sut.ClearDueDateAsync(task.Id);
        var toggled = await sut.ToggleTodayMarkAsync(task.Id);

        Assert.True(toggled.IsMarkedForToday);
    }

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

        var sut = CreateSut(subscription, new FakeTaskRepository(dueToday, markedToday), new FakeTaskHistoryRepository());

        var tasks = await sut.GetTodayAsync();

        Assert.Equal(2, tasks.Count);
        Assert.Contains(tasks, task => task.Id == dueToday.Id);
        Assert.Contains(tasks, task => task.Id == markedToday.Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task RecentWeekUpcomingAndDelete_FlowThroughRepository()
    {
        var subscription = CreateSubscription();
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(subscription.TimeZoneId);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        var projectId = Guid.NewGuid();

        var recent = new DomainTask(subscription.Id, "Recent", projectId);
        var thisWeek = new DomainTask(subscription.Id, "ThisWeek", projectId);
        thisWeek.SetDueDate(today.AddDays(1));
        var upcoming = new DomainTask(subscription.Id, "Upcoming", projectId);
        upcoming.SetDueDate(today.AddDays(14));

        var repository = new FakeTaskRepository(recent, thisWeek, upcoming);
        var sut = CreateSut(subscription, repository, new FakeTaskHistoryRepository());

        var recents = await sut.GetRecentAsync();
        var week = await sut.GetThisWeekAsync();
        var up = await sut.GetUpcomingAsync();
        var deleted = await sut.DeleteAsync(recent.Id);

        Assert.NotEmpty(recents);
        Assert.NotNull(week);
        Assert.NotEmpty(up);
        Assert.True(deleted);
    }

    private static TaskOrchestrator CreateSut(Subscription subscription, FakeTaskRepository repository, FakeTaskHistoryRepository history)
    {
        return new TaskOrchestrator(repository, history, new FakeCurrentSubscriptionAccessor(subscription));
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
        public List<(string Name, bool IsSubTaskName)> Registered { get; } = [];

        public List<string> Suggestions { get; set; } = [];

        public string LastPrefix { get; private set; } = string.Empty;

        public bool LastIsSubTaskName { get; private set; }

        public int LastTake { get; private set; }

        public System.Threading.Tasks.Task RegisterUsageAsync(string name, bool isSubTaskName, CancellationToken cancellationToken = default)
        {
            this.Registered.Add((name, isSubTaskName));
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public Task<List<string>> GetSuggestionsAsync(string prefix, bool isSubTaskName, int take = 20, CancellationToken cancellationToken = default)
        {
            this.LastPrefix = prefix;
            this.LastIsSubTaskName = isSubTaskName;
            this.LastTake = take;
            return System.Threading.Tasks.Task.FromResult(this.Suggestions.Take(take).ToList());
        }
    }
}
