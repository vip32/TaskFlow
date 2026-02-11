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

    [Fact]
    public void Constructor_ValidInput_SetsProperties()
    {
        var sectionId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var relation = new MyTaskFlowSectionTask(sectionId, taskId);

        Assert.Equal(sectionId, relation.SectionId);
        Assert.Equal(taskId, relation.TaskId);
    }
}
