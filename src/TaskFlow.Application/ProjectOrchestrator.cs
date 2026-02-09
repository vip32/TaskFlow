using Injectio.Attributes;
using TaskFlow.Domain;

namespace TaskFlow.Application;

/// <summary>
/// Default application orchestrator for project use cases.
/// </summary>
[RegisterScoped(ServiceType = typeof(IProjectOrchestrator))]
public sealed class ProjectOrchestrator : IProjectOrchestrator
{
    private readonly IProjectRepository projectRepository;
    private readonly ICurrentSubscriptionAccessor currentSubscriptionAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectOrchestrator"/> class.
    /// </summary>
    /// <param name="projectRepository">Project repository.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription accessor.</param>
    public ProjectOrchestrator(IProjectRepository projectRepository, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.projectRepository = projectRepository;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return this.projectRepository.GetAllAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<Project> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return this.projectRepository.GetByIdAsync(projectId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Project> CreateAsync(string name, string color, string icon, bool isDefault = false, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var project = new Project(subscriptionId, name, color, icon, note: string.Empty, isDefault: isDefault);
        return await this.projectRepository.AddAsync(project, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Project> UpdateNameAsync(Guid projectId, string newName, CancellationToken cancellationToken = default)
    {
        var project = await this.projectRepository.GetByIdAsync(projectId, cancellationToken);
        project.UpdateName(newName);
        return await this.projectRepository.UpdateAsync(project, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Project> UpdateVisualsAsync(Guid projectId, string newColor, string newIcon, CancellationToken cancellationToken = default)
    {
        var project = await this.projectRepository.GetByIdAsync(projectId, cancellationToken);
        project.UpdateColor(newColor);
        project.UpdateIcon(newIcon);
        return await this.projectRepository.UpdateAsync(project, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Project> UpdateViewTypeAsync(Guid projectId, ProjectViewType viewType, CancellationToken cancellationToken = default)
    {
        var project = await this.projectRepository.GetByIdAsync(projectId, cancellationToken);
        project.UpdateViewType(viewType);
        return await this.projectRepository.UpdateAsync(project, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return this.projectRepository.DeleteAsync(projectId, cancellationToken);
    }
}
