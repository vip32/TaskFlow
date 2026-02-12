using Microsoft.Extensions.Logging.Abstractions;
using TaskFlow.Domain;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.UnitTests.Infrastructure;

[Trait("Layer", "Infrastructure")]
[Trait("Slice", "Projects")]
[Trait("Type", "Unit")]
public class ProjectRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetAllAsync_FiltersByCurrentSubscription()
    {
        // Arrange
        var currentSubscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));

        await using (var db = await factory.CreateDbContextAsync())
        {
            db.Projects.Add(new Project(currentSubscriptionId, "Mine", "#123456", "work"));
            db.Projects.Add(new Project(Guid.NewGuid(), "Other", "#654321", "person"));
            await db.SaveChangesAsync();
        }

        // Act
        var sut = new ProjectRepository(NullLogger<ProjectRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(currentSubscriptionId));

        var result = await sut.GetAllAsync();

        // Assert
        result.ShouldHaveSingleItem();
        result[0].Name.ShouldBe("Mine");
    }

    [Fact]
    public async System.Threading.Tasks.Task AddUpdateDeleteAsync_PerformsCrudWithinSubscription()
    {
        // Arrange
        var currentSubscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));

        // Act
        var sut = new ProjectRepository(NullLogger<ProjectRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(currentSubscriptionId));

        var project = new Project(currentSubscriptionId, "Initial", "#123456", "work");
        await sut.AddAsync(project);

        project.UpdateName("Renamed");
        await sut.UpdateAsync(project);
        var fetched = await sut.GetByIdAsync(project.Id);

        var deleted = await sut.DeleteAsync(project.Id);

        // Assert
        fetched.Name.ShouldBe("Renamed");
        deleted.ShouldBeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAsync_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        // Arrange
        var currentSubscriptionId = Guid.NewGuid();

        // Act
        var sut = new ProjectRepository(
            NullLogger<ProjectRepository>.Instance,
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(currentSubscriptionId));

        var foreign = new Project(Guid.NewGuid(), "Foreign", "#abcdef", "work");

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(() => sut.AddAsync(foreign));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdAsync_NotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var currentSubscriptionId = Guid.NewGuid();

        // Act
        var sut = new ProjectRepository(
            NullLogger<ProjectRepository>.Instance,
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(currentSubscriptionId));

        // Assert
        await Should.ThrowAsync<EntityNotFoundException>(() => sut.GetByIdAsync(Guid.NewGuid()));
    }
}


