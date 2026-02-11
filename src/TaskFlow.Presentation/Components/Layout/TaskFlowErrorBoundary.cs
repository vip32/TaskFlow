using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TaskFlow.Presentation.Services;

namespace TaskFlow.Presentation.Components.Layout;

/// <summary>
/// Error boundary that maps recoverable exceptions to toasts and keeps the UI responsive.
/// </summary>
public sealed class TaskFlowErrorBoundary : ErrorBoundary
{
    [Inject]
    private IAppExceptionHandler UiExceptionHandler { get; set; } = null!;

    /// <inheritdoc />
    protected override Task OnErrorAsync(Exception exception)
    {
        var isRecoverable = this.UiExceptionHandler.Handle(exception, "Route");
        if (isRecoverable)
        {
            // Recover immediately for expected errors so users can keep working.
            this.Recover();
        }

        return Task.CompletedTask;
    }
}
