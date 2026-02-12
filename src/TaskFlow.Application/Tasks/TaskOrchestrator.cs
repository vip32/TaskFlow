using Microsoft.Extensions.Logging;
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
    private readonly ILogger<TaskOrchestrator> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskOrchestrator"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="taskRepository">Task repository.</param>
    /// <param name="taskHistoryRepository">Task history repository.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription accessor.</param>
    public TaskOrchestrator(
        ILogger<TaskOrchestrator> logger,
        ITaskRepository taskRepository,
        ITaskHistoryRepository taskHistoryRepository,
        ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.logger = logger;
        this.taskRepository = taskRepository;
        this.taskHistoryRepository = taskHistoryRepository;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public Task<List<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("Task - GetByProjectId: fetching tasks for project {ProjectId}", projectId);
        return this.taskRepository.GetByProjectIdAsync(projectId, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<DomainTask>> GetSubTasksAsync(Guid parentTaskId, CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("Task - GetSubTasks: fetching subtasks for parent task {ParentTaskId}", parentTaskId);
        return this.taskRepository.GetSubTasksAsync(parentTaskId, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<DomainTask>> SearchAsync(Guid projectId, string query, CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("Task - Search: searching tasks in project {ProjectId} with query length {QueryLength}", projectId, query?.Length ?? 0);
        return this.taskRepository.SearchAsync(query, projectId, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<string>> GetNameSuggestionsAsync(string prefix, bool isSubTaskName, int take = 20, CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("Task - GetNameSuggestions: fetching suggestions. IsSubTaskName={IsSubTaskName}, PrefixLength={PrefixLength}, Take={Take}", isSubTaskName, prefix?.Length ?? 0, take);
        return this.taskHistoryRepository.GetSuggestionsAsync(prefix, isSubTaskName, take, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> CreateAsync(Guid projectId, string title, TaskPriority priority, string note, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        this.logger.LogInformation("Task - Create: creating task for project {ProjectId} in subscription {SubscriptionId}. TitleLength={TitleLength}, Priority={Priority}", projectId, subscriptionId, title.Length, priority);
        var task = new DomainTask(subscriptionId, title, projectId);
        var nextSortOrder = await this.taskRepository.GetNextSortOrderAsync(projectId, null, cancellationToken);
        task.SetSortOrder(nextSortOrder);
        task.SetPriority(priority);
        task.UpdateNote(note);
        var created = await this.taskRepository.AddAsync(task, cancellationToken);
        await this.taskHistoryRepository.RegisterUsageAsync(created.Title, false, cancellationToken);
        this.logger.LogInformation("Task - Create: created task {TaskId} in project {ProjectId}", created.Id, projectId);
        return created;
    }

    /// <inheritdoc/>
    public async Task<DomainTask> CreateUnassignedAsync(string title, TaskPriority priority, string note, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        this.logger.LogInformation("Task - CreateUnassigned: creating unassigned task in subscription {SubscriptionId}. TitleLength={TitleLength}, Priority={Priority}", subscriptionId, title.Length, priority);
        var task = new DomainTask(subscriptionId, title, null);
        var nextSortOrder = await this.taskRepository.GetNextSortOrderAsync(null, null, cancellationToken);
        task.SetSortOrder(nextSortOrder);
        task.SetPriority(priority);
        task.UpdateNote(note);
        var created = await this.taskRepository.AddAsync(task, cancellationToken);
        await this.taskHistoryRepository.RegisterUsageAsync(created.Title, false, cancellationToken);
        this.logger.LogInformation("Task - CreateUnassigned: created unassigned task {TaskId}", created.Id);
        return created;
    }

    /// <inheritdoc/>
    public async Task<DomainTask> CreateSubTaskAsync(Guid parentTaskId, string title, TaskPriority priority, string note, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - CreateSubTask: creating subtask for parent task {ParentTaskId}. TitleLength={TitleLength}, Priority={Priority}", parentTaskId, title.Length, priority);
        var parent = await this.taskRepository.GetByIdAsync(parentTaskId, cancellationToken);
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var subTask = new DomainTask(subscriptionId, title, parent.ProjectId);
        parent.AddSubTask(subTask);
        subTask.SetPriority(priority);
        subTask.UpdateNote(note);
        var created = await this.taskRepository.AddAsync(subTask, cancellationToken);
        await this.taskHistoryRepository.RegisterUsageAsync(created.Title, true, cancellationToken);
        this.logger.LogInformation("Task - CreateSubTask: created subtask {TaskId} under parent task {ParentTaskId}", created.Id, parentTaskId);
        return created;
    }

    /// <inheritdoc/>
    public async Task<DomainTask> UpdateTitleAsync(Guid taskId, string newTitle, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - UpdateTitle: updating title for task {TaskId}. NewTitleLength={TitleLength}", taskId, newTitle.Length);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.UpdateTitle(newTitle);
        var updated = await this.taskRepository.UpdateAsync(task, cancellationToken);
        await this.taskHistoryRepository.RegisterUsageAsync(updated.Title, updated.ParentTaskId.HasValue, cancellationToken);
        return updated;
    }

    /// <inheritdoc/>
    public async Task<DomainTask> UpdateNoteAsync(Guid taskId, string newNote, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - UpdateNote: updating note for task {TaskId}. NoteLength={NoteLength}", taskId, newNote?.Length ?? 0);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.UpdateNote(newNote);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> SetCompletedAsync(Guid taskId, bool isCompleted, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - SetCompleted: setting completion {IsCompleted} for task {TaskId}", isCompleted, taskId);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (isCompleted)
        {
            task.Complete();
        }
        else
        {
            task.Uncomplete();
        }

        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> SetPriorityAsync(Guid taskId, TaskPriority priority, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - SetPriority: setting priority {Priority} for task {TaskId}", priority, taskId);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.SetPriority(priority);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> SetStatusAsync(Guid taskId, DomainTaskStatus status, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - SetStatus: setting status {Status} for task {TaskId}", status, taskId);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.SetStatus(status);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> ToggleFocusAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - ToggleFocus: toggling focus flag for task {TaskId}", taskId);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.ToggleFocus();
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> ToggleImportantAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - ToggleImportant: toggling important flag for task {TaskId}", taskId);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.ToggleImportant();
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> MoveToProjectAsync(Guid taskId, Guid newProjectId, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - MoveToProject: moving task {TaskId} to project {ProjectId}", taskId, newProjectId);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task.ParentTaskId.HasValue)
        {
            this.logger.LogWarning("Task - MoveToProject: cannot move subtask {TaskId} directly. ParentTaskId={ParentTaskId}", taskId, task.ParentTaskId);
            throw new InvalidOperationException("Subtasks inherit parent project assignment and cannot be moved directly.");
        }

        var nextSortOrder = await this.taskRepository.GetNextSortOrderAsync(newProjectId, null, cancellationToken);
        task.MoveToProject(newProjectId);
        task.SetSortOrder(nextSortOrder);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> ReorderProjectTasksAsync(Guid projectId, IReadOnlyList<Guid> orderedTaskIds, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - ReorderProjectTasks: reordering tasks for project {ProjectId}. RequestedOrderCount={OrderCount}", projectId, orderedTaskIds?.Count ?? 0);
        var tasks = await this.taskRepository.GetByProjectIdAsync(projectId, cancellationToken);
        return await ReorderAsync(tasks, orderedTaskIds, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> ReorderSubTasksAsync(Guid parentTaskId, IReadOnlyList<Guid> orderedTaskIds, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - ReorderSubTasks: reordering subtasks for parent task {ParentTaskId}. RequestedOrderCount={OrderCount}", parentTaskId, orderedTaskIds?.Count ?? 0);
        var tasks = await this.taskRepository.GetSubTasksAsync(parentTaskId, cancellationToken);
        return await ReorderAsync(tasks, orderedTaskIds, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> UnassignFromProjectAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - UnassignFromProject: unassigning task {TaskId} from project", taskId);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task.ParentTaskId.HasValue)
        {
            this.logger.LogWarning("Task - UnassignFromProject: cannot unassign subtask {TaskId} directly. ParentTaskId={ParentTaskId}", taskId, task.ParentTaskId);
            throw new InvalidOperationException("Subtasks inherit parent project assignment and cannot be unassigned directly.");
        }

        var nextSortOrder = await this.taskRepository.GetNextSortOrderAsync(null, null, cancellationToken);
        task.UnassignFromProject();
        task.SetSortOrder(nextSortOrder);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> SetDueDateAsync(Guid taskId, DateOnly dueDateLocal, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - SetDueDate: setting due date {DueDateLocal} for task {TaskId}", dueDateLocal, taskId);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.SetDueDate(dueDateLocal);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> SetDueDateTimeAsync(Guid taskId, DateOnly dueDateLocal, TimeOnly dueTimeLocal, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - SetDueDateTime: setting due datetime for task {TaskId}. DueDateLocal={DueDateLocal}, DueTimeLocal={DueTimeLocal}", taskId, dueDateLocal, dueTimeLocal);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        var timeZone = GetSubscriptionTimeZone();
        task.SetDueDateTime(dueDateLocal, dueTimeLocal, timeZone);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> ClearDueDateAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - ClearDueDate: clearing due date for task {TaskId}", taskId);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.ClearDueDate();
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> ToggleTodayMarkAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - ToggleTodayMark: toggling today mark for task {TaskId}", taskId);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.ToggleTodayMark();
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> AddRelativeReminderAsync(Guid taskId, int minutesBefore, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - AddRelativeReminder: adding relative reminder for task {TaskId}. MinutesBefore={MinutesBefore}", taskId, minutesBefore);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.AddRelativeReminder(minutesBefore);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> AddDateOnlyReminderAsync(Guid taskId, TimeOnly fallbackLocalTime, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - AddDateOnlyReminder: adding date-only reminder for task {TaskId}. FallbackLocalTime={FallbackLocalTime}", taskId, fallbackLocalTime);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        var timeZone = GetSubscriptionTimeZone();
        task.AddDateOnlyReminder(fallbackLocalTime, timeZone);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DomainTask> RemoveReminderAsync(Guid taskId, Guid reminderId, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - RemoveReminder: removing reminder {ReminderId} from task {TaskId}", reminderId, taskId);
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.RemoveReminder(reminderId);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<DomainTask>> GetRecentAsync(int days = 7, CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("Task - GetRecent: fetching recent tasks for last {Days} day(s)", days);
        return this.taskRepository.GetRecentAsync(days, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DomainTask>> GetTodayAsync(CancellationToken cancellationToken = default)
    {
        var timeZone = GetSubscriptionTimeZone();
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        this.logger.LogDebug("Task - GetToday: fetching today tasks for local date {TodayLocal} in timezone {TimeZoneId}", todayLocal, timeZone.Id);

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
        this.logger.LogDebug("Task - GetThisWeek: fetching this-week tasks. StartDate={StartDate}, EndDate={EndDate}, TimeZone={TimeZoneId}", todayLocal.AddDays(1), endOfWeekLocal, timeZone.Id);

        return this.taskRepository.GetDueInRangeAsync(todayLocal.AddDays(1), endOfWeekLocal, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<List<DomainTask>> GetUpcomingAsync(CancellationToken cancellationToken = default)
    {
        var timeZone = GetSubscriptionTimeZone();
        var todayLocal = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone));
        var daysUntilSunday = DayOfWeek.Sunday - todayLocal.DayOfWeek;
        var endOfWeekLocal = todayLocal.AddDays(daysUntilSunday);
        this.logger.LogDebug("Task - GetUpcoming: fetching upcoming tasks after {EndOfWeekLocal} in timezone {TimeZoneId}", endOfWeekLocal, timeZone.Id);

        return this.taskRepository.GetDueAfterDateAsync(endOfWeekLocal, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Task - Delete: deleting task {TaskId}", taskId);
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
            this.logger.LogDebug("Task - Reorder: requested reorder without ids. Returning existing order for {TaskCount} tasks", tasks.Count);
            return tasks.OrderBy(task => task.SortOrder).ThenBy(task => task.CreatedAt).ToList();
        }

        var duplicate = orderedTaskIds.GroupBy(id => id).FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            this.logger.LogWarning("Task - Reorder: duplicate task id {TaskId} detected in reorder request", duplicate.Key);
            throw new ArgumentException($"Task id '{duplicate.Key}' appears more than once in requested order.", nameof(orderedTaskIds));
        }

        var byId = tasks.ToDictionary(task => task.Id);
        foreach (var taskId in orderedTaskIds)
        {
            if (!byId.ContainsKey(taskId))
            {
                this.logger.LogWarning("Task - Reorder: task id {TaskId} was not part of reorder target list", taskId);
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

        this.logger.LogInformation("Task - Reorder: persisting reordered tasks. TaskCount={TaskCount}", ordered.Count);
        return await this.taskRepository.UpdateRangeAsync(ordered, cancellationToken);
    }
}
