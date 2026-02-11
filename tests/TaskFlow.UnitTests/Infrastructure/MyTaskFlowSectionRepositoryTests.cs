using TaskFlow.Domain;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.UnitTests.Infrastructure;

public class MyTaskFlowSectionRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetAllAsync_FiltersByCurrentSubscriptionAndOrdersBySortOrderThenName()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));

        await using (var db = await factory.CreateDbContextAsync())
        {
            db.MyTaskFlowSections.Add(new MyTaskFlowSection(subscriptionId, "B", 2));
            db.MyTaskFlowSections.Add(new MyTaskFlowSection(subscriptionId, "A", 2));
            db.MyTaskFlowSections.Add(new MyTaskFlowSection(subscriptionId, "First", 1));
            db.MyTaskFlowSections.Add(new MyTaskFlowSection(Guid.NewGuid(), "Foreign", 0));
            await db.SaveChangesAsync();
        }

        var repository = new MyTaskFlowSectionRepository(factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var result = await repository.GetAllAsync();

        Assert.Equal(3, result.Count);
        Assert.Equal("First", result[0].Name);
        Assert.Equal("A", result[1].Name);
        Assert.Equal("B", result[2].Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdAsync_FallsBackToSectionIdLookupWhenSubscriptionScopedMisses()
    {
        var subscriptionId = Guid.NewGuid();
        var foreignSection = new MyTaskFlowSection(Guid.NewGuid(), "Foreign", 1);
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));

        await using (var db = await factory.CreateDbContextAsync())
        {
            db.MyTaskFlowSections.Add(foreignSection);
            await db.SaveChangesAsync();
        }

        var repository = new MyTaskFlowSectionRepository(factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var fetched = await repository.GetByIdAsync(foreignSection.Id);

        Assert.Equal(foreignSection.Id, fetched.Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddUpdateDeleteAsync_PerformsCrudWithinSubscription()
    {
        var subscriptionId = Guid.NewGuid();
        var repository = new MyTaskFlowSectionRepository(
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(subscriptionId));

        var section = new MyTaskFlowSection(subscriptionId, "Inbox", 1);
        await repository.AddAsync(section);

        section.Rename("Renamed");
        await repository.UpdateAsync(section);
        var fetched = await repository.GetByIdAsync(section.Id);
        var deleted = await repository.DeleteAsync(section.Id);

        Assert.Equal("Renamed", fetched.Name);
        Assert.True(deleted);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAsync_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        var subscriptionId = Guid.NewGuid();
        var repository = new MyTaskFlowSectionRepository(
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(subscriptionId));

        var foreignSection = new MyTaskFlowSection(Guid.NewGuid(), "Foreign", 1);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddAsync(foreignSection));
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_MissingSection_ReturnsFalse()
    {
        var repository = new MyTaskFlowSectionRepository(
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(Guid.NewGuid()));

        var deleted = await repository.DeleteAsync(Guid.NewGuid());

        Assert.False(deleted);
    }
}
