namespace TaskFlow.Presentation.Services;

public sealed class FocusTimerStateService : IDisposable
{
    public event Action Changed;
    public event Action Completed;

    public int SelectedTimerMinutes { get; private set; } = 30;
    public TimeSpan TimerRemaining { get; private set; } = TimeSpan.FromMinutes(30);
    public bool TimerRunning { get; private set; }

    private DateTime? runningUntilUtc;
    private PeriodicTimer timer;
    private CancellationTokenSource timerCts;

    public Task SetPresetAsync(int minutes)
    {
        this.SelectedTimerMinutes = minutes;
        if (!this.TimerRunning)
        {
            this.TimerRemaining = TimeSpan.FromMinutes(minutes);
        }

        NotifyChanged();
        return Task.CompletedTask;
    }

    public Task StartAsync()
    {
        if (this.TimerRunning)
        {
            return Task.CompletedTask;
        }

        if (this.TimerRemaining <= TimeSpan.Zero)
        {
            this.TimerRemaining = TimeSpan.FromMinutes(this.SelectedTimerMinutes);
        }

        this.TimerRunning = true;
        this.runningUntilUtc = DateTime.UtcNow.Add(this.TimerRemaining);
        this.timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        this.timerCts = new CancellationTokenSource();
        NotifyChanged();
        _ = RunTimerAsync(this.timer, this.timerCts.Token);
        return Task.CompletedTask;
    }

    public Task PauseAsync()
    {
        if (!this.TimerRunning)
        {
            return Task.CompletedTask;
        }

        this.TimerRunning = false;
        this.TimerRemaining = CalculateRemaining();
        this.runningUntilUtc = null;
        this.timerCts?.Cancel();
        this.timer?.Dispose();
        this.timer = null;
        NotifyChanged();
        return Task.CompletedTask;
    }

    public async Task ResetAsync()
    {
        await PauseAsync();
        this.TimerRemaining = TimeSpan.FromMinutes(this.SelectedTimerMinutes);
        NotifyChanged();
    }

    private async Task RunTimerAsync(PeriodicTimer localTimer, CancellationToken cancellationToken)
    {
        try
        {
            while (await localTimer.WaitForNextTickAsync(cancellationToken))
            {
                this.TimerRemaining = CalculateRemaining();
                NotifyChanged();

                if (this.TimerRemaining > TimeSpan.Zero)
                {
                    continue;
                }

                this.TimerRunning = false;
                this.runningUntilUtc = null;
                this.timerCts?.Cancel();
                this.timer?.Dispose();
                this.timer = null;
                NotifyChanged();
                this.Completed?.Invoke();
                return;
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private TimeSpan CalculateRemaining()
    {
        if (!this.runningUntilUtc.HasValue)
        {
            return this.TimerRemaining;
        }

        var remaining = this.runningUntilUtc.Value - DateTime.UtcNow;
        return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }

    private void NotifyChanged()
    {
        this.Changed?.Invoke();
    }

    public void Dispose()
    {
        this.timerCts?.Cancel();
        this.timer?.Dispose();
        this.timerCts?.Dispose();
    }
}
