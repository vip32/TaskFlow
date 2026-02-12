namespace TaskFlow.Domain;

/// <summary>
/// Represents a task aggregate with behavior for completion, hierarchy, and metadata updates.
/// </summary>
public class Task
{
    private const int MAX_TITLE_LENGTH = 500;

    private readonly List<Task> subTasks = [];
    private readonly List<string> tags = [];
    private readonly List<TaskReminder> reminders = [];

    /// <summary>
    /// Gets the subscription identifier that owns this task.
    /// </summary>
    public Guid SubscriptionId { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the task.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the task title.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the optional note content for the task.
    /// </summary>
    public string Note { get; private set; }

    /// <summary>
    /// Gets the priority level of the task.
    /// </summary>
    public TaskPriority Priority { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the task is completed.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the task is currently marked as focused.
    /// </summary>
    public bool IsFocused { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this task is marked as important.
    /// </summary>
    public bool IsImportant { get; private set; }

    /// <summary>
    /// Gets the workflow status of the task.
    /// </summary>
    public TaskStatus Status { get; private set; }

    /// <summary>
    /// Gets the identifier of the project containing this task.
    /// </summary>
    public Guid? ProjectId { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this task is currently unassigned.
    /// </summary>
    public bool IsUnassigned => !this.ProjectId.HasValue;

    /// <summary>
    /// Gets the parent task identifier when this task is a subtask.
    /// </summary>
    public Guid? ParentTaskId { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the task was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the task was completed.
    /// Null indicates not completed.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Gets the persisted order position within either a project task list or sibling subtasks.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Gets a value indicating whether task has a due date.
    /// </summary>
    public bool HasDueDate => this.DueDateLocal.HasValue;

    /// <summary>
    /// Gets local due date in subscription timezone.
    /// Null indicates no due date.
    /// </summary>
    public DateOnly? DueDateLocal { get; private set; }

    /// <summary>
    /// Gets a value indicating whether task has a due time.
    /// </summary>
    public bool HasDueTime => this.DueTimeLocal.HasValue;

    /// <summary>
    /// Gets local due time in subscription timezone.
    /// Null indicates no due time.
    /// </summary>
    public TimeOnly? DueTimeLocal { get; private set; }

    /// <summary>
    /// Gets UTC due instant when due time is set.
    /// Null indicates no due date-time.
    /// </summary>
    public DateTime? DueAtUtc { get; private set; }

    /// <summary>
    /// Gets a value indicating whether task is explicitly marked for today.
    /// </summary>
    public bool IsMarkedForToday { get; private set; }

    /// <summary>
    /// Gets read-only subtasks that belong to this task.
    /// </summary>
    public IReadOnlyCollection<Task> SubTasks => this.subTasks.AsReadOnly();

    /// <summary>
    /// Gets tags assigned to this task.
    /// </summary>
    public IReadOnlyCollection<string> Tags => this.tags.AsReadOnly();

    /// <summary>
    /// Gets reminders configured for this task.
    /// </summary>
    public IReadOnlyCollection<TaskReminder> Reminders => this.reminders.AsReadOnly();

    private Task()
    {
        this.Title = string.Empty;
        this.Note = null;
    }

    /// <summary>
    /// Initializes a new task in a project.
    /// </summary>
    /// <param name="subscriptionId">Owning subscription identifier.</param>
    /// <param name="title">Task title.</param>
    /// <param name="projectId">Owning project identifier. Null means unassigned task.</param>
    public Task(Guid subscriptionId, string title, Guid? projectId)
    {
        if (subscriptionId == Guid.Empty)
        {
            throw new ArgumentException("Subscription id cannot be empty.", nameof(subscriptionId));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title cannot be empty.", nameof(title));
        }

        if (title.Trim().Length > MAX_TITLE_LENGTH)
        {
            throw new ArgumentException($"Task title cannot exceed {MAX_TITLE_LENGTH} characters.", nameof(title));
        }

        if (projectId.HasValue && projectId.Value == Guid.Empty)
        {
            throw new ArgumentException("Project id must be null or a non-empty guid.", nameof(projectId));
        }

        this.SubscriptionId = subscriptionId;
        this.Id = Guid.NewGuid();
        this.Title = title.Trim();
        this.Note = null;
        this.ProjectId = projectId;
        this.ParentTaskId = null;
        this.Priority = TaskPriority.Medium;
        this.Status = TaskStatus.Todo;
        this.IsImportant = false;
        this.CreatedAt = DateTime.UtcNow;
        this.CompletedAt = null;
        this.SortOrder = 0;
        this.DueDateLocal = null;
        this.DueTimeLocal = null;
        this.DueAtUtc = null;
        this.IsMarkedForToday = false;
    }

    /// <summary>
    /// Rehydrates a task from serialized import data.
    /// </summary>
    /// <param name="subscriptionId">Owning subscription identifier.</param>
    /// <param name="id">Persisted task identifier.</param>
    /// <param name="title">Task title.</param>
    /// <param name="projectId">Owning project identifier or null for unassigned.</param>
    /// <param name="parentTaskId">Parent task identifier for subtasks.</param>
    /// <param name="note">Optional note content.</param>
    /// <param name="priority">Task priority.</param>
    /// <param name="isCompleted">Completion flag.</param>
    /// <param name="isFocused">Focus flag.</param>
    /// <param name="status">Workflow status.</param>
    /// <param name="createdAt">Persisted creation timestamp (UTC).</param>
    /// <param name="completedAt">Persisted completion timestamp (UTC).</param>
    /// <param name="sortOrder">Persisted sort order.</param>
    /// <param name="dueDateLocal">Optional due date in subscription local timezone.</param>
    /// <param name="dueTimeLocal">Optional due time in subscription local timezone.</param>
    /// <param name="dueAtUtc">Optional due instant in UTC.</param>
    /// <param name="isMarkedForToday">Explicit today marker.</param>
    /// <param name="tags">Optional task tags.</param>
    /// <returns>Rehydrated task aggregate.</returns>
    public static Task Rehydrate(
        Guid subscriptionId,
        Guid id,
        string title,
        Guid? projectId,
        Guid? parentTaskId,
        string note,
        TaskPriority priority,
        bool isCompleted,
        bool isFocused,
        bool isImportant,
        TaskStatus status,
        DateTime createdAt,
        DateTime? completedAt,
        int sortOrder,
        DateOnly? dueDateLocal,
        TimeOnly? dueTimeLocal,
        DateTime? dueAtUtc,
        bool isMarkedForToday,
        IEnumerable<string> tags)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Task id cannot be empty.", nameof(id));
        }

        var task = new Task(subscriptionId, title, projectId)
        {
            Id = id,
            ParentTaskId = parentTaskId,
            Priority = priority,
            IsCompleted = isCompleted,
            IsFocused = isFocused,
            IsImportant = isImportant,
            Status = status,
            CreatedAt = createdAt,
            CompletedAt = completedAt,
            SortOrder = sortOrder < 0 ? 0 : sortOrder,
            DueDateLocal = dueDateLocal,
            DueTimeLocal = dueTimeLocal,
            DueAtUtc = dueAtUtc,
            IsMarkedForToday = isMarkedForToday,
        };

        task.UpdateNote(note);

        if (tags is not null)
        {
            task.SetTags(tags);
        }

        return task;
    }

    /// <summary>
    /// Marks the task as complete and completes all subtasks recursively.
    /// </summary>
    public void Complete()
    {
        if (this.IsCompleted)
        {
            return;
        }

        this.IsCompleted = true;
        this.CompletedAt = DateTime.UtcNow;

        foreach (var subTask in this.subTasks)
        {
            subTask.Complete();
        }
    }

    /// <summary>
    /// Marks the task as not complete.
    /// </summary>
    public void Uncomplete()
    {
        if (!this.IsCompleted)
        {
            return;
        }

        this.IsCompleted = false;
        this.CompletedAt = null;
    }

    /// <summary>
    /// Adds a subtask and aligns parent/project identifiers.
    /// </summary>
    /// <param name="subTask">Subtask to attach.</param>
    public void AddSubTask(Task subTask)
    {
        ArgumentNullException.ThrowIfNull(subTask);

        if (subTask.Id == this.Id)
        {
            throw new InvalidOperationException("Task cannot be its own subtask.");
        }

        if (this.subTasks.Any(existing => existing.Id == subTask.Id))
        {
            throw new InvalidOperationException("Subtask is already attached to this task.");
        }

        if (subTask.SubscriptionId != this.SubscriptionId)
        {
            throw new InvalidOperationException("Subtask subscription must match parent task subscription.");
        }

        subTask.SetParent(this.Id);
        if (!this.ProjectId.HasValue)
        {
            subTask.UnassignFromProject();
        }
        else
        {
            subTask.AssignToProject(this.ProjectId.Value);
        }

        var nextSortOrder = this.subTasks.Count == 0
            ? 0
            : this.subTasks.Max(existing => existing.SortOrder) + 1;
        subTask.SetSortOrder(nextSortOrder);

        this.subTasks.Add(subTask);
    }

    /// <summary>
    /// Moves the task and all subtasks to a new project.
    /// </summary>
    /// <param name="newProjectId">Target project identifier.</param>
    public void MoveToProject(Guid newProjectId)
    {
        this.AssignToProject(newProjectId);
    }

    /// <summary>
    /// Assigns task and all subtasks to a project.
    /// </summary>
    /// <param name="projectId">Target project identifier.</param>
    public void AssignToProject(Guid projectId)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project id cannot be empty.", nameof(projectId));
        }

        this.ProjectId = projectId;

        foreach (var subTask in this.subTasks)
        {
            subTask.AssignToProject(projectId);
        }
    }

    /// <summary>
    /// Removes project assignment from this task and all subtasks.
    /// </summary>
    public void UnassignFromProject()
    {
        this.ProjectId = null;
        foreach (var subTask in this.subTasks)
        {
            subTask.UnassignFromProject();
        }
    }

    /// <summary>
    /// Sets the task priority.
    /// </summary>
    /// <param name="priority">New priority.</param>
    public void SetPriority(TaskPriority priority)
    {
        this.Priority = priority;
    }

    /// <summary>
    /// Sets persisted list order position for this task.
    /// </summary>
    /// <param name="sortOrder">Zero-based position value.</param>
    public void SetSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new ArgumentException("Sort order must be zero or greater.", nameof(sortOrder));
        }

        this.SortOrder = sortOrder;
    }

    /// <summary>
    /// Toggles the focused flag.
    /// </summary>
    public void ToggleFocus()
    {
        this.IsFocused = !this.IsFocused;
    }

    /// <summary>
    /// Toggles important flag on top-level tasks.
    /// </summary>
    public void ToggleImportant()
    {
        if (this.ParentTaskId.HasValue)
        {
            throw new InvalidOperationException("Subtasks cannot be marked as important.");
        }

        this.IsImportant = !this.IsImportant;
    }

    /// <summary>
    /// Updates the task title.
    /// </summary>
    /// <param name="newTitle">New title value.</param>
    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            throw new ArgumentException("Task title cannot be empty.", nameof(newTitle));
        }

        if (newTitle.Trim().Length > MAX_TITLE_LENGTH)
        {
            throw new ArgumentException($"Task title cannot exceed {MAX_TITLE_LENGTH} characters.", nameof(newTitle));
        }

        this.Title = newTitle.Trim();
    }

    /// <summary>
    /// Updates the task note. Empty input clears the note.
    /// </summary>
    /// <param name="newNote">New note value.</param>
    public void UpdateNote(string newNote)
    {
        if (string.IsNullOrWhiteSpace(newNote))
        {
            this.Note = null;
            return;
        }

        this.Note = newNote.Trim();
    }

    /// <summary>
    /// Sets the workflow status and keeps completion fields consistent.
    /// </summary>
    /// <param name="status">Target status.</param>
    public void SetStatus(TaskStatus status)
    {
        this.Status = status;
    }

    /// <summary>
    /// Sets a date-only due date.
    /// </summary>
    /// <param name="dueDateLocal">Due date in subscription local timezone.</param>
    public void SetDueDate(DateOnly dueDateLocal)
    {
        if (dueDateLocal == DateOnly.MinValue)
        {
            throw new ArgumentException("Due date must be a valid date.", nameof(dueDateLocal));
        }

        this.DueDateLocal = dueDateLocal;
        this.DueTimeLocal = null;
        this.DueAtUtc = null;
    }

    /// <summary>
    /// Sets a due date and time.
    /// </summary>
    /// <param name="dueDateLocal">Due date in subscription local timezone.</param>
    /// <param name="dueTimeLocal">Due time in subscription local timezone.</param>
    /// <param name="timeZone">Subscription timezone.</param>
    public void SetDueDateTime(DateOnly dueDateLocal, TimeOnly dueTimeLocal, TimeZoneInfo timeZone)
    {
        ArgumentNullException.ThrowIfNull(timeZone);

        if (dueDateLocal == DateOnly.MinValue)
        {
            throw new ArgumentException("Due date must be a valid date.", nameof(dueDateLocal));
        }

        var dueLocalDateTime = dueDateLocal.ToDateTime(dueTimeLocal, DateTimeKind.Unspecified);

        this.DueDateLocal = dueDateLocal;
        this.DueTimeLocal = dueTimeLocal;
        this.DueAtUtc = TimeZoneInfo.ConvertTimeToUtc(dueLocalDateTime, timeZone);
    }

    /// <summary>
    /// Clears due date and due time.
    /// </summary>
    public void ClearDueDate()
    {
        this.DueDateLocal = null;
        this.DueTimeLocal = null;
        this.DueAtUtc = null;
    }

    /// <summary>
    /// Toggles explicit Today marker.
    /// </summary>
    public void ToggleTodayMark()
    {
        this.IsMarkedForToday = !this.IsMarkedForToday;
    }

    /// <summary>
    /// Adds a reminder that triggers relative to due date-time.
    /// </summary>
    /// <param name="minutesBefore">Minutes before due instant.</param>
    /// <returns>The created reminder.</returns>
    public TaskReminder AddRelativeReminder(int minutesBefore)
    {
        var reminder = TaskReminder.CreateRelative(this.Id, minutesBefore, this.DueAtUtc);
        this.reminders.Add(reminder);
        return reminder;
    }

    /// <summary>
    /// Adds an on-time reminder for due date-time tasks.
    /// </summary>
    /// <returns>The created reminder.</returns>
    public TaskReminder AddOnTimeReminder()
    {
        return this.AddRelativeReminder(0);
    }

    /// <summary>
    /// Adds a reminder for a date-only task using fallback local time.
    /// </summary>
    /// <param name="fallbackLocalTime">Fallback local reminder time.</param>
    /// <param name="timeZone">Subscription timezone.</param>
    /// <returns>The created reminder.</returns>
    public TaskReminder AddDateOnlyReminder(TimeOnly fallbackLocalTime, TimeZoneInfo timeZone)
    {
        if (!this.DueDateLocal.HasValue)
        {
            throw new InvalidOperationException("Date-only reminder requires a due date.");
        }

        var reminder = TaskReminder.CreateDateOnlyFallback(this.Id, this.DueDateLocal.Value, fallbackLocalTime, timeZone);
        this.reminders.Add(reminder);
        return reminder;
    }

    /// <summary>
    /// Removes a reminder from this task.
    /// </summary>
    /// <param name="reminderId">Reminder identifier.</param>
    public void RemoveReminder(Guid reminderId)
    {
        this.reminders.RemoveAll(x => x.Id == reminderId);
    }

    /// <summary>
    /// Marks reminder as sent.
    /// </summary>
    /// <param name="reminderId">Reminder identifier.</param>
    /// <param name="sentAtUtc">UTC send timestamp.</param>
    public void MarkReminderSent(Guid reminderId, DateTime sentAtUtc)
    {
        var reminder = this.reminders.FirstOrDefault(x => x.Id == reminderId);
        if (reminder is null)
        {
            throw new EntityNotFoundException(nameof(TaskReminder), reminderId);
        }

        reminder.MarkSent(sentAtUtc);
    }

    /// <summary>
    /// Assigns all tags for the task.
    /// </summary>
    /// <param name="newTags">Target tags collection.</param>
    public void SetTags(IEnumerable<string> newTags)
    {
        ArgumentNullException.ThrowIfNull(newTags);

        this.tags.Clear();
        foreach (var tag in newTags)
        {
            this.AddTag(tag);
        }
    }

    /// <summary>
    /// Adds a tag to the task.
    /// </summary>
    /// <param name="tag">Tag value.</param>
    public void AddTag(string tag)
    {
        var normalized = NormalizeTag(tag);
        if (this.tags.Any(existing => string.Equals(existing, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        this.tags.Add(normalized);
    }

    /// <summary>
    /// Removes a tag from the task.
    /// </summary>
    /// <param name="tag">Tag value.</param>
    public void RemoveTag(string tag)
    {
        var normalized = NormalizeTag(tag);
        this.tags.RemoveAll(existing => string.Equals(existing, normalized, StringComparison.OrdinalIgnoreCase));
    }

    private void SetParent(Guid parentTaskId)
    {
        if (parentTaskId == Guid.Empty)
        {
            throw new ArgumentException("Parent task id cannot be empty.", nameof(parentTaskId));
        }

        this.ParentTaskId = parentTaskId;
    }

    private static string NormalizeTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new ArgumentException("Tag cannot be empty.", nameof(tag));
        }

        return tag.Trim();
    }
}
