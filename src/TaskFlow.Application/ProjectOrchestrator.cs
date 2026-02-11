using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ProjectOrchestrator> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectOrchestrator"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="projectRepository">Project repository.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription accessor.</param>
    public ProjectOrchestrator(ILogger<ProjectOrchestrator> logger, IProjectRepository projectRepository, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.logger = logger;
        this.projectRepository = projectRepository;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("Project - GetAll: fetching all projects for current subscription");
        return this.projectRepository.GetAllAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task<Project> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("Project - GetById: fetching project {ProjectId}", projectId);
        return this.projectRepository.GetByIdAsync(projectId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Project> CreateAsync(string name, string color, string icon, bool isDefault = false, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        this.logger.LogInformation("Project - Create: creating project in subscription {SubscriptionId}. Name={ProjectName}, IsDefault={IsDefault}", subscriptionId, name, isDefault);
        var project = new Project(subscriptionId, name, color, icon, note: string.Empty, isDefault: isDefault);
        var created = await this.projectRepository.AddAsync(project, cancellationToken);
        this.logger.LogInformation("Project - Create: created project {ProjectId}", created.Id);
        return created;
    }

    /// <inheritdoc/>
    public async Task<Project> UpdateNameAsync(Guid projectId, string newName, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Project - UpdateName: updating project {ProjectId} name to {ProjectName}", projectId, newName);
        var project = await this.projectRepository.GetByIdAsync(projectId, cancellationToken);
        project.UpdateName(newName);
        return await this.projectRepository.UpdateAsync(project, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Project> UpdateVisualsAsync(Guid projectId, string newColor, string newIcon, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Project - UpdateVisuals: updating visuals for project {ProjectId}. Color={Color}, Icon={Icon}", projectId, newColor, newIcon);
        var project = await this.projectRepository.GetByIdAsync(projectId, cancellationToken);
        project.UpdateColor(newColor);
        project.UpdateIcon(newIcon);
        return await this.projectRepository.UpdateAsync(project, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Project> UpdateViewTypeAsync(Guid projectId, ProjectViewType viewType, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Project - UpdateViewType: updating view type for project {ProjectId} to {ViewType}", projectId, viewType);
        var project = await this.projectRepository.GetByIdAsync(projectId, cancellationToken);
        project.UpdateViewType(viewType);
        return await this.projectRepository.UpdateAsync(project, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Project - Delete: deleting project {ProjectId}", projectId);
        return this.projectRepository.DeleteAsync(projectId, cancellationToken);
    }
}
