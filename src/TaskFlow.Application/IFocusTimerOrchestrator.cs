using TaskFlow.Domain;

namespace TaskFlow.Application;

/// <summary>
/// Coordinates focus timer workflow and focus session persistence.
/// </summary>
public interface IFocusTimerOrchestrator
{
    /// <summary>
    /// Starts a focus session optionally linked to task.
    /// </summary>
    /// <param name="taskId">Task identifier or Guid.Empty for none.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Started focus session.</returns>
    Task<FocusSession> StartAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends currently running focus session if present.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ended session or null when no active session exists.</returns>
    Task<FocusSession> EndCurrentAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent focus sessions.
    /// </summary>
    /// <param name="take">Maximum number of sessions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Recent sessions.</returns>
    Task<List<FocusSession>> GetRecentAsync(int take = 20, CancellationToken cancellationToken = default);
}
