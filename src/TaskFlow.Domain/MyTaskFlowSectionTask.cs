namespace TaskFlow.Domain;

/// <summary>
/// Represents explicit manual task membership in a My Task Flow section.
/// </summary>
public class MyTaskFlowSectionTask
{
    /// <summary>
    /// Gets section identifier.
    /// </summary>
    public Guid SectionId { get; private set; }

    /// <summary>
    /// Gets task identifier.
    /// </summary>
    public Guid TaskId { get; private set; }

    private MyTaskFlowSectionTask()
    {
    }

    /// <summary>
    /// Initializes a new section task membership link.
    /// </summary>
    /// <param name="sectionId">Section identifier.</param>
    /// <param name="taskId">Task identifier.</param>
    public MyTaskFlowSectionTask(Guid sectionId, Guid taskId)
    {
        if (sectionId == Guid.Empty)
        {
            throw new ArgumentException("Section id cannot be empty.", nameof(sectionId));
        }

        if (taskId == Guid.Empty)
        {
            throw new ArgumentException("Task id cannot be empty.", nameof(taskId));
        }

        this.SectionId = sectionId;
        this.TaskId = taskId;
    }
}
