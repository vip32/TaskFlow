using TaskFlow.Domain;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.UnitTests.Infrastructure;

public class MyTaskFlowSectionRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task AddAsync_ValidSubscription_PersistsSection()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(AddAsync_ValidSubscription_PersistsSection));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new MyTaskFlowSectionRepository(factory, accessor);
        var section = new MyTaskFlowSection(subscriptionId, "Inbox", 1);

        var created = await repository.AddAsync(section);
        var loaded = await repository.GetByIdAsync(created.Id);

        Assert.Equal("Inbox", loaded.Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAsync_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        var factory = new InMemoryAppDbContextFactory(nameof(AddAsync_MismatchedSubscription_ThrowsInvalidOperationException));
        var accessor = new TestCurrentSubscriptionAccessor(Guid.NewGuid());
        var repository = new MyTaskFlowSectionRepository(factory, accessor);
        var section = new MyTaskFlowSection(Guid.NewGuid(), "Inbox", 1);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddAsync(section));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllAsync_FiltersAndSortsBySortOrderThenName()
    {
        var currentSubscriptionId = Guid.NewGuid();
        var otherSubscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(GetAllAsync_FiltersAndSortsBySortOrderThenName));
        var accessor = new TestCurrentSubscriptionAccessor(currentSubscriptionId);
        var repository = new MyTaskFlowSectionRepository(factory, accessor);

        await using (var db = factory.CreateDbContext())
        {
            db.MyTaskFlowSections.Add(new MyTaskFlowSection(currentSubscriptionId, "B", 1));
            db.MyTaskFlowSections.Add(new MyTaskFlowSection(currentSubscriptionId, "A", 1));
            db.MyTaskFlowSections.Add(new MyTaskFlowSection(otherSubscriptionId, "Other", 0));
            await db.SaveChangesAsync();
        }

        var sections = await repository.GetAllAsync();

        Assert.Equal(2, sections.Count);
        Assert.Equal("A", sections[0].Name);
        Assert.Equal("B", sections[1].Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateAsync_ExistingSection_PersistsChanges()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(UpdateAsync_ExistingSection_PersistsChanges));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new MyTaskFlowSectionRepository(factory, accessor);
        var section = await repository.AddAsync(new MyTaskFlowSection(subscriptionId, "Inbox", 1));
        section.Rename("Today");
        section.Reorder(5);

        await repository.UpdateAsync(section);
        var loaded = await repository.GetByIdAsync(section.Id);

        Assert.Equal("Today", loaded.Name);
        Assert.Equal(5, loaded.SortOrder);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_ExistingAndMissingSection_ReturnsExpectedValues()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(nameof(DeleteAsync_ExistingAndMissingSection_ReturnsExpectedValues));
        var accessor = new TestCurrentSubscriptionAccessor(subscriptionId);
        var repository = new MyTaskFlowSectionRepository(factory, accessor);
        var section = await repository.AddAsync(new MyTaskFlowSection(subscriptionId, "Inbox", 1));

        var deleted = await repository.DeleteAsync(section.Id);
        var deletedAgain = await repository.DeleteAsync(section.Id);

        Assert.True(deleted);
        Assert.False(deletedAgain);
    }
}

