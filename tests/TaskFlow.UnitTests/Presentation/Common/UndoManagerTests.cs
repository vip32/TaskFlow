using TaskFlow.Presentation.Services;

namespace TaskFlow.UnitTests.Presentation;

public class UndoManagerTests
{
    [Fact]
    public async System.Threading.Tasks.Task UndoAsync_ActionExists_ExecutesUndoAndReturnsDescription()
    {
        // Arrange

        // Act
        var sut = new UndoManager();
        var executed = false;
        sut.Register("toggle completion", () =>
        {
            executed = true;
            return System.Threading.Tasks.Task.CompletedTask;
        });

        var result = await sut.UndoAsync();

        // Assert
        executed.ShouldBeTrue();
        result.HasAction.ShouldBeTrue();
        result.Description.ShouldBe("toggle completion");
    }

    [Fact]
    public async System.Threading.Tasks.Task UndoAsync_NoActions_ReturnsNone()
    {
        // Arrange

        // Act
        var sut = new UndoManager();

        var result = await sut.UndoAsync();

        // Assert
        result.HasAction.ShouldBeFalse();
        result.Description.ShouldBe(string.Empty);
    }

    [Fact]
    public async System.Threading.Tasks.Task Register_MaxCapacityReached_ClearsOlderActions()
    {
        // Arrange

        // Act
        var sut = new UndoManager(maxActions: 2);

        sut.Register("first", () => System.Threading.Tasks.Task.CompletedTask);
        sut.Register("second", () => System.Threading.Tasks.Task.CompletedTask);
        sut.Register("third", () => System.Threading.Tasks.Task.CompletedTask);

        var firstUndo = await sut.UndoAsync();
        var secondUndo = await sut.UndoAsync();

        // Assert
        firstUndo.HasAction.ShouldBeTrue();
        firstUndo.Description.ShouldBe("third");
        secondUndo.HasAction.ShouldBeFalse();
    }

    [Fact]
    public async System.Threading.Tasks.Task UndoAsync_CallbackRegistersAnotherUndo_RegistrationIsIgnoredDuringUndoExecution()
    {
        // Arrange

        // Act
        var sut = new UndoManager();
        sut.Register("outer", () =>
        {
            sut.Register("nested", () => System.Threading.Tasks.Task.CompletedTask);
            return System.Threading.Tasks.Task.CompletedTask;
        });

        var firstUndo = await sut.UndoAsync();
        var secondUndo = await sut.UndoAsync();

        // Assert
        firstUndo.HasAction.ShouldBeTrue();
        firstUndo.Description.ShouldBe("outer");
        secondUndo.HasAction.ShouldBeFalse();
    }

    [Fact]
    public async System.Threading.Tasks.Task UndoAsync_CallbackThrows_ManagerCanBeUsedAgain()
    {
        // Arrange

        // Act
        var sut = new UndoManager();
        sut.Register("bad", () => throw new InvalidOperationException("boom"));

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(() => sut.UndoAsync());

        sut.Register("good", () => System.Threading.Tasks.Task.CompletedTask);
        var result = await sut.UndoAsync();
        result.HasAction.ShouldBeTrue();
        result.Description.ShouldBe("good");
    }
}
