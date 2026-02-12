namespace TaskFlow.Presentation.Services;

/// <summary>
/// Default in-memory undo manager for UI actions.
/// </summary>
public sealed class UndoManager : IUndoManager
{
    private readonly Microsoft.Extensions.Logging.ILogger<UndoManager> logger;
    private readonly int maxActions;
    private readonly Stack<UndoAction> undoActions = [];
    private bool isUndoInProgress;

    /// <summary>
    /// Initializes a new instance of the <see cref="UndoManager"/> class.
    /// </summary>
    /// <param name="maxActions">Maximum number of stored undo actions before the stack is reset.</param>
    public UndoManager(int maxActions = 20)
        : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<UndoManager>.Instance, maxActions)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UndoManager"/> class.
    /// </summary>
    /// <param name="logger">Logger used for undo registration and execution events.</param>
    /// <param name="maxActions">Maximum number of stored undo actions before the stack is reset.</param>
    public UndoManager(Microsoft.Extensions.Logging.ILogger<UndoManager> logger, int maxActions = 20)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxActions);

        this.logger = logger;
        this.maxActions = maxActions;
    }

    /// <inheritdoc />
    public void Register(string description, Func<System.Threading.Tasks.Task> undoAsync)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(undoAsync);

        if (this.isUndoInProgress)
        {
            this.logger.LogInformation("Undo action registration skipped for '{Description}' because an undo is currently in progress.", description);
            return;
        }

        if (this.undoActions.Count >= this.maxActions)
        {
            this.logger.LogInformation(
                "Undo stack reached max capacity ({MaxActions}). Clearing {ActionCount} pending actions before registering '{Description}'.",
                this.maxActions,
                this.undoActions.Count,
                description);
            this.undoActions.Clear();
        }

        this.undoActions.Push(new UndoAction(description, undoAsync));
        this.logger.LogInformation(
            "Registered undo action '{Description}'. Pending undo actions: {ActionCount}.",
            description,
            this.undoActions.Count);
    }

    /// <inheritdoc />
    public async System.Threading.Tasks.Task<UndoExecutionResult> UndoAsync()
    {
        if (!this.undoActions.TryPop(out var action))
        {
            this.logger.LogInformation("Undo requested but no pending actions are available.");
            return UndoExecutionResult.None;
        }

        try
        {
            this.logger.LogInformation(
                "Executing undo action '{Description}'. Remaining pending actions before execution: {ActionCount}.",
                action.Description,
                this.undoActions.Count);
            this.isUndoInProgress = true;
            await action.UndoAsync();
            this.logger.LogInformation(
                "Undo action '{Description}' completed successfully. Pending undo actions: {ActionCount}.",
                action.Description,
                this.undoActions.Count);
            return UndoExecutionResult.Success(action.Description);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Undo action '{Description}' failed.", action.Description);
            throw;
        }
        finally
        {
            this.isUndoInProgress = false;
        }
    }

    private sealed record UndoAction(string Description, Func<System.Threading.Tasks.Task> UndoAsync);
}
