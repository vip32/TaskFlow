namespace TaskFlow.Domain;

/// <summary>
/// Defines the visual representation mode for a project.
/// </summary>
public enum ProjectViewType
{
    /// <summary>
    /// Tasks are shown in a linear list.
    /// </summary>
    List = 0,

    /// <summary>
    /// Tasks are shown in board columns.
    /// </summary>
    Board = 1,
}
