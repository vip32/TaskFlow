namespace TaskFlow.Domain;

/// <summary>
/// Provides persistence operations for <see cref="MyTaskFlowSection"/> aggregates.
/// </summary>
public interface IMyTaskFlowSectionRepository
{
    /// <summary>
    /// Gets all sections for the current subscription.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ordered sections for the current subscription.</returns>
    global::System.Threading.Tasks.Task<List<MyTaskFlowSection>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one section by identifier.
    /// </summary>
    /// <param name="sectionId">Section identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching section.</returns>
    global::System.Threading.Tasks.Task<MyTaskFlowSection> GetByIdAsync(Guid sectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a section.
    /// </summary>
    /// <param name="section">Section to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added section.</returns>
    global::System.Threading.Tasks.Task<MyTaskFlowSection> AddAsync(MyTaskFlowSection section, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a section.
    /// </summary>
    /// <param name="section">Section to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated section.</returns>
    global::System.Threading.Tasks.Task<MyTaskFlowSection> UpdateAsync(MyTaskFlowSection section, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a section.
    /// </summary>
    /// <param name="sectionId">Section identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> when section was deleted; otherwise <c>false</c>.</returns>
    global::System.Threading.Tasks.Task<bool> DeleteAsync(Guid sectionId, CancellationToken cancellationToken = default);
}
