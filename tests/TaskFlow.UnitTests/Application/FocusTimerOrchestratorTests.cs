using Microsoft.Extensions.Logging.Abstractions;
using TaskFlow.Application;
using TaskFlow.Domain;

namespace TaskFlow.UnitTests.Application;

public class FocusTimerOrchestratorTests
{
    [Fact]
    public async System.Threading.Tasks.Task StartAsync_WithRunningSession_EndsOldAndCreatesNew()
    {
        var subscription = CreateSubscription();
        var running = new FocusSession(subscription.Id, Guid.NewGuid());
        var repository = new FakeFocusSessionRepository(running);
        var sut = new FocusTimerOrchestrator(NullLogger<FocusTimerOrchestrator>.Instance, repository, new FakeCurrentSubscriptionAccessor(subscription));

        var created = await sut.StartAsync(Guid.NewGuid());

        Assert.True(running.IsCompleted);
        Assert.Equal(1, repository.UpdateCallCount);
        Assert.Equal(1, repository.AddCallCount);
        Assert.Equal(subscription.Id, created.SubscriptionId);
    }

    [Fact]
    public async System.Threading.Tasks.Task StartAsync_EmptyTaskId_CreatesSessionWithoutTask()
    {
        var subscription = CreateSubscription();
        var repository = new FakeFocusSessionRepository();
        var sut = new FocusTimerOrchestrator(NullLogger<FocusTimerOrchestrator>.Instance, repository, new FakeCurrentSubscriptionAccessor(subscription));

        var created = await sut.StartAsync(Guid.Empty);

        Assert.Equal(Guid.Empty, created.TaskId);
    }

    [Fact]
    public async System.Threading.Tasks.Task EndCurrentAsync_NoRunning_ReturnsNull()
    {
        var subscription = CreateSubscription();
        var repository = new FakeFocusSessionRepository();
        var sut = new FocusTimerOrchestrator(NullLogger<FocusTimerOrchestrator>.Instance, repository, new FakeCurrentSubscriptionAccessor(subscription));

        var ended = await sut.EndCurrentAsync();

        Assert.Null(ended);
    }

    [Fact]
    public async System.Threading.Tasks.Task EndCurrentAsync_Running_EndsAndUpdates()
    {
        var subscription = CreateSubscription();
        var running = new FocusSession(subscription.Id);
        var repository = new FakeFocusSessionRepository(running);
        var sut = new FocusTimerOrchestrator(NullLogger<FocusTimerOrchestrator>.Instance, repository, new FakeCurrentSubscriptionAccessor(subscription));

        var ended = await sut.EndCurrentAsync();

        Assert.NotNull(ended);
        Assert.True(ended.IsCompleted);
        Assert.Equal(1, repository.UpdateCallCount);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetRecentAsync_ForwardsToRepository()
    {
        var subscription = CreateSubscription();
        var repository = new FakeFocusSessionRepository();
        repository.Recent.Add(new FocusSession(subscription.Id));
        var sut = new FocusTimerOrchestrator(NullLogger<FocusTimerOrchestrator>.Instance, repository, new FakeCurrentSubscriptionAccessor(subscription));

        var recent = await sut.GetRecentAsync(10);

        Assert.Single(recent);
    }

    private static Subscription CreateSubscription()
    {
        var subscription = new Subscription(Guid.NewGuid(), "Test", SubscriptionTier.Free, true, "Europe/Berlin");
        subscription.AddOpenEndedSchedule(DateOnly.FromDateTime(DateTime.UtcNow));
        return subscription;
    }

    private sealed class FakeCurrentSubscriptionAccessor : ICurrentSubscriptionAccessor
    {
        private readonly Subscription subscription;

        public FakeCurrentSubscriptionAccessor(Subscription subscription)
        {
            this.subscription = subscription;
        }

        public Subscription GetCurrentSubscription() => this.subscription;
    }

    private sealed class FakeFocusSessionRepository : IFocusSessionRepository
    {
        private FocusSession running;

        public FakeFocusSessionRepository(FocusSession running = null)
        {
            this.running = running;
        }

        public int AddCallCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public List<FocusSession> Recent { get; } = [];

        public Task<List<FocusSession>> GetRecentAsync(int take = 50, CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.Recent.Take(take).ToList());
        }

        public Task<FocusSession> GetRunningAsync(CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromResult(this.running);
        }

        public Task<FocusSession> AddAsync(FocusSession session, CancellationToken cancellationToken = default)
        {
            this.AddCallCount++;
            this.Recent.Add(session);
            this.running = session;
            return System.Threading.Tasks.Task.FromResult(session);
        }

        public Task<FocusSession> UpdateAsync(FocusSession session, CancellationToken cancellationToken = default)
        {
            this.UpdateCallCount++;
            if (this.running?.Id == session.Id)
            {
                this.running = session.IsCompleted ? null : session;
            }

            return System.Threading.Tasks.Task.FromResult(session);
        }
    }
}
