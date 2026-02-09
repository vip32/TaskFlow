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
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true);
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));

        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeTaskRepository();
        var orchestrator = new TaskOrchestrator(repository, accessor);

        var created = await orchestrator.CreateAsync(Guid.NewGuid(), "Draft roadmap", TaskPriority.High, "Initial note");

        Assert.Equal(subscription.Id, created.SubscriptionId);
        Assert.Equal(TaskPriority.High, created.Priority);
        Assert.Equal(1, repository.AddCallCount);
    }

    /// <summary>
    /// Verifies title updates persist immediately.
    /// </summary>
    [Fact]
    public async global::System.Threading.Tasks.Task UpdateTitleAsync_ExistingTask_PersistsChange()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true);
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));

        var existing = new DomainTask(subscription.Id, "Old", Guid.NewGuid());
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeTaskRepository(existing);
        var orchestrator = new TaskOrchestrator(repository, accessor);

        var updated = await orchestrator.UpdateTitleAsync(existing.Id, "New");

        Assert.Equal("New", updated.Title);
        Assert.Equal(1, repository.UpdateCallCount);
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

        public FakeTaskRepository()
        {
        }

        public FakeTaskRepository(DomainTask existing)
        {
            this.store[existing.Id] = existing;
        }

        public int AddCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values.Where(x => x.ProjectId == projectId).ToList();
            return global::System.Threading.Tasks.Task.FromResult(result);
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetByPriorityAsync(TaskPriority priority, Guid projectId, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values.Where(x => x.ProjectId == projectId && x.Priority == priority).ToList();
            return global::System.Threading.Tasks.Task.FromResult(result);
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> SearchAsync(string query, Guid projectId, CancellationToken cancellationToken = default)
        {
            var normalized = query.Trim().ToLowerInvariant();
            var result = this.store.Values
                .Where(x => x.ProjectId == projectId)
                .Where(x => x.Title.ToLowerInvariant().Contains(normalized) || x.Note.ToLowerInvariant().Contains(normalized))
                .ToList();
            return global::System.Threading.Tasks.Task.FromResult(result);
        }

        public global::System.Threading.Tasks.Task<List<DomainTask>> GetFocusedAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var result = this.store.Values.Where(x => x.ProjectId == projectId && x.IsFocused).ToList();
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
}
