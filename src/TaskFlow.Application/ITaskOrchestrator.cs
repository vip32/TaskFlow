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
    Task<List<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets direct subtasks for one parent task.
    /// </summary>
    /// <param name="parentTaskId">Parent task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Direct subtasks in persisted order.</returns>
    Task<List<DomainTask>> GetSubTasksAsync(Guid parentTaskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches tasks in a project.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="query">Search text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching tasks.</returns>
    Task<List<DomainTask>> SearchAsync(Guid projectId, string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets historical task name suggestions for autocomplete.
    /// </summary>
    /// <param name="prefix">Current input prefix.</param>
    /// <param name="isSubTaskName">Whether suggestions are for subtask context.</param>
    /// <param name="take">Maximum suggestion count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Suggested names ordered by recent usage and frequency.</returns>
    Task<List<string>> GetNameSuggestionsAsync(string prefix, bool isSubTaskName, int take = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a task and persists it immediately.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="title">Task title.</param>
    /// <param name="priority">Task priority.</param>
    /// <param name="note">Task note.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created task.</returns>
    Task<DomainTask> CreateAsync(Guid projectId, string title, TaskPriority priority, string note, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an unassigned task and persists it immediately.
    /// </summary>
    /// <param name="title">Task title.</param>
    /// <param name="priority">Task priority.</param>
    /// <param name="note">Task note.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created task.</returns>
    Task<DomainTask> CreateUnassignedAsync(string title, TaskPriority priority, string note, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates task title and persists immediately.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="newTitle">New title.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    Task<DomainTask> UpdateTitleAsync(Guid taskId, string newTitle, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates task note and persists immediately.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="newNote">New note.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    Task<DomainTask> UpdateNoteAsync(Guid taskId, string newNote, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets task priority and persists immediately.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="priority">Target priority.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    Task<DomainTask> SetPriorityAsync(Guid taskId, TaskPriority priority, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets task status and persists immediately.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="status">Target status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    Task<DomainTask> SetStatusAsync(Guid taskId, DomainTaskStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Toggles task focus and persists immediately.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    Task<DomainTask> ToggleFocusAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a task to another project and persists immediately.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="newProjectId">Target project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    Task<DomainTask> MoveToProjectAsync(Guid taskId, Guid newProjectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists custom ordering for top-level tasks in a project.
    /// </summary>
    /// <param name="projectId">Project identifier.</param>
    /// <param name="orderedTaskIds">Ordered list of task identifiers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated ordered tasks.</returns>
    Task<List<DomainTask>> ReorderProjectTasksAsync(Guid projectId, IReadOnlyList<Guid> orderedTaskIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists custom ordering for direct subtasks under a parent task.
    /// </summary>
    /// <param name="parentTaskId">Parent task identifier.</param>
    /// <param name="orderedTaskIds">Ordered list of subtask identifiers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated ordered subtasks.</returns>
    Task<List<DomainTask>> ReorderSubTasksAsync(Guid parentTaskId, IReadOnlyList<Guid> orderedTaskIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes project assignment from a task.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    Task<DomainTask> UnassignFromProjectAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets date-only due date.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="dueDateLocal">Local due date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    Task<DomainTask> SetDueDateAsync(Guid taskId, DateOnly dueDateLocal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets due date and due time.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="dueDateLocal">Local due date.</param>
    /// <param name="dueTimeLocal">Local due time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    Task<DomainTask> SetDueDateTimeAsync(Guid taskId, DateOnly dueDateLocal, TimeOnly dueTimeLocal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears due date settings.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    Task<DomainTask> ClearDueDateAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Toggles today marker on task.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    Task<DomainTask> ToggleTodayMarkAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds reminder relative to due date-time.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="minutesBefore">Minutes before due instant.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    Task<DomainTask> AddRelativeReminderAsync(Guid taskId, int minutesBefore, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds date-only fallback reminder.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="fallbackLocalTime">Fallback local time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    Task<DomainTask> AddDateOnlyReminderAsync(Guid taskId, TimeOnly fallbackLocalTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes reminder from task.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="reminderId">Reminder identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    Task<DomainTask> RemoveReminderAsync(Guid taskId, Guid reminderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks in recent bucket.
    /// </summary>
    /// <param name="days">Recent day window.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Recent tasks.</returns>
    Task<List<DomainTask>> GetRecentAsync(int days = 7, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks in today bucket.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Today tasks.</returns>
    Task<List<DomainTask>> GetTodayAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks in this-week bucket.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>This-week tasks.</returns>
    Task<List<DomainTask>> GetThisWeekAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks in upcoming bucket.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Upcoming tasks.</returns>
    Task<List<DomainTask>> GetUpcomingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a task.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> when task was deleted.</returns>
    Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken = default);
}
