namespace TaskFlow.Domain;

public interface ITaskRepository
{
    global::System.Threading.Tasks.Task<List<Task>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    global::System.Threading.Tasks.Task<List<Task>> GetByPriorityAsync(TaskPriority priority, Guid projectId, CancellationToken cancellationToken = default);

    global::System.Threading.Tasks.Task<List<Task>> SearchAsync(string query, Guid projectId, CancellationToken cancellationToken = default);

    global::System.Threading.Tasks.Task<List<Task>> GetFocusedAsync(Guid projectId, CancellationToken cancellationToken = default);

    global::System.Threading.Tasks.Task<Task> AddAsync(Task task, CancellationToken cancellationToken = default);

    global::System.Threading.Tasks.Task<Task> UpdateAsync(Task task, CancellationToken cancellationToken = default);

    global::System.Threading.Tasks.Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    global::System.Threading.Tasks.Task<Task> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
