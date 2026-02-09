namespace TaskFlow.Domain;

/// <summary>
/// Represents a task aggregate with behavior for completion, hierarchy, and metadata updates.
/// </summary>
public class Task
{
    private readonly List<Task> subTasks = [];

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
    /// Gets the workflow status of the task.
    /// </summary>
    public TaskStatus Status { get; private set; }

    /// <summary>
    /// Gets the identifier of the project containing this task.
    /// </summary>
    public Guid ProjectId { get; private set; }

    /// <summary>
    /// Gets the parent task identifier when this task is a subtask.
    /// </summary>
    public Guid ParentTaskId { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the task was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the task was completed.
    /// DateTime.MinValue indicates not completed.
    /// </summary>
    public DateTime CompletedAt { get; private set; }

    /// <summary>
    /// Gets read-only subtasks that belong to this task.
    /// </summary>
    public IReadOnlyCollection<Task> SubTasks => this.subTasks.AsReadOnly();

    private Task()
    {
        this.Title = string.Empty;
        this.Note = string.Empty;
    }

    /// <summary>
    /// Initializes a new task in a project.
    /// </summary>
    /// <param name="title">Task title.</param>
    /// <param name="projectId">Owning project identifier.</param>
    public Task(string title, Guid projectId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title cannot be empty.", nameof(title));
        }

        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project id cannot be empty.", nameof(projectId));
        }

        this.Id = Guid.NewGuid();
        this.Title = title.Trim();
        this.Note = string.Empty;
        this.ProjectId = projectId;
        this.ParentTaskId = Guid.Empty;
        this.Priority = TaskPriority.Medium;
        this.Status = TaskStatus.ToDo;
        this.CreatedAt = DateTime.UtcNow;
        this.CompletedAt = DateTime.MinValue;
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
        this.Status = TaskStatus.Done;
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
        this.CompletedAt = DateTime.MinValue;

        if (this.Status == TaskStatus.Done)
        {
            this.Status = TaskStatus.ToDo;
        }
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

        subTask.SetParent(this.Id);
        subTask.MoveToProject(this.ProjectId);
        this.subTasks.Add(subTask);
    }

    /// <summary>
    /// Moves the task and all subtasks to a new project.
    /// </summary>
    /// <param name="newProjectId">Target project identifier.</param>
    public void MoveToProject(Guid newProjectId)
    {
        if (newProjectId == Guid.Empty)
        {
            throw new ArgumentException("Project id cannot be empty.", nameof(newProjectId));
        }

        this.ProjectId = newProjectId;

        foreach (var subTask in this.subTasks)
        {
            subTask.MoveToProject(newProjectId);
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
    /// Toggles the focused flag.
    /// </summary>
    public void ToggleFocus()
    {
        this.IsFocused = !this.IsFocused;
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
            this.Note = string.Empty;
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

        if (status == TaskStatus.Done)
        {
            this.Complete();
            return;
        }

        if (this.IsCompleted)
        {
            this.Uncomplete();
        }
    }

    private void SetParent(Guid parentTaskId)
    {
        if (parentTaskId == Guid.Empty)
        {
            throw new ArgumentException("Parent task id cannot be empty.", nameof(parentTaskId));
        }

        this.ParentTaskId = parentTaskId;
    }
}
