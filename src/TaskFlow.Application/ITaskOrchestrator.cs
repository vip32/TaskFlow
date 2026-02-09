using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;
using DomainTaskStatus = TaskFlow.Domain.TaskStatus;

namespace TaskFlow.Application;

/// <summary>
/// Coordinates task-related application use cases.
/// </summary>
public interface ITaskOrchestrator
{
    /// <summary>
    /// Gets tasks for a project.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tasks for the project.</returns>
    global::System.Threading.Tasks.Task<List<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches tasks in a project.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="query">Search text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching tasks.</returns>
    global::System.Threading.Tasks.Task<List<DomainTask>> SearchAsync(Guid projectId, string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a task and persists it immediately.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="title">Task title.</param>
    /// <param name="priority">Task priority.</param>
    /// <param name="note">Task note.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created task.</returns>
    global::System.Threading.Tasks.Task<DomainTask> CreateAsync(Guid projectId, string title, TaskPriority priority, string note, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates task title and persists immediately.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="newTitle">New title.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    global::System.Threading.Tasks.Task<DomainTask> UpdateTitleAsync(Guid taskId, string newTitle, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates task note and persists immediately.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="newNote">New note.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    global::System.Threading.Tasks.Task<DomainTask> UpdateNoteAsync(Guid taskId, string newNote, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets task priority and persists immediately.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="priority">Target priority.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    global::System.Threading.Tasks.Task<DomainTask> SetPriorityAsync(Guid taskId, TaskPriority priority, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets task status and persists immediately.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="status">Target status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    global::System.Threading.Tasks.Task<DomainTask> SetStatusAsync(Guid taskId, DomainTaskStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Toggles task focus and persists immediately.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    global::System.Threading.Tasks.Task<DomainTask> ToggleFocusAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a task to another project and persists immediately.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="newProjectId">Target project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    global::System.Threading.Tasks.Task<DomainTask> MoveToProjectAsync(Guid taskId, Guid newProjectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a task.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> when task was deleted.</returns>
    global::System.Threading.Tasks.Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken = default);
}
