namespace TaskFlow.Presentation.Services;

/// <summary>
/// Stores undo actions and executes the most recent action on demand.
/// </summary>
public interface IUndoManager
{
    /// <summary>
    /// Registers an undo callback for a user action.
    /// </summary>
    /// <param name="description">Short description shown when the action is undone.</param>
    /// <param name="undoAsync">Undo callback.</param>
    void Register(string description, Func<System.Threading.Tasks.Task> undoAsync);

    /// <summary>
    /// Executes the most recent undo callback if available.
    /// </summary>
    /// <returns>Result describing whether an action was undone.</returns>
    System.Threading.Tasks.Task<UndoExecutionResult> UndoAsync();
}
