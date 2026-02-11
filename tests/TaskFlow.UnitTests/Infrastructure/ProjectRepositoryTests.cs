using TaskFlow.Domain;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.UnitTests.Infrastructure;

public class ProjectRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task AddAsync_ValidSubscription_PersistsProject()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(AddAsync_ValidSubscription_PersistsProject));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new ProjectRepository(factory, accessor);
        var project = new Project(subscriptionId, "Work", "#123456", "work");

        var created = await repository.AddAsync(project);
        var loaded = await repository.GetByIdAsync(created.Id);

        Assert.Equal("Work", loaded.Name);
        Assert.Equal(subscriptionId, loaded.SubscriptionId);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAsync_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        var factory = new InMemoryAppDbContextFactory(nameof(AddAsync_MismatchedSubscription_ThrowsInvalidOperationException));
        var accessor = new TestCurrentSubscriptionAccessor(Guid.NewGuid());
        var repository = new ProjectRepository(factory, accessor);
        var project = new Project(Guid.NewGuid(), "Work", "#123456", "work");

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddAsync(project));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllAsync_FiltersToCurrentSubscription()
    {
        var currentSubscriptionId = Guid.NewGuid();
        var otherSubscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(GetAllAsync_FiltersToCurrentSubscription));
        var accessor = new TestCurrentSubscriptionAccessor(currentSubscriptionId);
        var repository = new ProjectRepository(factory, accessor);

        await using (var db = factory.CreateDbContext())
        {
            db.Projects.Add(new Project(currentSubscriptionId, "Current", "#123456", "work"));
            db.Projects.Add(new Project(otherSubscriptionId, "Other", "#654321", "home"));
            await db.SaveChangesAsync();
        }

        var projects = await repository.GetAllAsync();

        Assert.Single(projects);
        Assert.Equal("Current", projects[0].Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateAsync_ExistingProject_PersistsChanges()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(UpdateAsync_ExistingProject_PersistsChanges));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new ProjectRepository(factory, accessor);
        var project = await repository.AddAsync(new Project(subscriptionId, "Old", "#123456", "work"));
        project.UpdateName("New");
        project.UpdateColor("#999999");

        await repository.UpdateAsync(project);
        var loaded = await repository.GetByIdAsync(project.Id);

        Assert.Equal("New", loaded.Name);
        Assert.Equal("#999999", loaded.Color);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_ExistingAndMissingProject_ReturnsExpectedValues()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(DeleteAsync_ExistingAndMissingProject_ReturnsExpectedValues));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new ProjectRepository(factory, accessor);
        var project = await repository.AddAsync(new Project(subscriptionId, "Work", "#123456", "work"));

        var deleted = await repository.DeleteAsync(project.Id);
        var deletedAgain = await repository.DeleteAsync(project.Id);

        Assert.True(deleted);
        Assert.False(deletedAgain);
    }
}

