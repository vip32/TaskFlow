using Microsoft.Extensions.DependencyInjection;

namespace TaskFlow.Application;

/// <summary>
/// Registers application orchestration services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application layer orchestrators to dependency injection.
    /// </summary>
    /// <param name="services">Service collection instance.</param>
    /// <returns>Service collection for chaining.</returns>
    public static IServiceCollection AddTaskFlowApplication(this IServiceCollection services)
    {
        services.AddScoped<IProjectOrchestrator, ProjectOrchestrator>();
        services.AddScoped<ITaskOrchestrator, TaskOrchestrator>();
        return services;
    }
}
