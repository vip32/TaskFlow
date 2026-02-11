using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.UnitTests.Domain;

public class ProjectTests
{
    [Fact]
    public void Constructor_EmptySubscription_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Project(Guid.Empty, "Work", "#123456", "work"));
    }

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

    [Fact]
    public void AddTask_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        var project = new Project(Guid.NewGuid(), "Work", "#40E0D0", "work");
        var task = new DomainTask(Guid.NewGuid(), "Task", project.Id);

        Assert.Throws<InvalidOperationException>(() => project.AddTask(task));
    }

    [Fact]
    public void AddTask_Duplicate_ThrowsInvalidOperationException()
    {
        var subscriptionId = Guid.NewGuid();
        var project = new Project(subscriptionId, "Work", "#40E0D0", "work");
        var task = new DomainTask(subscriptionId, "Task", project.Id);

        project.AddTask(task);

        Assert.Throws<InvalidOperationException>(() => project.AddTask(task));
    }

    [Fact]
    public void AddAndRemoveTask_UpdatesTaskCount()
    {
        var subscriptionId = Guid.NewGuid();
        var project = new Project(subscriptionId, "Work", "#40E0D0", "work");
        var task = new DomainTask(subscriptionId, "Task", null);

        project.AddTask(task);
        Assert.Equal(project.Id, task.ProjectId);
        Assert.Equal(1, project.GetTaskCount());

        project.RemoveTask(task);
        Assert.Equal(0, project.GetTaskCount());
    }

    [Fact]
    public void GetTaskCount_CompletedTasksAreExcluded()
    {
        var subscriptionId = Guid.NewGuid();
        var project = new Project(subscriptionId, "Work", "#40E0D0", "work");
        var active = new DomainTask(subscriptionId, "Active", project.Id);
        var done = new DomainTask(subscriptionId, "Done", project.Id);
        done.Complete();
        project.AddTask(active);
        project.AddTask(done);

        var count = project.GetTaskCount();

        Assert.Equal(1, count);
    }

    [Fact]
    public void UpdateNameColorIcon_Invalid_Throws()
    {
        var project = new Project(Guid.NewGuid(), "Work", "#123456", "work");

        Assert.Throws<ArgumentException>(() => project.UpdateName(" "));
        Assert.Throws<ArgumentException>(() => project.UpdateColor(" "));
        Assert.Throws<ArgumentException>(() => project.UpdateIcon(" "));
    }

    [Fact]
    public void TagOperations_AreCaseInsensitive()
    {
        var project = new Project(Guid.NewGuid(), "Work", "#123456", "work");
        project.AddTag("Study");
        project.AddTag("study");

        Assert.Single(project.Tags);

        project.RemoveTag("STUDY");
        Assert.Empty(project.Tags);
    }

    [Fact]
    public void Rehydrate_PopulatesFields()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var project = Project.Rehydrate(
            Guid.NewGuid(),
            id,
            "Work",
            "#123456",
            "work",
            "note",
            isDefault: true,
            viewType: ProjectViewType.Board,
            createdAt: createdAt,
            tags: ["A"]);

        Assert.Equal(id, project.Id);
        Assert.Equal(ProjectViewType.Board, project.ViewType);
        Assert.Equal(createdAt, project.CreatedAt);
        Assert.Single(project.Tags);
    }
}
