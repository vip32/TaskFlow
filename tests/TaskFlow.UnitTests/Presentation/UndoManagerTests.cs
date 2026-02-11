using TaskFlow.Presentation.Services;

namespace TaskFlow.UnitTests.Presentation;

public class UndoManagerTests
{
    [Fact]
    public async System.Threading.Tasks.Task UndoAsync_ActionExists_ExecutesUndoAndReturnsDescription()
    {
        var sut = new UndoManager();
        var executed = false;
        sut.Register("toggle completion", () =>
        {
            executed = true;
            return System.Threading.Tasks.Task.CompletedTask;
        });

        var result = await sut.UndoAsync();

        Assert.True(executed);
        Assert.True(result.HasAction);
        Assert.Equal("toggle completion", result.Description);
    }

    [Fact]
    public async System.Threading.Tasks.Task UndoAsync_NoActions_ReturnsNone()
    {
        var sut = new UndoManager();

        var result = await sut.UndoAsync();

        Assert.False(result.HasAction);
        Assert.Equal(string.Empty, result.Description);
    }

    [Fact]
    public async System.Threading.Tasks.Task Register_MaxCapacityReached_ClearsOlderActions()
    {
        var sut = new UndoManager(maxActions: 2);

        sut.Register("first", () => System.Threading.Tasks.Task.CompletedTask);
        sut.Register("second", () => System.Threading.Tasks.Task.CompletedTask);
        sut.Register("third", () => System.Threading.Tasks.Task.CompletedTask);

        var firstUndo = await sut.UndoAsync();
        var secondUndo = await sut.UndoAsync();

        Assert.True(firstUndo.HasAction);
        Assert.Equal("third", firstUndo.Description);
        Assert.False(secondUndo.HasAction);
    }

    [Fact]
    public async System.Threading.Tasks.Task UndoAsync_CallbackRegistersAnotherUndo_RegistrationIsIgnoredDuringUndoExecution()
    {
        var sut = new UndoManager();
        sut.Register("outer", () =>
        {
            sut.Register("nested", () => System.Threading.Tasks.Task.CompletedTask);
            return System.Threading.Tasks.Task.CompletedTask;
        });

        var firstUndo = await sut.UndoAsync();
        var secondUndo = await sut.UndoAsync();

        Assert.True(firstUndo.HasAction);
        Assert.Equal("outer", firstUndo.Description);
        Assert.False(secondUndo.HasAction);
    }

    [Fact]
    public async System.Threading.Tasks.Task UndoAsync_CallbackThrows_ManagerCanBeUsedAgain()
    {
        var sut = new UndoManager();
        sut.Register("bad", () => throw new InvalidOperationException("boom"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UndoAsync());

        sut.Register("good", () => System.Threading.Tasks.Task.CompletedTask);
        var result = await sut.UndoAsync();
        Assert.True(result.HasAction);
        Assert.Equal("good", result.Description);
    }
}
