using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Domain;

public class MyTaskFlowSectionTaskTests
{
    [Fact]
    public void Constructor_EmptySectionId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MyTaskFlowSectionTask(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_EmptyTaskId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MyTaskFlowSectionTask(Guid.NewGuid(), Guid.Empty));
    }
}
