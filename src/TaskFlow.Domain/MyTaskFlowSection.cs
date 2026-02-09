namespace TaskFlow.Domain;

/// <summary>
/// Represents a user-visible section in My Task Flow.
/// </summary>
public class MyTaskFlowSection
{
    private readonly List<MyTaskFlowSectionTask> manualTasks = [];

    /// <summary>
    /// Gets owning subscription identifier.
    /// </summary>
    public Guid SubscriptionId { get; private set; }

    /// <summary>
    /// Gets section identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets section display name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets sort order in sidebar/list.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this is a built-in system section.
    /// </summary>
    public bool IsSystemSection { get; private set; }

    /// <summary>
    /// Gets due bucket filter.
    /// </summary>
    public TaskFlowDueBucket DueBucket { get; private set; }

    /// <summary>
    /// Gets a value indicating whether assigned tasks are included in rule evaluation.
    /// </summary>
    public bool IncludeAssignedTasks { get; private set; }

    /// <summary>
    /// Gets a value indicating whether unassigned tasks are included in rule evaluation.
    /// </summary>
    public bool IncludeUnassignedTasks { get; private set; }

    /// <summary>
    /// Gets a value indicating whether done tasks are shown in rule evaluation.
    /// </summary>
    public bool IncludeDoneTasks { get; private set; }

    /// <summary>
    /// Gets a value indicating whether cancelled tasks are shown in rule evaluation.
    /// </summary>
    public bool IncludeCancelledTasks { get; private set; }

    /// <summary>
    /// Gets manually curated tasks in this section.
    /// </summary>
    public IReadOnlyCollection<MyTaskFlowSectionTask> ManualTasks => this.manualTasks.AsReadOnly();

    private MyTaskFlowSection()
    {
        this.Name = string.Empty;
    }

    /// <summary>
    /// Initializes a custom My Task Flow section.
    /// </summary>
    /// <param name="subscriptionId">Owning subscription identifier.</param>
    /// <param name="name">Section name.</param>
    /// <param name="sortOrder">Section sort order.</param>
    public MyTaskFlowSection(Guid subscriptionId, string name, int sortOrder)
    {
        if (subscriptionId == Guid.Empty)
        {
            throw new ArgumentException("Subscription id cannot be empty.", nameof(subscriptionId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Section name cannot be empty.", nameof(name));
        }

        this.SubscriptionId = subscriptionId;
        this.Id = Guid.NewGuid();
        this.Name = name.Trim();
        this.SortOrder = sortOrder;
        this.IsSystemSection = false;
        this.DueBucket = TaskFlowDueBucket.Any;
        this.IncludeAssignedTasks = true;
        this.IncludeUnassignedTasks = true;
        this.IncludeDoneTasks = false;
        this.IncludeCancelledTasks = false;
    }

    /// <summary>
    /// Creates a built-in system section.
    /// </summary>
    /// <param name="subscriptionId">Owning subscription identifier.</param>
    /// <param name="name">Section name.</param>
    /// <param name="sortOrder">Section sort order.</param>
    /// <param name="dueBucket">Built-in due bucket.</param>
    /// <returns>Created section.</returns>
    public static MyTaskFlowSection CreateSystem(Guid subscriptionId, string name, int sortOrder, TaskFlowDueBucket dueBucket)
    {
        var section = new MyTaskFlowSection(subscriptionId, name, sortOrder)
        {
            IsSystemSection = true,
            DueBucket = dueBucket,
        };

        if (dueBucket == TaskFlowDueBucket.Recent)
        {
            section.IncludeAssignedTasks = true;
            section.IncludeUnassignedTasks = true;
        }

        return section;
    }

    /// <summary>
    /// Renames the section.
    /// </summary>
    /// <param name="newName">New section name.</param>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Section name cannot be empty.", nameof(newName));
        }

        this.Name = newName.Trim();
    }

    /// <summary>
    /// Changes section sort order.
    /// </summary>
    /// <param name="sortOrder">Target sort order.</param>
    public void Reorder(int sortOrder)
    {
        this.SortOrder = sortOrder;
    }

    /// <summary>
    /// Updates rule-based filter settings.
    /// </summary>
    /// <param name="dueBucket">Due bucket filter.</param>
    /// <param name="includeAssignedTasks">Whether assigned tasks are included.</param>
    /// <param name="includeUnassignedTasks">Whether unassigned tasks are included.</param>
    /// <param name="includeDoneTasks">Whether done tasks are included.</param>
    /// <param name="includeCancelledTasks">Whether cancelled tasks are included.</param>
    public void UpdateRule(TaskFlowDueBucket dueBucket, bool includeAssignedTasks, bool includeUnassignedTasks, bool includeDoneTasks, bool includeCancelledTasks)
    {
        this.DueBucket = dueBucket;
        this.IncludeAssignedTasks = includeAssignedTasks;
        this.IncludeUnassignedTasks = includeUnassignedTasks;
        this.IncludeDoneTasks = includeDoneTasks;
        this.IncludeCancelledTasks = includeCancelledTasks;
    }

    /// <summary>
    /// Manually includes a task in this section.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    public void IncludeTask(Guid taskId)
    {
        if (taskId == Guid.Empty)
        {
            throw new ArgumentException("Task id cannot be empty.", nameof(taskId));
        }

        if (this.manualTasks.Any(x => x.TaskId == taskId))
        {
            return;
        }

        this.manualTasks.Add(new MyTaskFlowSectionTask(this.Id, taskId));
    }

    /// <summary>
    /// Removes a manually included task from this section.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    public void RemoveTask(Guid taskId)
    {
        this.manualTasks.RemoveAll(x => x.TaskId == taskId);
    }

    /// <summary>
    /// Evaluates whether a task belongs to this section.
    /// </summary>
    /// <param name="task">Task to evaluate.</param>
    /// <param name="todayLocal">Current local date.</param>
    /// <param name="endOfWeekLocal">Current week end date.</param>
    /// <param name="nowUtc">Current UTC instant.</param>
    /// <param name="timeZone">Subscription timezone.</param>
    /// <returns><c>true</c> when task is part of the section.</returns>
    public bool Matches(Task task, DateOnly todayLocal, DateOnly endOfWeekLocal, DateTime nowUtc, TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(timeZone);

        if (this.manualTasks.Any(x => x.TaskId == task.Id))
        {
            return true;
        }

        var isAssigned = task.ProjectId.HasValue;
        if ((isAssigned && !this.IncludeAssignedTasks) || (!isAssigned && !this.IncludeUnassignedTasks))
        {
            return false;
        }

        if (task.Status == TaskStatus.Done && !this.IncludeDoneTasks)
        {
            return false;
        }

        if (task.Status == TaskStatus.Cancelled && !this.IncludeCancelledTasks)
        {
            return false;
        }

        return this.DueBucket switch
        {
            TaskFlowDueBucket.Any => true,
            TaskFlowDueBucket.Today => task.IsMarkedForToday || (task.DueDateLocal.HasValue && task.DueDateLocal.Value == todayLocal),
            TaskFlowDueBucket.ThisWeek => task.DueDateLocal.HasValue && task.DueDateLocal.Value > todayLocal && task.DueDateLocal.Value <= endOfWeekLocal,
            TaskFlowDueBucket.Upcoming => task.DueDateLocal.HasValue && task.DueDateLocal.Value > endOfWeekLocal,
            TaskFlowDueBucket.NoDueDate => !task.DueDateLocal.HasValue,
            TaskFlowDueBucket.Recent => IsRecent(task, nowUtc, timeZone),
            _ => false,
        };
    }

    private static bool IsRecent(Task task, DateTime nowUtc, TimeZoneInfo timeZone)
    {
        var taskLocalDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(task.CreatedAt, timeZone));
        var nowLocalDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timeZone));
        return taskLocalDate >= nowLocalDate.AddDays(-7);
    }
}
