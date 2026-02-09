namespace TaskFlow.Domain;

/// <summary>
/// Provides persistence operations for <see cref="Project"/> aggregates.
/// </summary>
public interface IProjectRepository
{
    /// <summary>
    /// Gets all projects for the current subscription.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A list of projects.</returns>
    global::System.Threading.Tasks.Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project by identifier for the current subscription.
    /// </summary>
    /// <param name="id">Project identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The matching project.</returns>
    global::System.Threading.Tasks.Task<Project> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new project.
    /// </summary>
    /// <param name="project">Project to add.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The added project.</returns>
    global::System.Threading.Tasks.Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing project.
    /// </summary>
    /// <param name="project">Project to update.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The updated project.</returns>
    global::System.Threading.Tasks.Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a project by identifier for the current subscription.
    /// </summary>
    /// <param name="id">Project identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns><c>true</c> when a project was removed; otherwise <c>false</c>.</returns>
    global::System.Threading.Tasks.Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
