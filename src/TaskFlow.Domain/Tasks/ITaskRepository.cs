namespace TaskFlow.Domain;

/// <summary>
/// Provides persistence operations for <see cref="Task"/> aggregates.
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Gets tasks for a project within the current subscription.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Tasks belonging to the project.</returns>
    Task<List<Task>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets direct subtasks for a parent task.
    /// </summary>
    /// <param name="parentTaskId">Parent task identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Direct subtasks ordered by persisted sort order.</returns>
    Task<List<Task>> GetSubTasksAsync(Guid parentTaskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets next sort order value for tasks in the same sibling scope.
    /// </summary>
    /// <param name="projectId">Project identifier (or null for unassigned).</param>
    /// <param name="parentTaskId">Parent task identifier (or null for top-level tasks).</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Next zero-based sort order value.</returns>
    Task<int> GetNextSortOrderAsync(Guid? projectId, Guid? parentTaskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks by priority within a project for the current subscription.
    /// </summary>
    /// <param name="priority">Priority to match.</param>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Tasks matching the priority.</returns>
    Task<List<Task>> GetByPriorityAsync(TaskPriority priority, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches tasks in a project using free text for the current subscription.
    /// </summary>
    /// <param name="query">Text query.</param>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Tasks matching the query.</returns>
    Task<List<Task>> SearchAsync(string query, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets focused tasks in a project for the current subscription.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Focused tasks.</returns>
    Task<List<Task>> GetFocusedAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tasks for the current subscription.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>All tasks.</returns>
    Task<List<Task>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recently created tasks in the current subscription.
    /// </summary>
    /// <param name="days">Recent day window.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Recent tasks ordered by creation descending.</returns>
    Task<List<Task>> GetRecentAsync(int days, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recently created unassigned tasks in the current subscription.
    /// </summary>
    /// <param name="days">Recent day window.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Recent unassigned tasks ordered by creation descending.</returns>
    Task<List<Task>> GetUnassignedRecentAsync(int days, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks due on a specific local date.
    /// </summary>
    /// <param name="localDate">Local due date in subscription timezone.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Tasks due on that date.</returns>
    Task<List<Task>> GetDueOnDateAsync(DateOnly localDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks due in local date range.
    /// </summary>
    /// <param name="localStartInclusive">Range start date inclusive.</param>
    /// <param name="localEndInclusive">Range end date inclusive.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Tasks due in range.</returns>
    Task<List<Task>> GetDueInRangeAsync(DateOnly localStartInclusive, DateOnly localEndInclusive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks due after a local date.
    /// </summary>
    /// <param name="localDateExclusive">Exclusive local date boundary.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Tasks due after the boundary.</returns>
    Task<List<Task>> GetDueAfterDateAsync(DateOnly localDateExclusive, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks by identifiers.
    /// </summary>
    /// <param name="taskIds">Task identifiers.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Matching tasks.</returns>
    Task<List<Task>> GetByIdsAsync(IEnumerable<Guid> taskIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a task.
    /// </summary>
    /// <param name="task">Task to add.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The added task.</returns>
    Task<Task> AddAsync(Task task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a task.
    /// </summary>
    /// <param name="task">Task to update.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The updated task.</returns>
    Task<Task> UpdateAsync(Task task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a batch of tasks.
    /// </summary>
    /// <param name="tasks">Tasks to update.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The updated tasks.</returns>
    Task<List<Task>> UpdateRangeAsync(IEnumerable<Task> tasks, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a task by identifier within the current subscription.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns><c>true</c> when a task was removed; otherwise <c>false</c>.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task by identifier within the current subscription.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The matching task.</returns>
    Task<Task> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
