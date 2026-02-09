using TaskFlow.Application;
using TaskFlow.Domain;

namespace TaskFlow.UnitTests;

/// <summary>
/// Tests project orchestration behavior.
/// </summary>
public class ProjectOrchestratorTests
{
    /// <summary>
    /// Verifies create uses current subscription and persists through repository.
    /// </summary>
    [Fact]
    public async global::System.Threading.Tasks.Task CreateAsync_ValidInput_PersistsProjectWithCurrentSubscription()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true);
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));

        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeProjectRepository();
        var orchestrator = new ProjectOrchestrator(repository, accessor);

        var created = await orchestrator.CreateAsync("Work", "#123456", "work", false);

        Assert.Equal(subscription.Id, created.SubscriptionId);
        Assert.Equal(1, repository.AddCallCount);
    }

    /// <summary>
    /// Verifies view type updates are immediately persisted.
    /// </summary>
    [Fact]
    public async global::System.Threading.Tasks.Task UpdateViewTypeAsync_ExistingProject_PersistsChange()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true);
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));

        var project = new Project(subscription.Id, "Work", "#123456", "work");
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeProjectRepository(project);
        var orchestrator = new ProjectOrchestrator(repository, accessor);

        var updated = await orchestrator.UpdateViewTypeAsync(project.Id, ProjectViewType.Board);

        Assert.Equal(ProjectViewType.Board, updated.ViewType);
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

    private sealed class FakeProjectRepository : IProjectRepository
    {
        private readonly Dictionary<Guid, Project> store = [];

        public FakeProjectRepository()
        {
        }

        public FakeProjectRepository(Project existing)
        {
            this.store[existing.Id] = existing;
        }

        public int AddCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public global::System.Threading.Tasks.Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.store.Values.ToList());
        }

        public global::System.Threading.Tasks.Task<Project> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.store[id]);
        }

        public global::System.Threading.Tasks.Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default)
        {
            this.AddCallCount++;
            this.store[project.Id] = project;
            return global::System.Threading.Tasks.Task.FromResult(project);
        }

        public global::System.Threading.Tasks.Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default)
        {
            this.UpdateCallCount++;
            this.store[project.Id] = project;
            return global::System.Threading.Tasks.Task.FromResult(project);
        }

        public global::System.Threading.Tasks.Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return global::System.Threading.Tasks.Task.FromResult(this.store.Remove(id));
        }
    }
}
