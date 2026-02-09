namespace TaskFlow.Domain;

/// <summary>
/// Represents a project aggregate root that owns task organization settings.
/// </summary>
public class Project
{
    private readonly List<Task> tasks = [];

    /// <summary>
    /// Gets the subscription identifier that owns this project.
    /// </summary>
    public Guid SubscriptionId { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the project.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the display name of the project.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the color marker used by the project in the UI.
    /// </summary>
    public string Color { get; private set; }

    /// <summary>
    /// Gets the icon name used by the project in the UI.
    /// </summary>
    public string Icon { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this project is the default project.
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// Gets the preferred view mode for this project.
    /// </summary>
    public ProjectViewType ViewType { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when the project was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the tasks currently associated with this project.
    /// </summary>
    public IReadOnlyCollection<Task> Tasks => this.tasks.AsReadOnly();

    private Project()
    {
        this.Name = string.Empty;
        this.Color = string.Empty;
        this.Icon = string.Empty;
    }

    /// <summary>
    /// Initializes a new project with required metadata.
    /// </summary>
    /// <param name="subscriptionId">Owning subscription identifier.</param>
    /// <param name="name">Project display name.</param>
    /// <param name="color">Project color marker.</param>
    /// <param name="icon">Project icon key.</param>
    /// <param name="isDefault">Whether the project is the default project.</param>
    public Project(Guid subscriptionId, string name, string color, string icon, bool isDefault = false)
    {
        if (subscriptionId == Guid.Empty)
        {
            throw new ArgumentException("Subscription id cannot be empty.", nameof(subscriptionId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(color))
        {
            throw new ArgumentException("Project color cannot be empty.", nameof(color));
        }

        if (string.IsNullOrWhiteSpace(icon))
        {
            throw new ArgumentException("Project icon cannot be empty.", nameof(icon));
        }

        this.SubscriptionId = subscriptionId;
        this.Id = Guid.NewGuid();
        this.Name = name.Trim();
        this.Color = color.Trim();
        this.Icon = icon.Trim();
        this.IsDefault = isDefault;
        this.ViewType = ProjectViewType.List;
        this.CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a task to this project and aligns the task project identifier.
    /// </summary>
    /// <param name="task">Task to add.</param>
    public void AddTask(Task task)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.SubscriptionId != this.SubscriptionId)
        {
            throw new InvalidOperationException("Task subscription must match project subscription.");
        }

        if (this.tasks.Any(existing => existing.Id == task.Id))
        {
            throw new InvalidOperationException("Task is already in this project.");
        }

        task.MoveToProject(this.Id);
        this.tasks.Add(task);
    }

    /// <summary>
    /// Removes a task from this project.
    /// </summary>
    /// <param name="task">Task to remove.</param>
    public void RemoveTask(Task task)
    {
        ArgumentNullException.ThrowIfNull(task);

        this.tasks.RemoveAll(existing => existing.Id == task.Id);
    }

    /// <summary>
    /// Updates the preferred project view mode.
    /// </summary>
    /// <param name="viewType">Target view mode.</param>
    public void UpdateViewType(ProjectViewType viewType)
    {
        this.ViewType = viewType;
    }

    /// <summary>
    /// Gets the number of active tasks in this project.
    /// </summary>
    /// <returns>Count of non-completed tasks.</returns>
    public int GetTaskCount()
    {
        return this.tasks.Count(task => !task.IsCompleted);
    }

    /// <summary>
    /// Updates the project name.
    /// </summary>
    /// <param name="newName">New name value.</param>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Project name cannot be empty.", nameof(newName));
        }

        this.Name = newName.Trim();
    }

    /// <summary>
    /// Updates the project color marker.
    /// </summary>
    /// <param name="newColor">New color value.</param>
    public void UpdateColor(string newColor)
    {
        if (string.IsNullOrWhiteSpace(newColor))
        {
            throw new ArgumentException("Project color cannot be empty.", nameof(newColor));
        }

        this.Color = newColor.Trim();
    }

    /// <summary>
    /// Updates the project icon key.
    /// </summary>
    /// <param name="newIcon">New icon value.</param>
    public void UpdateIcon(string newIcon)
    {
        if (string.IsNullOrWhiteSpace(newIcon))
        {
            throw new ArgumentException("Project icon cannot be empty.", nameof(newIcon));
        }

        this.Icon = newIcon.Trim();
    }
}
