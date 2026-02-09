using TaskFlow.Application;
using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.UnitTests;

/// <summary>
/// Tests task orchestration behavior.
/// </summary>
public class TaskOrchestratorTests
{
    /// <summary>
    /// Verifies create task uses current subscription and persists immediately.
    /// </summary>
    [Fact]
    public async global::System.Threading.Tasks.Task CreateAsync_ValidInput_PersistsTask()
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
    public async global::System.Threading.Tasks.Task CreateUnassignedAsync_ValidInput_PersistsUnassignedTask()
    {
        var subscription = CreateSubscription();
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeTaskRepository();
        var historyRepository = new FakeTaskHistoryRepository();
        var orchestrator = new TaskOrchestrator(repository, historyRepository, accessor);

        var created = await orchestrator.CreateUnassignedAsync("Inbox idea", TaskPriority.Medium, string.Empty);

        Assert.Equal(Guid.Empty, created.ProjectId);
        Assert.True(created.IsUnassigned);
        Assert.Equal(1, repository.AddCallCount);
    }

    /// <summary>
    /// Verifies title updates persist immediately.
    /// </summary>
    [Fact]
    public async global::System.Threading.Tasks.Task UpdateTitleAsync_ExistingTask_PersistsChange()
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
    public async global::System.Threading.Tasks.Task GetTodayAsync_DueAndMarkedTasks_ReturnsMergedUniqueList()
    {
        var subscription = CreateSubscription();
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(subscription.TimeZoneId);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));

        var dueToday = new DomainTask(subscription.Id, "Due today", Guid.NewGuid());
        dueToday.SetDueDate(today);

        var markedToday = new DomainTask(subscription.Id, "Marked today", Guid.Empty);
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

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values.Where(task => task.ProjectId == projectId).ToList();
            return global::System.Threading.Tasks.Task.FromResult(result);
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetByPriorityAsync(TaskPriority priority, Guid projectId, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values.Where(task => task.ProjectId == projectId && task.Priority == priority).ToList();
            return global::System.Threading.Tasks.Task.FromResult(result);
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> SearchAsync(string query, Guid projectId, CancellationToken cancellationToken = default)
        {
            var normalized = query.Trim().ToLowerInvariant();
            var result = this.store.Values
                .Where(task => task.ProjectId == projectId)
                .Where(task => task.Title.ToLowerInvariant().Contains(normalized) || task.Note.ToLowerInvariant().Contains(normalized))
                .ToList();
            return global::System.Threading.Tasks.Task.FromResult(result);
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetFocusedAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values.Where(task => task.ProjectId == projectId && task.IsFocused).ToList();
            return global::System.Threading.Tasks.Task.FromResult(result);
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.store.Values.ToList());
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetRecentAsync(int days, CancellationToken cancellationToken = default)
        {
            var threshold = DateTime.UtcNow.AddDays(-Math.Abs(days));
            var result = this.store.Values.Where(task => task.CreatedAt >= threshold).OrderByDescending(task => task.CreatedAt).ToList();
            return global::System.Threading.Tasks.Task.FromResult(result);
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetUnassignedRecentAsync(int days, CancellationToken cancellationToken = default)
        {
            var threshold = DateTime.UtcNow.AddDays(-Math.Abs(days));
            var result = this.store.Values.Where(task => task.ProjectId == Guid.Empty && task.CreatedAt >= threshold).OrderByDescending(task => task.CreatedAt).ToList();
            return global::System.Threading.Tasks.Task.FromResult(result);
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetDueOnDateAsync(DateOnly localDate, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values.Where(task => task.HasDueDate && task.DueDateLocal == localDate).ToList();
            return global::System.Threading.Tasks.Task.FromResult(result);
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetDueInRangeAsync(DateOnly localStartInclusive, DateOnly localEndInclusive, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values.Where(task => task.HasDueDate && task.DueDateLocal >= localStartInclusive && task.DueDateLocal <= localEndInclusive).ToList();
            return global::System.Threading.Tasks.Task.FromResult(result);
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetDueAfterDateAsync(DateOnly localDateExclusive, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values.Where(task => task.HasDueDate && task.DueDateLocal > localDateExclusive).ToList();
            return global::System.Threading.Tasks.Task.FromResult(result);
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetByIdsAsync(IEnumerable<Guid> taskIds, CancellationToken cancellationToken = default)
        {
            var ids = taskIds.ToHashSet();
            var result = this.store.Values.Where(task => ids.Contains(task.Id)).ToList();
            return global::System.Threading.Tasks.Task.FromResult(result);
        }

        public global::System.Threading.Tasks.Task<DomainTask> AddAsync(DomainTask task, CancellationToken cancellationToken = default)
        {
            this.AddCallCount++;
            this.store[task.Id] = task;
            return global::System.Threading.Tasks.Task.FromResult(task);
        }

        public global::System.Threading.Tasks.Task<DomainTask> UpdateAsync(DomainTask task, CancellationToken cancellationToken = default)
        {
            this.UpdateCallCount++;
            this.store[task.Id] = task;
            return global::System.Threading.Tasks.Task.FromResult(task);
        }

        public global::System.Threading.Tasks.Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.store.Remove(id));
        }

        public global::System.Threading.Tasks.Task<DomainTask> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.store[id]);
        }
    }

    private sealed class FakeTaskHistoryRepository : ITaskHistoryRepository
    {
        public global::System.Threading.Tasks.Task RegisterUsageAsync(string name, bool isSubTaskName, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.CompletedTask;
        }

        public global::System.Threading.Tasks.Task<List<string>> GetSuggestionsAsync(string prefix, bool isSubTaskName, int take = 20, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(new List<string>());
        }
    }
}
