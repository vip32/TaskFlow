using Microsoft.Extensions.Logging;
using NSubstitute;
using TaskFlow.Application;
using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Application;

[Trait("Layer", "Application")]
[Trait("Slice", "Subscription")]
[Trait("Type", "Unit")]
public class SubscriptionSettingsOrchestratorTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetAsync_ForwardsToRepository()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        var repository = Substitute.For<ISubscriptionRepository>();
        repository.GetCurrentAsync(Arg.Any<CancellationToken>()).Returns(subscription);
        var sut = new SubscriptionSettingsOrchestrator(Substitute.For<ILogger<SubscriptionSettingsOrchestrator>>(), repository);

        // Act
        var result = await sut.GetAsync();

        // Assert
        result.ShouldBe(subscription.Settings);
        await repository.Received(1).GetCurrentAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateAlwaysShowCompletedTasksAsync_ValidValue_PersistsUpdatedSettings()
    {
        // Arrange
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        var repository = Substitute.For<ISubscriptionRepository>();
        repository.GetCurrentAsync(Arg.Any<CancellationToken>()).Returns(subscription);
        repository.UpdateAsync(Arg.Any<Subscription>(), Arg.Any<CancellationToken>()).Returns(call => call.Arg<Subscription>());
        var sut = new SubscriptionSettingsOrchestrator(Substitute.For<ILogger<SubscriptionSettingsOrchestrator>>(), repository);

        // Act
        var updated = await sut.UpdateAlwaysShowCompletedTasksAsync(true);

        // Assert
        updated.AlwaysShowCompletedTasks.ShouldBeTrue();
        await repository.Received(1).UpdateAsync(Arg.Is<Subscription>(x => x.Settings.AlwaysShowCompletedTasks), Arg.Any<CancellationToken>());
    }
}
