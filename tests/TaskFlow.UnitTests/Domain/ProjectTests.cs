using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.UnitTests.Domain;

public class ProjectTests
{
    [Fact]
    public void Constructor_EmptySubscriptionId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Project(Guid.Empty, "Work", "#123456", "work"));
    }

    [Fact]
    public void Constructor_WhitespaceName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Project(Guid.NewGuid(), " ", "#123456", "work"));
    }

    [Fact]
    public void Constructor_WhitespaceColor_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Project(Guid.NewGuid(), "Work", " ", "work"));
    }

    [Fact]
    public void Constructor_WhitespaceIcon_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Project(Guid.NewGuid(), "Work", "#123456", " "));
    }

    [Fact]
    public void Constructor_NameTooLong_ThrowsArgumentException()
    {
        var name = new string('a', 101);

        Assert.Throws<ArgumentException>(() => new Project(Guid.NewGuid(), name, "#123456", "work"));
    }

    [Fact]
    public void Constructor_ValidInput_TrimsValuesAndDefaultsToListView()
    {
        var project = new Project(Guid.NewGuid(), " Work ", " #123456 ", " work ", " note ", isDefault: true);

        Assert.Equal("Work", project.Name);
        Assert.Equal("#123456", project.Color);
        Assert.Equal("work", project.Icon);
        Assert.Equal("note", project.Note);
        Assert.True(project.IsDefault);
        Assert.Equal(ProjectViewType.List, project.ViewType);
        Assert.NotEqual(Guid.Empty, project.Id);
    }

    [Fact]
    public void UpdateNote_Whitespace_ClearsToNull()
    {
        var project = new Project(Guid.NewGuid(), "Work", "#123456", "work", "Initial");

        project.UpdateNote(" ");

        Assert.Null(project.Note);
    }

    [Fact]
    public void AddTask_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        var project = new Project(Guid.NewGuid(), "Work", "#40E0D0", "work");
        var task = new DomainTask(Guid.NewGuid(), "Task", project.Id);

        Action act = () => project.AddTask(task);

        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void AddTask_DuplicateTask_ThrowsInvalidOperationException()
    {
        var subscriptionId = Guid.NewGuid();
        var project = new Project(subscriptionId, "Work", "#40E0D0", "work");
        var task = new DomainTask(subscriptionId, "Task", null);

        project.AddTask(task);

        Assert.Throws<InvalidOperationException>(() => project.AddTask(task));
    }

    [Fact]
    public void AddTask_ValidTask_AssignsProjectAndSortOrder()
    {
        var subscriptionId = Guid.NewGuid();
        var project = new Project(subscriptionId, "Work", "#40E0D0", "work");
        var first = new DomainTask(subscriptionId, "First", null);
        var second = new DomainTask(subscriptionId, "Second", null);

        project.AddTask(first);
        project.AddTask(second);

        Assert.Equal(project.Id, first.ProjectId);
        Assert.Equal(project.Id, second.ProjectId);
        Assert.Equal(0, first.SortOrder);
        Assert.Equal(1, second.SortOrder);
        Assert.Equal(2, project.Tasks.Count);
    }

    [Fact]
    public void RemoveTask_RemovesTaskById()
    {
        var subscriptionId = Guid.NewGuid();
        var project = new Project(subscriptionId, "Work", "#40E0D0", "work");
        var task = new DomainTask(subscriptionId, "Task", null);
        project.AddTask(task);

        project.RemoveTask(task);

        Assert.Empty(project.Tasks);
    }

    [Fact]
    public void GetTaskCount_ExcludesCompletedTasks()
    {
        var subscriptionId = Guid.NewGuid();
        var project = new Project(subscriptionId, "Work", "#40E0D0", "work");
        var open = new DomainTask(subscriptionId, "Open", null);
        var done = new DomainTask(subscriptionId, "Done", null);
        done.Complete();
        project.AddTask(open);
        project.AddTask(done);

        var count = project.GetTaskCount();

        Assert.Equal(1, count);
    }

    [Fact]
    public void UpdateViewType_ChangesViewType()
    {
        var project = new Project(Guid.NewGuid(), "Work", "#123456", "work");

        project.UpdateViewType(ProjectViewType.Board);

        Assert.Equal(ProjectViewType.Board, project.ViewType);
    }

    [Fact]
    public void UpdateName_Whitespace_ThrowsArgumentException()
    {
        var project = new Project(Guid.NewGuid(), "Work", "#123456", "work");

        Assert.Throws<ArgumentException>(() => project.UpdateName(" "));
    }

    [Fact]
    public void UpdateName_TooLong_ThrowsArgumentException()
    {
        var project = new Project(Guid.NewGuid(), "Work", "#123456", "work");
        var longName = new string('x', 101);

        Assert.Throws<ArgumentException>(() => project.UpdateName(longName));
    }

    [Fact]
    public void UpdateColor_Whitespace_ThrowsArgumentException()
    {
        var project = new Project(Guid.NewGuid(), "Work", "#123456", "work");

        Assert.Throws<ArgumentException>(() => project.UpdateColor(" "));
    }

    [Fact]
    public void UpdateIcon_Whitespace_ThrowsArgumentException()
    {
        var project = new Project(Guid.NewGuid(), "Work", "#123456", "work");

        Assert.Throws<ArgumentException>(() => project.UpdateIcon(" "));
    }

    [Fact]
    public void Tags_AddRemoveAndSet_AreNormalizedAndDistinct()
    {
        var project = new Project(Guid.NewGuid(), "Work", "#123456", "work");

        project.AddTag(" urgent ");
        project.AddTag("URGENT");
        project.AddTag("backend");
        project.RemoveTag(" UrGent ");
        project.SetTags([" alpha ", "ALPHA", " beta "]);

        Assert.Equal(2, project.Tags.Count);
        Assert.Contains("alpha", project.Tags);
        Assert.Contains("beta", project.Tags);
    }

    [Fact]
    public void AddTag_Whitespace_ThrowsArgumentException()
    {
        var project = new Project(Guid.NewGuid(), "Work", "#123456", "work");

        Assert.Throws<ArgumentException>(() => project.AddTag(" "));
    }
}

