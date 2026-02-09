using Injectio.Attributes;
using TaskFlow.Domain;
using DomainTask = TaskFlow.Domain.Task;
using DomainTaskStatus = TaskFlow.Domain.TaskStatus;

namespace TaskFlow.Application;

/// <summary>
/// Default application orchestrator for task use cases.
/// </summary>
[RegisterScoped(ServiceType = typeof(ITaskOrchestrator))]
public sealed class TaskOrchestrator : ITaskOrchestrator
{
    private readonly ITaskRepository taskRepository;
    private readonly ITaskHistoryRepository taskHistoryRepository;
    private readonly ICurrentSubscriptionAccessor currentSubscriptionAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskOrchestrator"/> class.
    /// </summary>
    /// <param name="taskRepository">Task repository.</param>
    /// <param name="taskHistoryRepository">Task history repository.</param>
    /// <param name="currentSubscriptionAccessor">Current subscription accessor.</param>
    public TaskOrchestrator(ITaskRepository taskRepository, ITaskHistoryRepository taskHistoryRepository, ICurrentSubscriptionAccessor currentSubscriptionAccessor)
    {
        this.taskRepository = taskRepository;
        this.taskHistoryRepository = taskHistoryRepository;
        this.currentSubscriptionAccessor = currentSubscriptionAccessor;
    }

    /// <inheritdoc/>
    public global::System.Threading.Tasks.Task<List<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return this.taskRepository.GetByProjectIdAsync(projectId, cancellationToken);
    }

    /// <inheritdoc/>
    public global::System.Threading.Tasks.Task<List<DomainTask>> SearchAsync(Guid projectId, string query, CancellationToken cancellationToken = default)
    {
        return this.taskRepository.SearchAsync(query, projectId, cancellationToken);
    }

    /// <inheritdoc/>
    public global::System.Threading.Tasks.Task<List<string>> GetNameSuggestionsAsync(string prefix, bool isSubTaskName, int take = 20, CancellationToken cancellationToken = default)
    {
        return this.taskHistoryRepository.GetSuggestionsAsync(prefix, isSubTaskName, take, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> CreateAsync(Guid projectId, string title, TaskPriority priority, string note, CancellationToken cancellationToken = default)
    {
        var subscriptionId = this.currentSubscriptionAccessor.GetCurrentSubscription().Id;
        var task = new DomainTask(subscriptionId, title, projectId);
        task.SetPriority(priority);
        task.UpdateNote(note);
        var created = await this.taskRepository.AddAsync(task, cancellationToken);
        await this.taskHistoryRepository.RegisterUsageAsync(created.Title, false, cancellationToken);
        return created;
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> UpdateTitleAsync(Guid taskId, string newTitle, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.UpdateTitle(newTitle);
        var updated = await this.taskRepository.UpdateAsync(task, cancellationToken);
        await this.taskHistoryRepository.RegisterUsageAsync(updated.Title, updated.ParentTaskId != Guid.Empty, cancellationToken);
        return updated;
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> UpdateNoteAsync(Guid taskId, string newNote, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.UpdateNote(newNote);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> SetPriorityAsync(Guid taskId, TaskPriority priority, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.SetPriority(priority);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> SetStatusAsync(Guid taskId, DomainTaskStatus status, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.SetStatus(status);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> ToggleFocusAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.ToggleFocus();
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public async global::System.Threading.Tasks.Task<DomainTask> MoveToProjectAsync(Guid taskId, Guid newProjectId, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(taskId, cancellationToken);
        task.MoveToProject(newProjectId);
        return await this.taskRepository.UpdateAsync(task, cancellationToken);
    }

    /// <inheritdoc/>
    public global::System.Threading.Tasks.Task<bool> DeleteAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return this.taskRepository.DeleteAsync(taskId, cancellationToken);
    }
}
