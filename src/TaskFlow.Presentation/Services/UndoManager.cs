namespace TaskFlow.Presentation.Services;

/// <summary>
/// Default in-memory undo manager for UI actions.
/// </summary>
public sealed class UndoManager : IUndoManager
{
    private readonly int maxActions;
    private readonly Stack<UndoAction> undoActions = [];
    private bool isUndoInProgress;

    /// <summary>
    /// Initializes a new instance of the <see cref="UndoManager"/> class.
    /// </summary>
    /// <param name="maxActions">Maximum number of stored undo actions before the stack is reset.</param>
    public UndoManager(int maxActions = 20)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxActions);

        this.maxActions = maxActions;
    }

    /// <inheritdoc />
    public void Register(string description, Func<System.Threading.Tasks.Task> undoAsync)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(undoAsync);

        if (this.isUndoInProgress)
        {
            return;
        }

        if (this.undoActions.Count >= this.maxActions)
        {
            this.undoActions.Clear();
        }

        this.undoActions.Push(new UndoAction(description, undoAsync));
    }

    /// <inheritdoc />
    public async System.Threading.Tasks.Task<UndoExecutionResult> UndoAsync()
    {
        if (!this.undoActions.TryPop(out var action))
        {
            return UndoExecutionResult.None;
        }

        try
        {
            this.isUndoInProgress = true;
            await action.UndoAsync();
            return UndoExecutionResult.Success(action.Description);
        }
        finally
        {
            this.isUndoInProgress = false;
        }
    }

    private sealed record UndoAction(string Description, Func<System.Threading.Tasks.Task> UndoAsync);
}
