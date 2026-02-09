using TaskFlow.Domain;

namespace TaskFlow.Application;

/// <summary>
/// Coordinates project-related application use cases.
/// </summary>
public interface IProjectOrchestrator
{
    /// <summary>
    /// Gets all projects for the current subscription.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Projects ordered by repository behavior.</returns>
    global::System.Threading.Tasks.Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one project by identifier.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching project.</returns>
    global::System.Threading.Tasks.Task<Project> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new project for the current subscription.
    /// </summary>
    /// <param name="name">Project name.</param>
    /// <param name="color">Project color.</param>
    /// <param name="icon">Project icon key.</param>
    /// <param name="isDefault">Whether project is default.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created project.</returns>
    global::System.Threading.Tasks.Task<Project> CreateAsync(string name, string color, string icon, bool isDefault = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates project name and persists immediately.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="newName">New project name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated project.</returns>
    global::System.Threading.Tasks.Task<Project> UpdateNameAsync(Guid projectId, string newName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates project visual properties and persists immediately.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="newColor">New project color.</param>
    /// <param name="newIcon">New project icon key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated project.</returns>
    global::System.Threading.Tasks.Task<Project> UpdateVisualsAsync(Guid projectId, string newColor, string newIcon, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates project view type and persists immediately.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="viewType">Target view mode.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated project.</returns>
    global::System.Threading.Tasks.Task<Project> UpdateViewTypeAsync(Guid projectId, ProjectViewType viewType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes project by identifier.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> when project was deleted.</returns>
    global::System.Threading.Tasks.Task<bool> DeleteAsync(Guid projectId, CancellationToken cancellationToken = default);
}
