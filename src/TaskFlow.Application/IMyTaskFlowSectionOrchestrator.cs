using TaskFlow.Domain;

namespace TaskFlow.Application;

/// <summary>
/// Coordinates My Task Flow section use cases.
/// </summary>
public interface IMyTaskFlowSectionOrchestrator
{
    /// <summary>
    /// Gets all sections for current subscription.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ordered sections.</returns>
    Task<List<MyTaskFlowSection>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a custom section.
    /// </summary>
    /// <param name="name">Section name.</param>
    /// <param name="sortOrder">Sort order.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created section.</returns>
    Task<MyTaskFlowSection> CreateAsync(string name, int sortOrder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates section rule.
    /// </summary>
    /// <param name="sectionId">Section identifier.</param>
    /// <param name="dueBucket">Due bucket filter.</param>
    /// <param name="includeAssignedTasks">Include assigned tasks.</param>
    /// <param name="includeUnassignedTasks">Include unassigned tasks.</param>
    /// <param name="includeDoneTasks">Include done tasks.</param>
    /// <param name="includeCancelledTasks">Include cancelled tasks.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated section.</returns>
    Task<MyTaskFlowSection> UpdateRuleAsync(Guid sectionId, TaskFlowDueBucket dueBucket, bool includeAssignedTasks, bool includeUnassignedTasks, bool includeDoneTasks, bool includeCancelledTasks, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds manual task membership.
    /// </summary>
    /// <param name="sectionId">Section identifier.</param>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated section.</returns>
    Task<MyTaskFlowSection> IncludeTaskAsync(Guid sectionId, Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes manual task membership.
    /// </summary>
    /// <param name="sectionId">Section identifier.</param>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated section.</returns>
    Task<MyTaskFlowSection> RemoveTaskAsync(Guid sectionId, Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves tasks shown for one section using hybrid rule and manual curation.
    /// </summary>
    /// <param name="sectionId">Section identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tasks included in section.</returns>
    Task<List<Domain.Task>> GetSectionTasksAsync(Guid sectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves tasks shown for a provided section definition.
    /// </summary>
    /// <param name="section">Section definition to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tasks included in section.</returns>
    Task<List<Domain.Task>> GetSectionTasksAsync(MyTaskFlowSection section, CancellationToken cancellationToken = default);
}
