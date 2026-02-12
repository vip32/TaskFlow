using Microsoft.Extensions.Logging.Abstractions;
using TaskFlow.Infrastructure.Repositories;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.UnitTests.Infrastructure;

public class TaskRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetByProjectIdAsync_ReturnsOnlyTopLevelOrderedTasksForCurrentSubscription()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var otherSubscriptionId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));

        var topLevelLater = new DomainTask(subscriptionId, "B", projectId);
        topLevelLater.SetSortOrder(1);

        var topLevelFirst = new DomainTask(subscriptionId, "A", projectId);
        topLevelFirst.SetSortOrder(0);

        var parent = new DomainTask(subscriptionId, "Parent", projectId);
        parent.SetSortOrder(2);
        var subTask = new DomainTask(subscriptionId, "Sub", projectId);
        parent.AddSubTask(subTask);

        var foreign = new DomainTask(otherSubscriptionId, "Foreign", projectId);

        await using (var db = await factory.CreateDbContextAsync())
        {
            db.Tasks.AddRange(topLevelLater, topLevelFirst, parent, subTask, foreign);
            await db.SaveChangesAsync();
        }

        // Act
        var sut = new TaskRepository(NullLogger<TaskRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));
        var result = await sut.GetByProjectIdAsync(projectId);

        // Assert
        result.Count.ShouldBe(3);
        result[0].Title.ShouldBe("A");
        result[1].Title.ShouldBe("B");
        result[2].Title.ShouldBe("Parent");
    }

    [Fact]
    public async System.Threading.Tasks.Task GetSubTasksAsync_ReturnsOrderedSubTasks()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var parent = new DomainTask(subscriptionId, "Parent", projectId);

        var first = new DomainTask(subscriptionId, "First", projectId);
        var second = new DomainTask(subscriptionId, "Second", projectId);
        parent.AddSubTask(first);
        parent.AddSubTask(second);

        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));
        await using (var db = await factory.CreateDbContextAsync())
        {
            db.Tasks.AddRange(parent, first, second);
            await db.SaveChangesAsync();
        }

        // Act
        var sut = new TaskRepository(NullLogger<TaskRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));
        var result = await sut.GetSubTasksAsync(parent.Id);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Id.ShouldBe(first.Id);
        result[1].Id.ShouldBe(second.Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetNextSortOrderAsync_ReturnsMaxPlusOneWithinScope()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var first = new DomainTask(subscriptionId, "A", projectId);
        first.SetSortOrder(2);
        var second = new DomainTask(subscriptionId, "B", projectId);
        second.SetSortOrder(5);

        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));
        await using (var db = await factory.CreateDbContextAsync())
        {
            db.Tasks.AddRange(first, second);
            await db.SaveChangesAsync();
        }

        // Act
        var sut = new TaskRepository(NullLogger<TaskRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var next = await sut.GetNextSortOrderAsync(projectId, null);

        // Assert
        next.ShouldBe(6);
    }

    [Fact]
    public async System.Threading.Tasks.Task SearchAsync_WhitespaceQuery_ReturnsEmptyList()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        // Act
        var sut = new TaskRepository(
            NullLogger<TaskRepository>.Instance,
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(subscriptionId));

        var result = await sut.SearchAsync(" ", Guid.NewGuid());

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async System.Threading.Tasks.Task DueDateQueries_FilterCorrectly()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var today = new DateOnly(2026, 2, 11);

        var dueToday = new DomainTask(subscriptionId, "Today", projectId);
        dueToday.SetDueDate(today);

        var dueTomorrow = new DomainTask(subscriptionId, "Tomorrow", projectId);
        dueTomorrow.SetDueDate(today.AddDays(1));

        var dueLater = new DomainTask(subscriptionId, "Later", projectId);
        dueLater.SetDueDate(today.AddDays(10));

        var noDueDate = new DomainTask(subscriptionId, "NoDate", projectId);

        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));
        await using (var db = await factory.CreateDbContextAsync())
        {
            db.Tasks.AddRange(dueToday, dueTomorrow, dueLater, noDueDate);
            await db.SaveChangesAsync();
        }

        // Act
        var sut = new TaskRepository(NullLogger<TaskRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var onDate = await sut.GetDueOnDateAsync(today);
        var inRange = await sut.GetDueInRangeAsync(today, today.AddDays(1));
        var afterDate = await sut.GetDueAfterDateAsync(today.AddDays(1));

        // Assert
        onDate.ShouldHaveSingleItem();
        inRange.Count.ShouldBe(2);
        afterDate.ShouldHaveSingleItem();
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAndUpdateRange_WithMismatchedSubscription_ThrowsInvalidOperationException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        // Act
        var sut = new TaskRepository(
            NullLogger<TaskRepository>.Instance,
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(subscriptionId));

        var foreign = new DomainTask(Guid.NewGuid(), "Foreign", Guid.NewGuid());

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(() => sut.AddAsync(foreign));
        await Should.ThrowAsync<InvalidOperationException>(() => sut.UpdateRangeAsync([foreign]));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdsAndDeleteAsync_RespectsCurrentSubscription()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var foreignSubscriptionId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var mine = new DomainTask(subscriptionId, "Mine", projectId);
        var foreign = new DomainTask(foreignSubscriptionId, "Foreign", projectId);

        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));
        await using (var db = await factory.CreateDbContextAsync())
        {
            db.Tasks.AddRange(mine, foreign);
            await db.SaveChangesAsync();
        }

        // Act
        var sut = new TaskRepository(NullLogger<TaskRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var byIds = await sut.GetByIdsAsync([mine.Id, foreign.Id]);
        var deletedMine = await sut.DeleteAsync(mine.Id);
        var deletedForeign = await sut.DeleteAsync(foreign.Id);

        // Assert
        byIds.ShouldHaveSingleItem();
        byIds[0].Id.ShouldBe(mine.Id);
        deletedMine.ShouldBeTrue();
        deletedForeign.ShouldBeFalse();
    }
}
