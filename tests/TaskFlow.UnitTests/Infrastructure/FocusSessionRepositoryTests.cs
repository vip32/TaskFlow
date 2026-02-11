using Microsoft.Extensions.Logging.Abstractions;
using TaskFlow.Domain;
using TaskFlow.Infrastructure.Repositories;

namespace TaskFlow.UnitTests.Infrastructure;

public class FocusSessionRepositoryTests
{
    [Fact]
    public async System.Threading.Tasks.Task AddAndUpdateAsync_PersistSession()
    {
        var subscriptionId = Guid.NewGuid();
        var factory = new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N"));
        var repository = new FocusSessionRepository(NullLogger<FocusSessionRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var session = new FocusSession(subscriptionId, Guid.NewGuid());
        await repository.AddAsync(session);

        session.End();
        var updated = await repository.UpdateAsync(session);

        Assert.True(updated.IsCompleted);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetRunningAsync_ReturnsOnlyCurrentSubscriptionRunningSession()
    {
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

        var repository = new FocusSessionRepository(NullLogger<FocusSessionRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var running = await repository.GetRunningAsync();

        Assert.NotNull(running);
        Assert.Equal(mine.Id, running.Id);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetRecentAsync_UsesFallbackTakeForNonPositiveInput()
    {
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

        var repository = new FocusSessionRepository(NullLogger<FocusSessionRepository>.Instance, factory, new TestCurrentSubscriptionAccessor(subscriptionId));

        var recent = await repository.GetRecentAsync(0);

        Assert.Equal(20, recent.Count);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAndUpdate_Null_ThrowArgumentNullException()
    {
        var subscriptionId = Guid.NewGuid();
        var repository = new FocusSessionRepository(
            NullLogger<FocusSessionRepository>.Instance,
            new InMemoryAppDbContextFactory(Guid.NewGuid().ToString("N")),
            new TestCurrentSubscriptionAccessor(subscriptionId));

        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.UpdateAsync(null!));
    }
}
