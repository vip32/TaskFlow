using Microsoft.Extensions.Logging.Abstractions;
using TaskFlow.Domain;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.UnitTests.Infrastructure;

public class MyTaskFlowSectionRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task GetAllAsync_FiltersByCurrentSubscriptionAndOrdersBySortOrderThenName()
    {
        // Arrange
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

        // Act
        var sut = new MyTaskFlowSectionRepository(NullLogger<MyTaskFlowSectionRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var result = await sut.GetAllAsync();

        // Assert
        result.Count.ShouldBe(3);
        result[0].Name.ShouldBe("First");
        result[1].Name.ShouldBe("A");
        result[2].Name.ShouldBe("B");
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdAsync_FallsBackToSectionIdLookupWhenSubscriptionScopedMisses()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var foreignSection = new MyTaskFlowSection(Guid.NewGuid(), "Foreign", 1);
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));

        await using (var db = await factory.CreateDbContextAsync())
        {
            db.MyTaskFlowSections.Add(foreignSection);
            await db.SaveChangesAsync();
        }

        // Act
        var sut = new MyTaskFlowSectionRepository(NullLogger<MyTaskFlowSectionRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var fetched = await sut.GetByIdAsync(foreignSection.Id);

        // Assert
        fetched.Id.ShouldBe(foreignSection.Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddUpdateDeleteAsync_PerformsCrudWithinSubscription()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        // Act
        var sut = new MyTaskFlowSectionRepository(
            NullLogger<MyTaskFlowSectionRepository>.Instance,
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(subscriptionId));

        var section = new MyTaskFlowSection(subscriptionId, "Inbox", 1);
        await sut.AddAsync(section);

        section.Rename("Renamed");
        await sut.UpdateAsync(section);
        var fetched = await sut.GetByIdAsync(section.Id);
        var deleted = await sut.DeleteAsync(section.Id);

        // Assert
        fetched.Name.ShouldBe("Renamed");
        deleted.ShouldBeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAsync_MismatchedSubscription_ThrowsInvalidOperationException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        // Act
        var sut = new MyTaskFlowSectionRepository(
            NullLogger<MyTaskFlowSectionRepository>.Instance,
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(subscriptionId));

        var foreignSection = new MyTaskFlowSection(Guid.NewGuid(), "Foreign", 1);

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(() => sut.AddAsync(foreignSection));
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_MissingSection_ReturnsFalse()
    {
        // Arrange

        // Act
        var sut = new MyTaskFlowSectionRepository(
            NullLogger<MyTaskFlowSectionRepository>.Instance,
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(Guid.NewGuid()));

        var deleted = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        deleted.ShouldBeFalse();
    }
}
