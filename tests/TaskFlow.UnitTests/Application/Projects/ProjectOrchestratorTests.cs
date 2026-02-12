using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using TaskFlow.Application;
using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Application;

public class ProjectOrchestratorTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetAllAsync_ForwardsToRepository()
    {
        // Arrange
        var subscription = CreateSubscription();
        var first = new Project(subscription.Id, "One", "#111111", "work");
        var second = new Project(subscription.Id, "Two", "#222222", "person");
        var repository = Substitute.For<IProjectRepository>();
        repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([first, second]);

        var sut = new ProjectOrchestrator(
            Substitute.For<ILogger<ProjectOrchestrator>>(),
            repository,
            CreateAccessor(subscription));

        // Act
        var result = await sut.GetAllAsync();

        // Assert
        result.Count.ShouldBe(2);
        await repository.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdAsync_ForwardsToRepository()
    {
        // Arrange
        var subscription = CreateSubscription();
        var existing = new Project(subscription.Id, "One", "#111111", "work");
        var repository = Substitute.For<IProjectRepository>();
        repository.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>()).Returns(existing);

        var sut = new ProjectOrchestrator(
            Substitute.For<ILogger<ProjectOrchestrator>>(),
            repository,
            CreateAccessor(subscription));

        // Act
        var result = await sut.GetByIdAsync(existing.Id);

        // Assert
        result.Id.ShouldBe(existing.Id);
        await repository.Received(1).GetByIdAsync(existing.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ValidInput_PersistsProjectWithCurrentSubscription()
    {
        // Arrange
        var subscription = CreateSubscription();
        var repository = Substitute.For<IProjectRepository>();
        repository.AddAsync(Arg.Any<Project>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<Project>());

        var sut = new ProjectOrchestrator(
            Substitute.For<ILogger<ProjectOrchestrator>>(),
            repository,
            CreateAccessor(subscription));

        // Act
        var created = await sut.CreateAsync("Work", "#123456", "work", false);

        // Assert
        created.SubscriptionId.ShouldBe(subscription.Id);
        await repository.Received(1).AddAsync(Arg.Is<Project>(p => p.SubscriptionId == subscription.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateNameAsync_ExistingProject_PersistsChange()
    {
        // Arrange
        var subscription = CreateSubscription();
        var existing = new Project(subscription.Id, "Old", "#123456", "work");
        var repository = Substitute.For<IProjectRepository>();
        repository.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>()).Returns(existing);
        repository.UpdateAsync(Arg.Any<Project>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<Project>());

        var sut = new ProjectOrchestrator(
            Substitute.For<ILogger<ProjectOrchestrator>>(),
            repository,
            CreateAccessor(subscription));

        // Act
        var updated = await sut.UpdateNameAsync(existing.Id, "New");

        // Assert
        updated.Name.ShouldBe("New");
        await repository.Received(1).UpdateAsync(Arg.Is<Project>(p => p.Name == "New"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateVisualsAsync_ExistingProject_PersistsChange()
    {
        // Arrange
        var subscription = CreateSubscription();
        var existing = new Project(subscription.Id, "Work", "#123456", "work");
        var repository = Substitute.For<IProjectRepository>();
        repository.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>()).Returns(existing);
        repository.UpdateAsync(Arg.Any<Project>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<Project>());

        var sut = new ProjectOrchestrator(
            Substitute.For<ILogger<ProjectOrchestrator>>(),
            repository,
            CreateAccessor(subscription));

        // Act
        var updated = await sut.UpdateVisualsAsync(existing.Id, "#abcdef", "star");

        // Assert
        updated.Color.ShouldBe("#abcdef");
        updated.Icon.ShouldBe("star");
        await repository.Received(1).UpdateAsync(Arg.Is<Project>(p => p.Color == "#abcdef" && p.Icon == "star"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateViewTypeAsync_ExistingProject_PersistsChange()
    {
        // Arrange
        var subscription = CreateSubscription();
        var existing = new Project(subscription.Id, "Work", "#123456", "work");
        var repository = Substitute.For<IProjectRepository>();
        repository.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>()).Returns(existing);
        repository.UpdateAsync(Arg.Any<Project>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<Project>());

        var sut = new ProjectOrchestrator(
            Substitute.For<ILogger<ProjectOrchestrator>>(),
            repository,
            CreateAccessor(subscription));

        // Act
        var updated = await sut.UpdateViewTypeAsync(existing.Id, ProjectViewType.Board);

        // Assert
        updated.ViewType.ShouldBe(ProjectViewType.Board);
        await repository.Received(1).UpdateAsync(Arg.Is<Project>(p => p.ViewType == ProjectViewType.Board), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_ForwardsToRepository()
    {
        // Arrange
        var subscription = CreateSubscription();
        var existing = new Project(subscription.Id, "Work", "#123456", "work");
        var repository = Substitute.For<IProjectRepository>();
        repository.DeleteAsync(existing.Id, Arg.Any<CancellationToken>()).Returns(true);

        var sut = new ProjectOrchestrator(
            Substitute.For<ILogger<ProjectOrchestrator>>(),
            repository,
            CreateAccessor(subscription));

        // Act
        var deleted = await sut.DeleteAsync(existing.Id);

        // Assert
        deleted.ShouldBeTrue();
        await repository.Received(1).DeleteAsync(existing.Id, Arg.Any<CancellationToken>());
    }

    private static Subscription CreateSubscription()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
        return subscription;
    }

    private static ICurrentSubscriptionAccessor CreateAccessor(Subscription subscription)
    {
        var accessor = Substitute.For<ICurrentSubscriptionAccessor>();
        accessor.GetCurrentSubscription().Returns(subscription);
        return accessor;
    }
}
