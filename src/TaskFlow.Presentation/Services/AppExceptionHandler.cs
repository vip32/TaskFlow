using MudBlazor;
using TaskFlow.Domain;

namespace TaskFlow.Presentation.Services;

/// <summary>
/// Default UI exception handler for user-friendly feedback and structured logs.
/// </summary>
public sealed class AppExceptionHandler : IAppExceptionHandler
{
    private readonly ISnackbar snackbar;
    private readonly ILogger<AppExceptionHandler> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppExceptionHandler"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="snackbar">Snackbar service.</param>
    public AppExceptionHandler(ILogger<AppExceptionHandler> logger, ISnackbar snackbar)
    {
        this.logger = logger;
        this.snackbar = snackbar;
    }

    /// <inheritdoc />
    public bool Handle(Exception exception, string context)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(context);

        switch (exception)
        {
            case OperationCanceledException:
                this.logger.LogDebug(exception, "UI - Exception: operation canceled in {Context}", context);
                return true;
            case EntityNotFoundException notFound:
                this.logger.LogWarning(exception, "UI - Exception: entity not found in {Context}. EntityName={EntityName}, EntityId={EntityId}", context, notFound.EntityName, notFound.EntityId);
                this.snackbar.Add($"{notFound.EntityName} was not found. The view was refreshed.", Severity.Warning);
                return true;
            case InvalidOperationException:
            case ArgumentException:
                this.logger.LogWarning(exception, "UI - Exception: recoverable error in {Context}", context);
                this.snackbar.Add(exception.Message, Severity.Warning);
                return true;
            default:
                this.logger.LogError(exception, "UI - Exception: unhandled error in {Context}", context);
                this.snackbar.Add("An unexpected error occurred.", Severity.Error);
                return false;
        }
    }
}
