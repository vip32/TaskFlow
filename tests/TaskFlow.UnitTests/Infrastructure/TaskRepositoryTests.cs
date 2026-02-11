using TaskFlow.Domain;
using TaskFlow.Infrastructure.Repositories;
using DomainTask = TaskFlow.Domain.Task;

namespace TaskFlow.UnitTests.Infrastructure;

public class TaskRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task AddAsync_ValidSubscription_PersistsTask()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(AddAsync_ValidSubscription_PersistsTask));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new TaskRepository(factory, accessor);
        var task = new DomainTask(subscriptionId, "Task", Guid.NewGuid());

        var created = await repository.AddAsync(task);
        var loaded = await repository.GetByIdAsync(created.Id);

        Assert.Equal("Task", loaded.Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAsync_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        var factory = new InMemoryAppDbContextFactory(nameof(AddAsync_MismatchedSubscription_ThrowsInvalidOperationException));
        var accessor = new TestCurrentSubscriptionAccessor(Guid.NewGuid());
        var repository = new TaskRepository(factory, accessor);
        var task = new DomainTask(Guid.NewGuid(), "Task", Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddAsync(task));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByProjectIdAsync_ReturnsOnlyTopLevelTasksForCurrentSubscription()
    {
        var currentSubscriptionId = Guid.NewGuid();
        var otherSubscriptionId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(GetByProjectIdAsync_ReturnsOnlyTopLevelTasksForCurrentSubscription));
        var accessor = new TestCurrentSubscriptionAccessor(currentSubscriptionId);
        var repository = new TaskRepository(factory, accessor);

        var parent = new DomainTask(currentSubscriptionId, "Parent", projectId);
        parent.SetSortOrder(1);
        var child = new DomainTask(currentSubscriptionId, "Child", projectId);
        parent.AddSubTask(child);
        var otherSubscriptionTask = new DomainTask(otherSubscriptionId, "Other", projectId);

        await using (var db = factory.CreateDbContext())
        {
            db.Tasks.Add(parent);
            db.Tasks.Add(child);
            db.Tasks.Add(otherSubscriptionTask);
            await db.SaveChangesAsync();
        }

        var tasks = await repository.GetByProjectIdAsync(projectId);

        var result = Assert.Single(tasks);
        Assert.Equal(parent.Id, result.Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetSubTasksAsync_ReturnsChildrenByParent()
    {
        var subscriptionId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(GetSubTasksAsync_ReturnsChildrenByParent));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new TaskRepository(factory, accessor);

        var parent = new DomainTask(subscriptionId, "Parent", projectId);
        var first = new DomainTask(subscriptionId, "First", projectId);
        var second = new DomainTask(subscriptionId, "Second", projectId);
        parent.AddSubTask(first);
        parent.AddSubTask(second);

        await using (var db = factory.CreateDbContext())
        {
            db.Tasks.Add(parent);
            db.Tasks.Add(first);
            db.Tasks.Add(second);
            await db.SaveChangesAsync();
        }

        var tasks = await repository.GetSubTasksAsync(parent.Id);

        Assert.Equal(2, tasks.Count);
        Assert.Equal(first.Id, tasks[0].Id);
        Assert.Equal(second.Id, tasks[1].Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetNextSortOrderAsync_ReturnsMaxPlusOne()
    {
        var subscriptionId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(GetNextSortOrderAsync_ReturnsMaxPlusOne));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new TaskRepository(factory, accessor);

        var first = new DomainTask(subscriptionId, "First", projectId);
        first.SetSortOrder(3);
        var second = new DomainTask(subscriptionId, "Second", projectId);
        second.SetSortOrder(8);

        await using (var db = factory.CreateDbContext())
        {
            db.Tasks.Add(first);
            db.Tasks.Add(second);
            await db.SaveChangesAsync();
        }

        var next = await repository.GetNextSortOrderAsync(projectId, null);

        Assert.Equal(9, next);
    }

    [Fact]
    public async System.Threading.Tasks.Task SearchAsync_WhitespaceQuery_ReturnsEmpty()
    {
        var repository = new TaskRepository(
            new InMemoryAppDbContextFactory(nameof(SearchAsync_WhitespaceQuery_ReturnsEmpty)),
            new TestCurrentSubscriptionAccessor(Guid.NewGuid()));

        var tasks = await repository.SearchAsync("  ", Guid.NewGuid());

        Assert.Empty(tasks);
    }

    [Fact]
    public async System.Threading.Tasks.Task SearchAsync_MatchesTitleAndNote()
    {
        var subscriptionId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(SearchAsync_MatchesTitleAndNote));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new TaskRepository(factory, accessor);

        var titleMatch = new DomainTask(subscriptionId, "Build API", projectId);
        var noteMatch = new DomainTask(subscriptionId, "Other", projectId);
        noteMatch.UpdateNote("review API contract");
        var noMatch = new DomainTask(subscriptionId, "Nope", projectId);

        await using (var db = factory.CreateDbContext())
        {
            db.Tasks.Add(titleMatch);
            db.Tasks.Add(noteMatch);
            db.Tasks.Add(noMatch);
            await db.SaveChangesAsync();
        }

        var tasks = await repository.SearchAsync("api", projectId);

        Assert.Equal(2, tasks.Count);
    }

    [Fact]
    public async System.Threading.Tasks.Task DateFilters_ReturnExpectedDueTasks()
    {
        var subscriptionId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(DateFilters_ReturnExpectedDueTasks));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new TaskRepository(factory, accessor);
        var today = new DateOnly(2026, 2, 11);

        var dueToday = new DomainTask(subscriptionId, "Today", projectId);
        dueToday.SetDueDate(today);
        var dueInRange = new DomainTask(subscriptionId, "Range", projectId);
        dueInRange.SetDueDate(today.AddDays(2));
        var dueAfter = new DomainTask(subscriptionId, "After", projectId);
        dueAfter.SetDueDate(today.AddDays(10));

        await using (var db = factory.CreateDbContext())
        {
            db.Tasks.Add(dueToday);
            db.Tasks.Add(dueInRange);
            db.Tasks.Add(dueAfter);
            await db.SaveChangesAsync();
        }

        var onDate = await repository.GetDueOnDateAsync(today);
        var inRange = await repository.GetDueInRangeAsync(today, today.AddDays(6));
        var afterDate = await repository.GetDueAfterDateAsync(today.AddDays(6));

        Assert.Single(onDate);
        Assert.Equal(2, inRange.Count);
        Assert.Single(afterDate);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdsAsync_EmptyIds_ReturnsEmpty()
    {
        var repository = new TaskRepository(
            new InMemoryAppDbContextFactory(nameof(GetByIdsAsync_EmptyIds_ReturnsEmpty)),
            new TestCurrentSubscriptionAccessor(Guid.NewGuid()));

        var tasks = await repository.GetByIdsAsync([]);

        Assert.Empty(tasks);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateRangeAndDelete_ApplyChangesAndReturnExpectedValues()
    {
        var subscriptionId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(UpdateRangeAndDelete_ApplyChangesAndReturnExpectedValues));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new TaskRepository(factory, accessor);

        var first = await repository.AddAsync(new DomainTask(subscriptionId, "First", projectId));
        var second = await repository.AddAsync(new DomainTask(subscriptionId, "Second", projectId));
        first.SetSortOrder(5);
        second.SetSortOrder(6);

        var updated = await repository.UpdateRangeAsync([first, second]);
        var deleted = await repository.DeleteAsync(first.Id);
        var deletedAgain = await repository.DeleteAsync(first.Id);

        Assert.Equal(2, updated.Count);
        Assert.True(deleted);
        Assert.False(deletedAgain);
    }
}

