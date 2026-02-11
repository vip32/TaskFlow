using TaskFlow.Domain;

namespace TaskFlow.Presentation.Services;

public sealed class AppState
{
    private readonly Dictionary<Guid, int> projectTaskCounts = [];

    public event Action Changed;

    public bool IsDrawerOpen { get; private set; } = true;

    public IReadOnlyList<Project> Projects { get; private set; } = [];

    public IReadOnlyList<MyTaskFlowSection> MyTaskFlowSections { get; private set; } = [];

    public IReadOnlyDictionary<Guid, int> ProjectTaskCounts => this.projectTaskCounts;

    public Guid? SelectedProjectId { get; private set; }

    public Guid? SelectedSectionId { get; private set; }

    public void SetDrawerOpen(bool isOpen)
    {
        if (this.IsDrawerOpen == isOpen)
        {
            return;
        }

        this.IsDrawerOpen = isOpen;
        NotifyChanged();
    }

    public void ToggleDrawer()
    {
        this.IsDrawerOpen = !this.IsDrawerOpen;
        NotifyChanged();
    }

    public void SetNavigationData(
        IReadOnlyList<Project> projects,
        IReadOnlyList<MyTaskFlowSection> myTaskFlowSections,
        IReadOnlyDictionary<Guid, int> taskCounts)
    {
        this.Projects = projects;
        this.MyTaskFlowSections = myTaskFlowSections;
        this.projectTaskCounts.Clear();
        foreach (var entry in taskCounts)
        {
            this.projectTaskCounts[entry.Key] = entry.Value;
        }

        if (!this.SelectedProjectId.HasValue && !this.SelectedSectionId.HasValue)
        {
            this.SelectedProjectId = this.Projects.FirstOrDefault()?.Id;
        }

        if (this.SelectedProjectId.HasValue && !this.Projects.Any(x => x.Id == this.SelectedProjectId.Value))
        {
            this.SelectedProjectId = this.Projects.FirstOrDefault()?.Id;
            this.SelectedSectionId = null;
        }

        if (this.SelectedSectionId.HasValue && !this.MyTaskFlowSections.Any(x => x.Id == this.SelectedSectionId.Value))
        {
            this.SelectedSectionId = null;
        }

        NotifyChanged();
    }

    public void SetProjectTaskCounts(IReadOnlyDictionary<Guid, int> taskCounts)
    {
        this.projectTaskCounts.Clear();
        foreach (var entry in taskCounts)
        {
            this.projectTaskCounts[entry.Key] = entry.Value;
        }

        NotifyChanged();
    }

    public void SelectProject(Guid projectId)
    {
        this.SelectedProjectId = projectId;
        this.SelectedSectionId = null;
        NotifyChanged();
    }

    public void SelectSection(Guid sectionId)
    {
        this.SelectedSectionId = sectionId;
        this.SelectedProjectId = null;
        NotifyChanged();
    }

    public int GetProjectTaskCount(Guid projectId)
    {
        return this.projectTaskCounts.TryGetValue(projectId, out var count) ? count : 0;
    }

    private void NotifyChanged()
    {
        this.Changed?.Invoke();
    }
}
