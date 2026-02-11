namespace TaskFlow.Presentation.Services;

/// <summary>
/// Result from attempting to undo the latest action.
/// </summary>
/// <param name="HasAction">Indicates whether an action was undone.</param>
/// <param name="Description">Description of the undone action.</param>
public readonly record struct UndoExecutionResult(bool HasAction, string Description)
{
    /// <summary>
    /// Returns a result indicating there was nothing to undo.
    /// </summary>
    public static UndoExecutionResult None => new(false, string.Empty);

    /// <summary>
    /// Returns a successful undo result for the provided action description.
    /// </summary>
    /// <param name="description">Description of the undone action.</param>
    /// <returns>Successful undo result.</returns>
    public static UndoExecutionResult Success(string description)
    {
        return new UndoExecutionResult(true, description);
    }
}
