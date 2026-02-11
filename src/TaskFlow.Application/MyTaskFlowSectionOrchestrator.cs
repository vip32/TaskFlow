using Microsoft.Extensions.Logging;
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
    private readonly ILogger<MyTaskFlowSectionOrchestrator> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyTaskFlowSectionOrchestrator"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="sectionRepository">Section repository.</param>
    /// <param name="taskRepository">Task repository.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription accessor.</param>
    public MyTaskFlowSectionOrchestrator(ILogger<MyTaskFlowSectionOrchestrator> logger, IMyTaskFlowSectionRepository sectionRepository, ITaskRepository taskRepository, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.logger = logger;
        this.sectionRepository = sectionRepository;
        this.taskRepository = taskRepository;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public Task<List<MyTaskFlowSection>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("MyTaskFlowSection - GetAll: fetching sections for current subscription");
        return this.sectionRepository.GetAllAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<MyTaskFlowSection> CreateAsync(string name, int sortOrder, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        this.logger.LogInformation("MyTaskFlowSection - Create: creating section in subscription {SubscriptionId}. Name={SectionName}, SortOrder={SortOrder}", subscriptionId, name, sortOrder);
        var section = new MyTaskFlowSection(subscriptionId, name, sortOrder);
        return this.sectionRepository.AddAsync(section, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<MyTaskFlowSection> UpdateRuleAsync(Guid sectionId, TaskFlowDueBucket dueBucket, bool includeAssignedTasks, bool includeUnassignedTasks, bool includeDoneTasks, bool includeCancelledTasks, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("MyTaskFlowSection - UpdateRule: updating rule for section {SectionId}. DueBucket={DueBucket}, IncludeAssigned={IncludeAssigned}, IncludeUnassigned={IncludeUnassigned}, IncludeDone={IncludeDone}, IncludeCancelled={IncludeCancelled}", sectionId, dueBucket, includeAssignedTasks, includeUnassignedTasks, includeDoneTasks, includeCancelledTasks);
        var section = await this.sectionRepository.GetByIdAsync(sectionId, cancellationToken);
        section.UpdateRule(dueBucket, includeAssignedTasks, includeUnassignedTasks, includeDoneTasks, includeCancelledTasks);
        return await this.sectionRepository.UpdateAsync(section, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<MyTaskFlowSection> IncludeTaskAsync(Guid sectionId, Guid taskId, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("MyTaskFlowSection - IncludeTask: including task {TaskId} in section {SectionId}", taskId, sectionId);
        var section = await this.sectionRepository.GetByIdAsync(sectionId, cancellationToken);
        section.IncludeTask(taskId);
        return await this.sectionRepository.UpdateAsync(section, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<MyTaskFlowSection> RemoveTaskAsync(Guid sectionId, Guid taskId, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("MyTaskFlowSection - RemoveTask: removing task {TaskId} from section {SectionId}", taskId, sectionId);
        var section = await this.sectionRepository.GetByIdAsync(sectionId, cancellationToken);
        section.RemoveTask(taskId);
        return await this.sectionRepository.UpdateAsync(section, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<Domain.Task>> GetSectionTasksAsync(Guid sectionId, CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("MyTaskFlowSection - GetSectionTasks: fetching tasks for section {SectionId}", sectionId);
        var section = await this.sectionRepository.GetByIdAsync(sectionId, cancellationToken);
        return await GetSectionTasksAsync(section, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<Domain.Task>> GetSectionTasksAsync(MyTaskFlowSection section, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(section);
        this.logger.LogDebug("MyTaskFlowSection - ResolveTasks: resolving tasks for section {SectionId} ({SectionName})", section.Id, section.Name);

        var allTasks = await this.taskRepository.GetAllAsync(cancellationToken);

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(this.currentSubscriptionAccessor.GetCurrentSubscription().TimeZoneId);
        var nowUtc = DateTime.UtcNow;
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timeZone));
        var daysUntilSunday = DayOfWeek.Sunday - todayLocal.DayOfWeek;
        var endOfWeekLocal = todayLocal.AddDays(daysUntilSunday);

        var tasks = allTasks
            .Where(task => section.Matches(task, todayLocal, endOfWeekLocal, nowUtc, timeZone))
            .OrderBy(task => task.HasDueDate ? 0 : 1)
            .ThenBy(task => task.DueDateLocal)
            .ThenBy(task => task.DueTimeLocal)
            .ThenByDescending(task => task.CreatedAt)
            .ToList();
        this.logger.LogDebug("MyTaskFlowSection - ResolveTasks: resolved {TaskCount} task(s) for section {SectionId}", tasks.Count, section.Id);
        return tasks;
    }
}
