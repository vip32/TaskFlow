using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

[Trait("Layer", "Domain")]
[Trait("Slice", "Flow")]
[Trait("Type", "Unit")]
public class MyTaskFlowSectionTaskTests
{
    [Fact]
    public void Constructor_EmptySectionId_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var act = () => new MyTaskFlowSectionTask(Guid.Empty, Guid.NewGuid());

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Constructor_EmptyTaskId_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var act = () => new MyTaskFlowSectionTask(Guid.NewGuid(), Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }
}


