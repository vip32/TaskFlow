using Injectio.Attributes;
using TaskFlow.Domain;

namespace TaskFlow.Application;

/// <summary>
/// Default application orchestrator for My Task Flow sections.
/// </summary>
[RegisterScoped(ServiceType = typeof(IMyTaskFlowSectionOrchestrator))]
public sealed class MyTaskFlowSectionOrchestrator : IMyTaskFlowSectionOrchestrator
{
    private readonly IMyTaskFlowSectionRepository sectionRepository;
    private readonly ITaskRepository taskRepository;
    private readonly ICurrentSubscriptionAccessor currentSubscriptionAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyTaskFlowSectionOrchestrator"/> class.
    /// </summary>
    /// <param name="sectionRepository">Section repository.</param>
    /// <param name="taskRepository">Task repository.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription accessor.</param>
    public MyTaskFlowSectionOrchestrator(IMyTaskFlowSectionRepository sectionRepository, ITaskRepository taskRepository, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.sectionRepository = sectionRepository;
        this.taskRepository = taskRepository;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public Task<List<MyTaskFlowSection>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return this.sectionRepository.GetAllAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<MyTaskFlowSection> CreateAsync(string name, int sortOrder, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var section = new MyTaskFlowSection(subscriptionId, name, sortOrder);
        return this.sectionRepository.AddAsync(section, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<MyTaskFlowSection> UpdateRuleAsync(Guid sectionId, TaskFlowDueBucket dueBucket, bool includeAssignedTasks, bool includeUnassignedTasks, bool includeDoneTasks, bool includeCancelledTasks, CancellationToken cancellationToken = default)
    {
        var section = await this.sectionRepository.GetByIdAsync(sectionId, cancellationToken);
        section.UpdateRule(dueBucket, includeAssignedTasks, includeUnassignedTasks, includeDoneTasks, includeCancelledTasks);
        return await this.sectionRepository.UpdateAsync(section, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<MyTaskFlowSection> IncludeTaskAsync(Guid sectionId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var section = await this.sectionRepository.GetByIdAsync(sectionId, cancellationToken);
        section.IncludeTask(taskId);
        return await this.sectionRepository.UpdateAsync(section, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<MyTaskFlowSection> RemoveTaskAsync(Guid sectionId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var section = await this.sectionRepository.GetByIdAsync(sectionId, cancellationToken);
        section.RemoveTask(taskId);
        return await this.sectionRepository.UpdateAsync(section, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<Domain.Task>> GetSectionTasksAsync(Guid sectionId, CancellationToken cancellationToken = default)
    {
        var section = await this.sectionRepository.GetByIdAsync(sectionId, cancellationToken);
        return await GetSectionTasksAsync(section, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<Domain.Task>> GetSectionTasksAsync(MyTaskFlowSection section, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(section);

        var allTasks = await this.taskRepository.GetAllAsync(cancellationToken);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(this.currentSubscriptionAccessor.GetCurrentSubscription().TimeZoneId);
        var nowUtc = DateTime.UtcNow;
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timeZone));
        var daysUntilSunday = DayOfWeek.Sunday - todayLocal.DayOfWeek;
        var endOfWeekLocal = todayLocal.AddDays(daysUntilSunday);

        return allTasks
            .Where(task => section.Matches(task, todayLocal, endOfWeekLocal, nowUtc, timeZone))
            .OrderBy(task => task.HasDueDate ? 0 : 1)
            .ThenBy(task => task.DueDateLocal)
            .ThenBy(task => task.DueTimeLocal)
            .ThenByDescending(task => task.CreatedAt)
            .ToList();
    }
}
