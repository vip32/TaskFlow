namespace TaskFlow.Domain;

public class FocusSession
{
    public Guid Id { get; private set; }

    public Guid TaskId { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime EndedAt { get; private set; }

    public bool IsCompleted => this.EndedAt != DateTime.MinValue;

    public TimeSpan Duration
    {
        get
        {
            var endTime = DateTime.UtcNow;
            if (this.IsCompleted)
            {
                endTime = this.EndedAt;
            }

            return endTime - this.StartedAt;
        }
    }

    public FocusSession()
    {
        this.Id = Guid.NewGuid();
        this.TaskId = Guid.Empty;
        this.StartedAt = DateTime.UtcNow;
        this.EndedAt = DateTime.MinValue;
    }

    public FocusSession(Guid taskId)
        : this()
    {
        if (taskId != Guid.Empty)
        {
            this.TaskId = taskId;
        }
    }

    public void End()
    {
        if (this.IsCompleted)
        {
            return;
        }

        this.EndedAt = DateTime.UtcNow;
    }

    public void AttachToTask(Guid taskId)
    {
        if (taskId == Guid.Empty)
        {
            throw new ArgumentException("Task id cannot be empty.", nameof(taskId));
        }

        this.TaskId = taskId;
    }
}
