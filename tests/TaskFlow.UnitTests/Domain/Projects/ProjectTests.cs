using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.UnitTests.Domain;

public class ProjectTests
{
    [Fact]
    public void Constructor_EmptySubscription_ThrowsArgumentException()
    {
        // Arrange

        // Act
        var act = () => new Project(Guid.Empty, "Work", "#123456", "work");

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void Constructor_NameTooLong_ThrowsArgumentException()
    {
        // Arrange
        var name = new string('a', 101);

        // Act
        var act = () => new Project(Guid.NewGuid(), name, "#123456", "work");

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void UpdateNote_Whitespace_ClearsToNull()
    {
        // Arrange
        var sut = new Project(Guid.NewGuid(), "Work", "#123456", "work", "Initial");

        // Act
        sut.UpdateNote(" ");

        // Assert
        sut.Note.ShouldBeNull();
    }

    [Fact]
    public void AddTask_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut = new Project(Guid.NewGuid(), "Work", "#40E0D0", "work");
        var task = new DomainTask(Guid.NewGuid(), "Task", sut.Id);

        // Act
        var act = () => sut.AddTask(task);

        // Assert
        Should.Throw<InvalidOperationException>(act);
    }

    [Fact]
    public void AddTask_Duplicate_ThrowsInvalidOperationException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var sut = new Project(subscriptionId, "Work", "#40E0D0", "work");
        var task = new DomainTask(subscriptionId, "Task", sut.Id);
        sut.AddTask(task);

        // Act
        var act = () => sut.AddTask(task);

        // Assert
        Should.Throw<InvalidOperationException>(act);
    }

    [Fact]
    public void AddAndRemoveTask_UpdatesTaskCount()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var sut = new Project(subscriptionId, "Work", "#40E0D0", "work");
        var task = new DomainTask(subscriptionId, "Task", null);

        // Act
        sut.AddTask(task);

        // Assert
        task.ProjectId.ShouldBe(sut.Id);
        sut.GetTaskCount().ShouldBe(1);

        sut.RemoveTask(task);
        sut.GetTaskCount().ShouldBe(0);
    }

    [Fact]
    public void GetTaskCount_CompletedTasksAreExcluded()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var sut = new Project(subscriptionId, "Work", "#40E0D0", "work");
        var active = new DomainTask(subscriptionId, "Active", sut.Id);
        var done = new DomainTask(subscriptionId, "Done", sut.Id);
        done.Complete();
        sut.AddTask(active);
        sut.AddTask(done);

        // Act
        var count = sut.GetTaskCount();

        // Assert
        count.ShouldBe(1);
    }

    [Fact]
    public void UpdateNameColorIcon_Invalid_Throws()
    {
        // Arrange
        var sut = new Project(Guid.NewGuid(), "Work", "#123456", "work");

        // Act
        var nameAct = () => sut.UpdateName(" ");
        var colorAct = () => sut.UpdateColor(" ");
        var iconAct = () => sut.UpdateIcon(" ");

        // Assert
        Should.Throw<ArgumentException>(nameAct);
        Should.Throw<ArgumentException>(colorAct);
        Should.Throw<ArgumentException>(iconAct);
    }

    [Fact]
    public void TagOperations_AreCaseInsensitive()
    {
        // Arrange
        var sut = new Project(Guid.NewGuid(), "Work", "#123456", "work");

        // Act
        sut.AddTag("Study");
        sut.AddTag("study");

        // Assert
        sut.Tags.ShouldHaveSingleItem();

        sut.RemoveTag("STUDY");
        sut.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void Rehydrate_PopulatesFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var sut = Project.Rehydrate(
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

        // Assert
        sut.Id.ShouldBe(id);
        sut.ViewType.ShouldBe(ProjectViewType.Board);
        sut.CreatedAt.ShouldBe(createdAt);
        sut.Tags.ShouldHaveSingleItem();
    }
}
