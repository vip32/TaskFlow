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
    global::System.Threading.Tasks.Task<List<Task>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks by priority within a project for the current subscription.
    /// </summary>
    /// <param name="priority">Priority to match.</param>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Tasks matching the priority.</returns>
    global::System.Threading.Tasks.Task<List<Task>> GetByPriorityAsync(TaskPriority priority, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches tasks in a project using free text for the current subscription.
    /// </summary>
    /// <param name="query">Text query.</param>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Tasks matching the query.</returns>
    global::System.Threading.Tasks.Task<List<Task>> SearchAsync(string query, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets focused tasks in a project for the current subscription.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Focused tasks.</returns>
    global::System.Threading.Tasks.Task<List<Task>> GetFocusedAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a task.
    /// </summary>
    /// <param name="task">Task to add.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The added task.</returns>
    global::System.Threading.Tasks.Task<Task> AddAsync(Task task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a task.
    /// </summary>
    /// <param name="task">Task to update.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The updated task.</returns>
    global::System.Threading.Tasks.Task<Task> UpdateAsync(Task task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a task by identifier within the current subscription.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns><c>true</c> when a task was removed; otherwise <c>false</c>.</returns>
    global::System.Threading.Tasks.Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task by identifier within the current subscription.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The matching task.</returns>
    global::System.Threading.Tasks.Task<Task> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
