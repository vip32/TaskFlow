using Injectio.Attributes;
using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;
using DomainTaskStatus = TaskFlow.Domain.TaskStatus;

namespace TaskFlow.Application;

/// <summary>
/// Default application orchestrator for task use cases.
/// </summary>
[RegisterScoped(ServiceType = typeof(ITaskOrchestrator))]
public sealed class TaskOrchestrator : ITaskOrchestrator
{
    private readonly ITaskRepository taskRepository;
    private readonly ITaskHistoryRepository taskHistoryRepository;
    private readonly ICurrentSubscriptionAccessor currentSubscriptionAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskOrchestrator"/> class.
    /// </summary>
    /// <param name="taskRepository">Task repository.</param>
    /// <param name="taskHistoryRepository">Task history repository.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription accessor.</param>
    public TaskOrchestrator(ITaskRepository taskRepository, ITaskHistoryRepository taskHistoryRepository, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.taskRepository = taskRepository;
        this.taskHistoryRepository = taskHistoryRepository;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public global::System.Threading.Tasks.Task<List<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return this.taskRepository.GetByProjectIdAsync(projectId, cancellationToken);
    }

    /// <inheritdoc/>
    public global::System.Threading.Tasks.Task<List<DomainTask>> SearchAsync(Guid projectId, string query, CancellationToken cancellationToken = default)
    {
        return this.taskRepository.SearchAsync(query, projectId, cancellationToken);
    }

    /// <inheritdoc/>
    public global::System.Threading.Tasks.Task<List<string>> GetNameSuggestionsAsync(string prefix, bool isSubTaskName, int take = 20, CancellationToken cancellationToken = default)
    {
        return this.taskHistoryRepository.GetSuggestionsAsync(prefix, isSubTaskName, take, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> CreateAsync(Guid projectId, string title, TaskPriority priority, string note, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var task = new DomainTask(subscriptionId, title, projectId);
        task.SetPriority(priority);
        task.UpdateNote(note);
        var created = await this.taskRepository.AddAsync(task, cancellationToken);
        await this.taskHistoryRepository.RegisterUsageAsync(created.Title, false, cancellationToken);
        return created;
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> CreateUnassignedAsync(string title, TaskPriority priority, string note, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var task = new DomainTask(subscriptionId, title, Guid.Empty);
        task.SetPriority(priority);
        task.UpdateNote(note);
        var created = await this.taskRepository.AddAsync(task, cancellationToken);
        await this.taskHistoryRepository.RegisterUsageAsync(created.Title, false, cancellationToken);
        return created;
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> UpdateTitleAsync(Guid taskId, string newTitle, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.UpdateTitle(newTitle);
        var updated = await this.taskRepository.UpdateAsync(task, cancellationToken);
        await this.taskHistoryRepository.RegisterUsageAsync(updated.Title, updated.ParentTaskId != Guid.Empty, cancellationToken);
        return updated;
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> UpdateNoteAsync(Guid taskId, string newNote, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.UpdateNote(newNote);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> SetPriorityAsync(Guid taskId, TaskPriority priority, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.SetPriority(priority);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> SetStatusAsync(Guid taskId, DomainTaskStatus status, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.SetStatus(status);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> ToggleFocusAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.ToggleFocus();
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> MoveToProjectAsync(Guid taskId, Guid newProjectId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.MoveToProject(newProjectId);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> UnassignFromProjectAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.UnassignFromProject();
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> SetDueDateAsync(Guid taskId, DateOnly dueDateLocal, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.SetDueDate(dueDateLocal);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> SetDueDateTimeAsync(Guid taskId, DateOnly dueDateLocal, TimeOnly dueTimeLocal, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        var timeZone = GetSubscriptionTimeZone();
        task.SetDueDateTime(dueDateLocal, dueTimeLocal, timeZone);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> ClearDueDateAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.ClearDueDate();
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> ToggleTodayMarkAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.ToggleTodayMark();
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> AddRelativeReminderAsync(Guid taskId, int minutesBefore, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.AddRelativeReminder(minutesBefore);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> AddDateOnlyReminderAsync(Guid taskId, TimeOnly fallbackLocalTime, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        var timeZone = GetSubscriptionTimeZone();
        task.AddDateOnlyReminder(fallbackLocalTime, timeZone);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> RemoveReminderAsync(Guid taskId, Guid reminderId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.RemoveReminder(reminderId);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public global::System.Threading.Tasks.Task<List<DomainTask>> GetRecentAsync(int days = 7, CancellationToken cancellationToken = default)
    {
        return this.taskRepository.GetRecentAsync(days, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<List<DomainTask>> GetTodayAsync(CancellationToken cancellationToken = default)
    {
        var timeZone = GetSubscriptionTimeZone();
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));

        var dueToday = await this.taskRepository.GetDueOnDateAsync(todayLocal, cancellationToken);
        var allTasks = await this.taskRepository.GetAllAsync(cancellationToken);
        var markedToday = allTasks.Where(x => x.IsMarkedForToday);

        return dueToday
            .Concat(markedToday)
            .GroupBy(x => x.Id)
            .Select(group => group.First())
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.DueTimeLocal)
            .ThenByDescending(x => x.CreatedAt)
            .ToList();
    }

    /// <inheritdoc/>
    public global::System.Threading.Tasks.Task<List<DomainTask>> GetThisWeekAsync(CancellationToken cancellationToken = default)
    {
        var timeZone = GetSubscriptionTimeZone();
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        var daysUntilSunday = DayOfWeek.Sunday - todayLocal.DayOfWeek;
        var endOfWeekLocal = todayLocal.AddDays(daysUntilSunday);

        return this.taskRepository.GetDueInRangeAsync(todayLocal.AddDays(1), endOfWeekLocal, cancellationToken);
    }

    /// <inheritdoc/>
    public global::System.Threading.Tasks.Task<List<DomainTask>> GetUpcomingAsync(CancellationToken cancellationToken = default)
    {
        var timeZone = GetSubscriptionTimeZone();
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        var daysUntilSunday = DayOfWeek.Sunday - todayLocal.DayOfWeek;
        var endOfWeekLocal = todayLocal.AddDays(daysUntilSunday);

        return this.taskRepository.GetDueAfterDateAsync(endOfWeekLocal, cancellationToken);
    }

    /// <inheritdoc/>
    public global::System.Threading.Tasks.Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return this.taskRepository.DeleteAsync(taskId, cancellationToken);
    }

    private TimeZoneInfo GetSubscriptionTimeZone()
    {
        var subscription = this.currentSubscriptionAccessor.GetCurrentSubscription();
        return TimeZoneInfo.FindSystemTimeZoneById(subscription.TimeZoneId);
    }
}
