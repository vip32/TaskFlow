using TaskFlow.Application;
using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Application;

/// <summary>
/// Tests project orchestration behavior.
/// </summary>
public class ProjectOrchestratorTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetAllAsync_ReturnsRepositoryProjects()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true);
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));

        var projectA = new Project(subscription.Id, "A", "#123456", "work");
        var projectB = new Project(subscription.Id, "B", "#654321", "home");
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeProjectRepository(projectA, projectB);
        var orchestrator = new ProjectOrchestrator(repository, accessor);

        var projects = await orchestrator.GetAllAsync();

        Assert.Equal(2, projects.Count);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdAsync_ExistingProject_ReturnsProject()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true);
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));

        var project = new Project(subscription.Id, "A", "#123456", "work");
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeProjectRepository(project);
        var orchestrator = new ProjectOrchestrator(repository, accessor);

        var found = await orchestrator.GetByIdAsync(project.Id);

        Assert.Equal(project.Id, found.Id);
    }

    /// <summary>
    /// Verifies create uses current subscription and persists through repository.
    /// </summary>
    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ValidInput_PersistsProjectWithCurrentSubscription()
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
    public async System.Threading.Tasks.Task UpdateViewTypeAsync_ExistingProject_PersistsChange()
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

    [Fact]
    public async System.Threading.Tasks.Task UpdateNameAsync_ExistingProject_PersistsChange()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true);
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
        var project = new Project(subscription.Id, "Old", "#123456", "work");
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeProjectRepository(project);
        var orchestrator = new ProjectOrchestrator(repository, accessor);

        var updated = await orchestrator.UpdateNameAsync(project.Id, "New");

        Assert.Equal("New", updated.Name);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateVisualsAsync_ExistingProject_PersistsChanges()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true);
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
        var project = new Project(subscription.Id, "Work", "#123456", "work");
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeProjectRepository(project);
        var orchestrator = new ProjectOrchestrator(repository, accessor);

        var updated = await orchestrator.UpdateVisualsAsync(project.Id, "#654321", "home");

        Assert.Equal("#654321", updated.Color);
        Assert.Equal("home", updated.Icon);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_ExistingProject_ReturnsTrue()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true);
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
        var project = new Project(subscription.Id, "Work", "#123456", "work");
        var accessor = new FakeCurrentSubscriptionAccessor(subscription);
        var repository = new FakeProjectRepository(project);
        var orchestrator = new ProjectOrchestrator(repository, accessor);

        var deleted = await orchestrator.DeleteAsync(project.Id);

        Assert.True(deleted);
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

        public FakeProjectRepository(params Project[] existing)
        {
            foreach (var project in existing)
            {
                this.store[project.Id] = project;
            }
        }

        public int AddCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.store.Values.ToList());
        }

        public Task<Project> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.store[id]);
        }

        public Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default)
        {
            this.AddCallCount++;
            this.store[project.Id] = project;
            return System.Threading.Tasks.Task.FromResult(project);
        }

        public Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default)
        {
            this.UpdateCallCount++;
            this.store[project.Id] = project;
            return System.Threading.Tasks.Task.FromResult(project);
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.store.Remove(id));
        }
    }
}

