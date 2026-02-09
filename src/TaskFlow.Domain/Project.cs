namespace TaskFlow.Domain;

public class Project
{
    private readonly List<Task> tasks = [];

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Color { get; private set; }

    public string Icon { get; private set; }

    public bool IsDefault { get; private set; }

    public ProjectViewType ViewType { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<Task> Tasks => this.tasks.AsReadOnly();

    private Project()
    {
        this.Name = string.Empty;
        this.Color = string.Empty;
        this.Icon = string.Empty;
    }

    public Project(string name, string color, string icon, bool isDefault = false)
    {
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

        this.Id = Guid.NewGuid();
        this.Name = name.Trim();
        this.Color = color.Trim();
        this.Icon = icon.Trim();
        this.IsDefault = isDefault;
        this.ViewType = ProjectViewType.List;
        this.CreatedAt = DateTime.UtcNow;
    }

    public void AddTask(Task task)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (this.tasks.Any(existing => existing.Id == task.Id))
        {
            throw new InvalidOperationException("Task is already in this project.");
        }

        task.MoveToProject(this.Id);
        this.tasks.Add(task);
    }

    public void RemoveTask(Task task)
    {
        ArgumentNullException.ThrowIfNull(task);

        this.tasks.RemoveAll(existing => existing.Id == task.Id);
    }

    public void UpdateViewType(ProjectViewType viewType)
    {
        this.ViewType = viewType;
    }

    public int GetTaskCount()
    {
        return this.tasks.Count(task => !task.IsCompleted);
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Project name cannot be empty.", nameof(newName));
        }

        this.Name = newName.Trim();
    }

    public void UpdateColor(string newColor)
    {
        if (string.IsNullOrWhiteSpace(newColor))
        {
            throw new ArgumentException("Project color cannot be empty.", nameof(newColor));
        }

        this.Color = newColor.Trim();
    }

    public void UpdateIcon(string newIcon)
    {
        if (string.IsNullOrWhiteSpace(newIcon))
        {
            throw new ArgumentException("Project icon cannot be empty.", nameof(newIcon));
        }

        this.Icon = newIcon.Trim();
    }
}
