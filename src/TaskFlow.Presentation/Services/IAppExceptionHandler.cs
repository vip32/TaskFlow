namespace TaskFlow.Presentation.Services;

/// <summary>
/// Handles exceptions that occur in the UI layer and maps them to user-facing feedback.
/// </summary>
public interface IAppExceptionHandler
{
    /// <summary>
    /// Handles the given exception and returns whether the exception is considered recoverable.
    /// </summary>
    /// <param name="exception">Exception instance.</param>
    /// <param name="context">Short context label for logging.</param>
    /// <returns>True if UI can recover without rendering a fatal error state.</returns>
    bool Handle(Exception exception, string context);
}
