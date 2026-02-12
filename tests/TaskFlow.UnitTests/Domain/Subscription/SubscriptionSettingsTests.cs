using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

[Trait("Layer", "Domain")]
[Trait("Slice", "Subscription")]
[Trait("Type", "Unit")]
public class SubscriptionSettingsTests
{
    [Fact]
    public void Constructor_EmptySubscriptionId_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var act = () => new SubscriptionSettings(Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetAlwaysShowCompletedTasks_ValidValue_UpdatesPreference()
    {
        // Arrange
        var sut = new SubscriptionSettings(Guid.NewGuid(), alwaysShowCompletedTasks: false);

        // Act
        sut.SetAlwaysShowCompletedTasks(true);

        // Assert
        sut.AlwaysShowCompletedTasks.ShouldBeTrue();
    }
}
