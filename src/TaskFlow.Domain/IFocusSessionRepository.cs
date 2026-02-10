namespace TaskFlow.Domain;

/// <summary>
/// Provides persistence operations for <see cref="FocusSession"/> aggregates.
/// </summary>
public interface IFocusSessionRepository
{
    /// <summary>
    /// Gets most recent focus sessions for current subscription.
    /// </summary>
    /// <param name="take">Maximum sessions to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Recent focus sessions ordered by start time descending.</returns>
    Task<List<FocusSession>> GetRecentAsync(int take = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active running focus session if any.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Running session or null.</returns>
    Task<FocusSession> GetRunningAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds focus session.
    /// </summary>
    /// <param name="session">Session to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Added session.</returns>
    Task<FocusSession> AddAsync(FocusSession session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates focus session.
    /// </summary>
    /// <param name="session">Session to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated session.</returns>
    Task<FocusSession> UpdateAsync(FocusSession session, CancellationToken cancellationToken = default);
}
