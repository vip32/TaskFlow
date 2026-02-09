namespace TaskFlow.Domain;

public interface IProjectRepository
{
    global::System.Threading.Tasks.Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default);

    global::System.Threading.Tasks.Task<Project> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    global::System.Threading.Tasks.Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default);

    global::System.Threading.Tasks.Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default);

    global::System.Threading.Tasks.Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
