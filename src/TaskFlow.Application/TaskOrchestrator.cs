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
    public Task<List<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return this.taskRepository.GetByProjectIdAsync(projectId, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<DomainTask>> GetSubTasksAsync(Guid parentTaskId, CancellationToken cancellationToken = default)
    {
        return this.taskRepository.GetSubTasksAsync(parentTaskId, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<DomainTask>> SearchAsync(Guid projectId, string query, CancellationToken cancellationToken = default)
    {
        return this.taskRepository.SearchAsync(query, projectId, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<string>> GetNameSuggestionsAsync(string prefix, bool isSubTaskName, int take = 20, CancellationToken cancellationToken = default)
    {
        return this.taskHistoryRepository.GetSuggestionsAsync(prefix, isSubTaskName, take, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> CreateAsync(Guid projectId, string title, TaskPriority priority, string note, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var task = new DomainTask(subscriptionId, title, projectId);
        var nextSortOrder = await this.taskRepository.GetNextSortOrderAsync(projectId, Guid.Empty, cancellationToken);
        task.SetSortOrder(nextSortOrder);
        task.SetPriority(priority);
        task.UpdateNote(note);
        var created = await this.taskRepository.AddAsync(task, cancellationToken);
        await this.taskHistoryRepository.RegisterUsageAsync(created.Title, false, cancellationToken);
        return created;
    }

    /// <inheritdoc/>
    public async Task<DomainTask> CreateUnassignedAsync(string title, TaskPriority priority, string note, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var task = new DomainTask(subscriptionId, title, Guid.Empty);
        var nextSortOrder = await this.taskRepository.GetNextSortOrderAsync(Guid.Empty, Guid.Empty, cancellationToken);
        task.SetSortOrder(nextSortOrder);
        task.SetPriority(priority);
        task.UpdateNote(note);
        var created = await this.taskRepository.AddAsync(task, cancellationToken);
        await this.taskHistoryRepository.RegisterUsageAsync(created.Title, false, cancellationToken);
        return created;
    }

    /// <inheritdoc/>
    public async Task<DomainTask> UpdateTitleAsync(Guid taskId, string newTitle, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.UpdateTitle(newTitle);
        var updated = await this.taskRepository.UpdateAsync(task, cancellationToken);
        await this.taskHistoryRepository.RegisterUsageAsync(updated.Title, updated.ParentTaskId != Guid.Empty, cancellationToken);
        return updated;
    }

    /// <inheritdoc/>
    public async Task<DomainTask> UpdateNoteAsync(Guid taskId, string newNote, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.UpdateNote(newNote);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> SetPriorityAsync(Guid taskId, TaskPriority priority, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.SetPriority(priority);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> SetStatusAsync(Guid taskId, DomainTaskStatus status, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.SetStatus(status);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> ToggleFocusAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.ToggleFocus();
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> MoveToProjectAsync(Guid taskId, Guid newProjectId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task.ParentTaskId != Guid.Empty)
        {
            throw new InvalidOperationException("Subtasks inherit parent project assignment and cannot be moved directly.");
        }

        var nextSortOrder = await this.taskRepository.GetNextSortOrderAsync(newProjectId, Guid.Empty, cancellationToken);
        task.MoveToProject(newProjectId);
        task.SetSortOrder(nextSortOrder);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> ReorderProjectTasksAsync(Guid projectId, IReadOnlyList<Guid> orderedTaskIds, CancellationToken cancellationToken = default)
    {
        var tasks = await this.taskRepository.GetByProjectIdAsync(projectId, cancellationToken);
        return await ReorderAsync(tasks, orderedTaskIds, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> ReorderSubTasksAsync(Guid parentTaskId, IReadOnlyList<Guid> orderedTaskIds, CancellationToken cancellationToken = default)
    {
        var tasks = await this.taskRepository.GetSubTasksAsync(parentTaskId, cancellationToken);
        return await ReorderAsync(tasks, orderedTaskIds, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> UnassignFromProjectAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task.ParentTaskId != Guid.Empty)
        {
            throw new InvalidOperationException("Subtasks inherit parent project assignment and cannot be unassigned directly.");
        }

        var nextSortOrder = await this.taskRepository.GetNextSortOrderAsync(Guid.Empty, Guid.Empty, cancellationToken);
        task.UnassignFromProject();
        task.SetSortOrder(nextSortOrder);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> SetDueDateAsync(Guid taskId, DateOnly dueDateLocal, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.SetDueDate(dueDateLocal);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> SetDueDateTimeAsync(Guid taskId, DateOnly dueDateLocal, TimeOnly dueTimeLocal, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        var timeZone = GetSubscriptionTimeZone();
        task.SetDueDateTime(dueDateLocal, dueTimeLocal, timeZone);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> ClearDueDateAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.ClearDueDate();
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> ToggleTodayMarkAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.ToggleTodayMark();
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> AddRelativeReminderAsync(Guid taskId, int minutesBefore, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.AddRelativeReminder(minutesBefore);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> AddDateOnlyReminderAsync(Guid taskId, TimeOnly fallbackLocalTime, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        var timeZone = GetSubscriptionTimeZone();
        task.AddDateOnlyReminder(fallbackLocalTime, timeZone);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> RemoveReminderAsync(Guid taskId, Guid reminderId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.RemoveReminder(reminderId);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<DomainTask>> GetRecentAsync(int days = 7, CancellationToken cancellationToken = default)
    {
        return this.taskRepository.GetRecentAsync(days, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> GetTodayAsync(CancellationToken cancellationToken = default)
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
    public Task<List<DomainTask>> GetThisWeekAsync(CancellationToken cancellationToken = default)
    {
        var timeZone = GetSubscriptionTimeZone();
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        var daysUntilSunday = DayOfWeek.Sunday - todayLocal.DayOfWeek;
        var endOfWeekLocal = todayLocal.AddDays(daysUntilSunday);

        return this.taskRepository.GetDueInRangeAsync(todayLocal.AddDays(1), endOfWeekLocal, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<DomainTask>> GetUpcomingAsync(CancellationToken cancellationToken = default)
    {
        var timeZone = GetSubscriptionTimeZone();
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        var daysUntilSunday = DayOfWeek.Sunday - todayLocal.DayOfWeek;
        var endOfWeekLocal = todayLocal.AddDays(daysUntilSunday);

        return this.taskRepository.GetDueAfterDateAsync(endOfWeekLocal, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return this.taskRepository.DeleteAsync(taskId, cancellationToken);
    }

    private TimeZoneInfo GetSubscriptionTimeZone()
    {
        var subscription = this.currentSubscriptionAccessor.GetCurrentSubscription();
        return TimeZoneInfo.FindSystemTimeZoneById(subscription.TimeZoneId);
    }

    private async Task<List<DomainTask>> ReorderAsync(List<DomainTask> tasks, IReadOnlyList<Guid> orderedTaskIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(orderedTaskIds);

        if (orderedTaskIds.Count == 0)
        {
            return tasks.OrderBy(task => task.SortOrder).ThenBy(task => task.CreatedAt).ToList();
        }

        var duplicate = orderedTaskIds.GroupBy(id => id).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new ArgumentException($"Task id '{duplicate.Key}' appears more than once in requested order.", nameof(orderedTaskIds));
        }

        var byId = tasks.ToDictionary(task => task.Id);
        foreach (var taskId in orderedTaskIds)
        {
            if (!byId.ContainsKey(taskId))
            {
                throw new ArgumentException($"Task id '{taskId}' is not part of the target list.", nameof(orderedTaskIds));
            }
        }

        var ordered = new List<DomainTask>(tasks.Count);
        foreach (var taskId in orderedTaskIds)
        {
            ordered.Add(byId[taskId]);
        }

        var remaining = tasks
            .Where(task => !orderedTaskIds.Contains(task.Id))
            .OrderBy(task => task.SortOrder)
            .ThenBy(task => task.CreatedAt);
        ordered.AddRange(remaining);

        for (var index = 0; index < ordered.Count; index++)
        {
            ordered[index].SetSortOrder(index);
        }

        return await this.taskRepository.UpdateRangeAsync(ordered, cancellationToken);
    }
}
