namespace TaskFlow.Domain;

public class Task
{
    private readonly List<Task> subTasks = [];

    public Guid Id { get; private set; }

    public string Title { get; private set; }

    public string Note { get; private set; }

    public TaskPriority Priority { get; private set; }

    public bool IsCompleted { get; private set; }

    public bool IsFocused { get; private set; }

    public TaskStatus Status { get; private set; }

    public Guid ProjectId { get; private set; }

    public Guid ParentTaskId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime CompletedAt { get; private set; }

    public IReadOnlyCollection<Task> SubTasks => this.subTasks.AsReadOnly();

    private Task()
    {
        this.Title = string.Empty;
        this.Note = string.Empty;
    }

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

    public void SetPriority(TaskPriority priority)
    {
        this.Priority = priority;
    }

    public void ToggleFocus()
    {
        this.IsFocused = !this.IsFocused;
    }

    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            throw new ArgumentException("Task title cannot be empty.", nameof(newTitle));
        }

        this.Title = newTitle.Trim();
    }

    public void UpdateNote(string newNote)
    {
        if (string.IsNullOrWhiteSpace(newNote))
        {
            this.Note = string.Empty;
            return;
        }

        this.Note = newNote.Trim();
    }

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
