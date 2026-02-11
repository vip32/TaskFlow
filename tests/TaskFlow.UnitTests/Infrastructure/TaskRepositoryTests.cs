using TaskFlow.Domain;
using TaskFlow.Infrastructure.Repositories;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.UnitTests.Infrastructure;

public class TaskRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetByProjectIdAsync_ReturnsOnlyTopLevelOrderedTasksForCurrentSubscription()
    {
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

        var repository = new TaskRepository(factory, new TestCurrentSubscriptionAccessor(subscriptionId));
        var result = await repository.GetByProjectIdAsync(projectId);

        Assert.Equal(3, result.Count);
        Assert.Equal("A", result[0].Title);
        Assert.Equal("B", result[1].Title);
        Assert.Equal("Parent", result[2].Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetSubTasksAsync_ReturnsOrderedSubTasks()
    {
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

        var repository = new TaskRepository(factory, new TestCurrentSubscriptionAccessor(subscriptionId));
        var result = await repository.GetSubTasksAsync(parent.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal(first.Id, result[0].Id);
        Assert.Equal(second.Id, result[1].Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetNextSortOrderAsync_ReturnsMaxPlusOneWithinScope()
    {
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

        var repository = new TaskRepository(factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var next = await repository.GetNextSortOrderAsync(projectId, null);

        Assert.Equal(6, next);
    }

    [Fact]
    public async System.Threading.Tasks.Task SearchAsync_WhitespaceQuery_ReturnsEmptyList()
    {
        var subscriptionId = Guid.NewGuid();
        var repository = new TaskRepository(
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(subscriptionId));

        var result = await repository.SearchAsync(" ", Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public async System.Threading.Tasks.Task DueDateQueries_FilterCorrectly()
    {
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

        var repository = new TaskRepository(factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var onDate = await repository.GetDueOnDateAsync(today);
        var inRange = await repository.GetDueInRangeAsync(today, today.AddDays(1));
        var afterDate = await repository.GetDueAfterDateAsync(today.AddDays(1));

        Assert.Single(onDate);
        Assert.Equal(2, inRange.Count);
        Assert.Single(afterDate);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAndUpdateRange_WithMismatchedSubscription_ThrowsInvalidOperationException()
    {
        var subscriptionId = Guid.NewGuid();
        var repository = new TaskRepository(
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(subscriptionId));

        var foreign = new DomainTask(Guid.NewGuid(), "Foreign", Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddAsync(foreign));
        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdateRangeAsync([foreign]));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdsAndDeleteAsync_RespectsCurrentSubscription()
    {
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

        var repository = new TaskRepository(factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var byIds = await repository.GetByIdsAsync([mine.Id, foreign.Id]);
        var deletedMine = await repository.DeleteAsync(mine.Id);
        var deletedForeign = await repository.DeleteAsync(foreign.Id);

        Assert.Single(byIds);
        Assert.Equal(mine.Id, byIds[0].Id);
        Assert.True(deletedMine);
        Assert.False(deletedForeign);
    }
}
