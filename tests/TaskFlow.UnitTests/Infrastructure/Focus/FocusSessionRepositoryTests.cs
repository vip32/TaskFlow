using Microsoft.Extensions.Logging.Abstractions;
using TaskFlow.Domain;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.UnitTests.Infrastructure;

[Trait("Layer", "Infrastructure")]
[Trait("Slice", "Focus")]
[Trait("Type", "Unit")]
public class FocusSessionRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task AddAndUpdateAsync_PersistSession()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));

        // Act
        var sut = new FocusSessionRepository(NullLogger<FocusSessionRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var session = new FocusSession(subscriptionId, Guid.NewGuid());
        await sut.AddAsync(session);

        session.End();
        var updated = await sut.UpdateAsync(session);

        // Assert
        updated.IsCompleted.ShouldBeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetRunningAsync_ReturnsOnlyCurrentSubscriptionRunningSession()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var otherSubscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));

        var mine = new FocusSession(subscriptionId);
        var other = new FocusSession(otherSubscriptionId);

        await using (var db = await factory.CreateDbContextAsync())
        {
            db.FocusSessions.AddRange(mine, other);
            await db.SaveChangesAsync();
        }

        // Act
        var sut = new FocusSessionRepository(NullLogger<FocusSessionRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var running = await sut.GetRunningAsync();

        // Assert
        running.ShouldNotBeNull();
        running.Id.ShouldBe(mine.Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetRecentAsync_UsesFallbackTakeForNonPositiveInput()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));

        await using (var db = await factory.CreateDbContextAsync())
        {
            for (var i = 0; i < 25; i++)
            {
                db.FocusSessions.Add(new FocusSession(subscriptionId));
            }

            await db.SaveChangesAsync();
        }

        // Act
        var sut = new FocusSessionRepository(NullLogger<FocusSessionRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var recent = await sut.GetRecentAsync(0);

        // Assert
        recent.Count.ShouldBe(20);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAndUpdate_Null_ThrowArgumentNullException()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();

        // Act
        var sut = new FocusSessionRepository(
            NullLogger<FocusSessionRepository>.Instance,
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(subscriptionId));

        // Assert
        await Should.ThrowAsync<ArgumentNullException>(() => sut.AddAsync(null!));
        await Should.ThrowAsync<ArgumentNullException>(() => sut.UpdateAsync(null!));
    }
}


