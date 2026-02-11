using TaskFlow.Application;
using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Application;

public class ProjectOrchestratorTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetAllAsync_ForwardsToRepository()
    {
        var subscription = CreateSubscription();
        var first = new Project(subscription.Id, "One", "#111111", "work");
        var second = new Project(subscription.Id, "Two", "#222222", "person");

        var repository = new FakeProjectRepository(first, second);
        var sut = new ProjectOrchestrator(repository, new FakeCurrentSubscriptionAccessor(subscription));

        var result = await sut.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ValidInput_PersistsProjectWithCurrentSubscription()
    {
        var subscription = CreateSubscription();
        var repository = new FakeProjectRepository();
        var sut = new ProjectOrchestrator(repository, new FakeCurrentSubscriptionAccessor(subscription));

        var created = await sut.CreateAsync("Work", "#123456", "work", false);

        Assert.Equal(subscription.Id, created.SubscriptionId);
        Assert.Equal(1, repository.AddCallCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateNameAsync_ExistingProject_PersistsChange()
    {
        var subscription = CreateSubscription();
        var existing = new Project(subscription.Id, "Old", "#123456", "work");
        var repository = new FakeProjectRepository(existing);
        var sut = new ProjectOrchestrator(repository, new FakeCurrentSubscriptionAccessor(subscription));

        var updated = await sut.UpdateNameAsync(existing.Id, "New");

        Assert.Equal("New", updated.Name);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateVisualsAsync_ExistingProject_PersistsChange()
    {
        var subscription = CreateSubscription();
        var existing = new Project(subscription.Id, "Work", "#123456", "work");
        var repository = new FakeProjectRepository(existing);
        var sut = new ProjectOrchestrator(repository, new FakeCurrentSubscriptionAccessor(subscription));

        var updated = await sut.UpdateVisualsAsync(existing.Id, "#abcdef", "star");

        Assert.Equal("#abcdef", updated.Color);
        Assert.Equal("star", updated.Icon);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateViewTypeAsync_ExistingProject_PersistsChange()
    {
        var subscription = CreateSubscription();
        var existing = new Project(subscription.Id, "Work", "#123456", "work");
        var repository = new FakeProjectRepository(existing);
        var sut = new ProjectOrchestrator(repository, new FakeCurrentSubscriptionAccessor(subscription));

        var updated = await sut.UpdateViewTypeAsync(existing.Id, ProjectViewType.Board);

        Assert.Equal(ProjectViewType.Board, updated.ViewType);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_ForwardsToRepository()
    {
        var subscription = CreateSubscription();
        var existing = new Project(subscription.Id, "Work", "#123456", "work");
        var repository = new FakeProjectRepository(existing);
        var sut = new ProjectOrchestrator(repository, new FakeCurrentSubscriptionAccessor(subscription));

        var deleted = await sut.DeleteAsync(existing.Id);

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

    private sealed class FakeProjectRepository : IProjectRepository
    {
        private readonly Dictionary<Guid, Project> store = [];

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
