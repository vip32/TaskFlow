namespace TaskFlow.Domain;

/// <summary>
/// Provides access to task history for autocomplete and autofill scenarios.
/// </summary>
public interface ITaskHistoryRepository
{
    /// <summary>
    /// Registers usage of a task or subtask name.
    /// </summary>
    /// <param name="name">Name to register.</param>
    /// <param name="isSubTaskName">Whether usage belongs to subtask context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing completion.</returns>
    global::System.Threading.Tasks.Task RegisterUsageAsync(string name, bool isSubTaskName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets suggested names by prefix for the current subscription.
    /// </summary>
    /// <param name="prefix">Prefix text typed by the user.</param>
    /// <param name="isSubTaskName">Whether suggestions are requested for subtask context.</param>
    /// <param name="take">Maximum suggestion count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ordered name suggestions.</returns>
    global::System.Threading.Tasks.Task<List<string>> GetSuggestionsAsync(string prefix, bool isSubTaskName, int take = 20, CancellationToken cancellationToken = default);
}
