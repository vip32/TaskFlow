using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using TaskFlow.Application;
using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Application;

[Trait("Layer", "Application")]
[Trait("Slice", "Focus")]
[Trait("Type", "Unit")]
public class FocusTimerOrchestratorTests
{
    [Fact]
    public async System.Threading.Tasks.Task StartAsync_WithRunningSession_EndsOldAndCreatesNew()
    {
        // Arrange
        var subscription = CreateSubscription();
        var running = new FocusSession(subscription.Id, Guid.NewGuid());
        var repository = Substitute.For<IFocusSessionRepository>();
        repository.GetRunningAsync(Arg.Any<CancellationToken>()).Returns(running);
        repository.AddAsync(Arg.Any<FocusSession>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<FocusSession>());

        var sut = new FocusTimerOrchestrator(
            Substitute.For<ILogger<FocusTimerOrchestrator>>(),
            repository,
            CreateAccessor(subscription));

        // Act
        var created = await sut.StartAsync(Guid.NewGuid());

        // Assert
        running.IsCompleted.ShouldBeTrue();
        created.SubscriptionId.ShouldBe(subscription.Id);
        await repository.Received(1).UpdateAsync(running, Arg.Any<CancellationToken>());
        await repository.Received(1).AddAsync(Arg.Any<FocusSession>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async System.Threading.Tasks.Task StartAsync_EmptyTaskId_CreatesSessionWithoutTask()
    {
        // Arrange
        var subscription = CreateSubscription();
        var repository = Substitute.For<IFocusSessionRepository>();
        repository.GetRunningAsync(Arg.Any<CancellationToken>()).Returns((FocusSession)null);
        repository.AddAsync(Arg.Any<FocusSession>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<FocusSession>());

        var sut = new FocusTimerOrchestrator(
            Substitute.For<ILogger<FocusTimerOrchestrator>>(),
            repository,
            CreateAccessor(subscription));

        // Act
        var created = await sut.StartAsync(Guid.Empty);

        // Assert
        created.TaskId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public async System.Threading.Tasks.Task EndCurrentAsync_NoRunning_ReturnsNull()
    {
        // Arrange
        var subscription = CreateSubscription();
        var repository = Substitute.For<IFocusSessionRepository>();
        repository.GetRunningAsync(Arg.Any<CancellationToken>()).Returns((FocusSession)null);

        var sut = new FocusTimerOrchestrator(
            Substitute.For<ILogger<FocusTimerOrchestrator>>(),
            repository,
            CreateAccessor(subscription));

        // Act
        var ended = await sut.EndCurrentAsync();

        // Assert
        ended.ShouldBeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task EndCurrentAsync_Running_EndsAndUpdates()
    {
        // Arrange
        var subscription = CreateSubscription();
        var running = new FocusSession(subscription.Id);
        var repository = Substitute.For<IFocusSessionRepository>();
        repository.GetRunningAsync(Arg.Any<CancellationToken>()).Returns(running);
        repository.UpdateAsync(Arg.Any<FocusSession>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<FocusSession>());

        var sut = new FocusTimerOrchestrator(
            Substitute.For<ILogger<FocusTimerOrchestrator>>(),
            repository,
            CreateAccessor(subscription));

        // Act
        var ended = await sut.EndCurrentAsync();

        // Assert
        ended.ShouldNotBeNull();
        ended.IsCompleted.ShouldBeTrue();
        await repository.Received(1).UpdateAsync(running, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async System.Threading.Tasks.Task GetRecentAsync_ForwardsToRepository()
    {
        // Arrange
        var subscription = CreateSubscription();
        var repository = Substitute.For<IFocusSessionRepository>();
        repository.GetRecentAsync(10, Arg.Any<CancellationToken>()).Returns([new FocusSession(subscription.Id)]);

        var sut = new FocusTimerOrchestrator(
            Substitute.For<ILogger<FocusTimerOrchestrator>>(),
            repository,
            CreateAccessor(subscription));

        // Act
        var recent = await sut.GetRecentAsync(10);

        // Assert
        recent.Count.ShouldBe(1);
        await repository.Received(1).GetRecentAsync(10, Arg.Any<CancellationToken>());
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


