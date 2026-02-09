using TaskFlow.Domain;

namespace TaskFlow.UnitTests;

public class ProjectDomainTests
{
    [Fact]
    public void Constructor_NameTooLong_ThrowsArgumentException()
    {
        var name = new string('a', 101);

        Assert.Throws<ArgumentException>(() => new Project(Guid.NewGuid(), name, "#123456", "work"));
    }

    [Fact]
    public void UpdateNote_Whitespace_ClearsToNull()
    {
        var project = new Project(Guid.NewGuid(), "Work", "#123456", "work", "Initial");

        project.UpdateNote(" ");

        Assert.Null(project.Note);
    }
}
