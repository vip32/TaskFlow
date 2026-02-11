using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.UnitTests.Infrastructure;

internal sealed class InMemoryAppDbContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly DbContextOptions<AppDbContext> options;

    public InMemoryAppDbContextFactory(string databaseName)
    {
        this.options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
    }

    public AppDbContext CreateDbContext()
    {
        return new AppDbContext(this.options);
    }

    public System.Threading.Tasks.Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        return System.Threading.Tasks.Task.FromResult(CreateDbContext());
    }
}

internal sealed class TestCurrentSubscriptionAccessor : ICurrentSubscriptionAccessor
{
    private readonly Subscription subscription;

    public TestCurrentSubscriptionAccessor(Guid subscriptionId)
    {
        this.subscription = new Subscription(subscriptionId, "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        this.subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public Subscription GetCurrentSubscription()
    {
        return this.subscription;
    }
}

