using Microsoft.Extensions.Logging.Abstractions;
using TaskFlow.Domain;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.UnitTests.Infrastructure;

public class ProjectRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetAllAsync_FiltersByCurrentSubscription()
    {
        var currentSubscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));

        await using (var db = await factory.CreateDbContextAsync())
        {
            db.Projects.Add(new Project(currentSubscriptionId, "Mine", "#123456", "work"));
            db.Projects.Add(new Project(Guid.NewGuid(), "Other", "#654321", "person"));
            await db.SaveChangesAsync();
        }

        var repository = new ProjectRepository(NullLogger<ProjectRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(currentSubscriptionId));

        var result = await repository.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("Mine", result[0].Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddUpdateDeleteAsync_PerformsCrudWithinSubscription()
    {
        var currentSubscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));
        var repository = new ProjectRepository(NullLogger<ProjectRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(currentSubscriptionId));

        var project = new Project(currentSubscriptionId, "Initial", "#123456", "work");
        await repository.AddAsync(project);

        project.UpdateName("Renamed");
        await repository.UpdateAsync(project);
        var fetched = await repository.GetByIdAsync(project.Id);

        var deleted = await repository.DeleteAsync(project.Id);

        Assert.Equal("Renamed", fetched.Name);
        Assert.True(deleted);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAsync_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        var currentSubscriptionId = Guid.NewGuid();
        var repository = new ProjectRepository(
            NullLogger<ProjectRepository>.Instance,
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(currentSubscriptionId));

        var foreign = new Project(Guid.NewGuid(), "Foreign", "#abcdef", "work");

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddAsync(foreign));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdAsync_NotFound_ThrowsEntityNotFoundException()
    {
        var currentSubscriptionId = Guid.NewGuid();
        var repository = new ProjectRepository(
            NullLogger<ProjectRepository>.Instance,
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(currentSubscriptionId));

        await Assert.ThrowsAsync<EntityNotFoundException>(() => repository.GetByIdAsync(Guid.NewGuid()));
    }
}
