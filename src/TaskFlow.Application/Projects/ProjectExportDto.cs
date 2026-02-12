using TaskFlow.Domain;

namespace TaskFlow.Application;

/// <summary>
/// Serializable project payload for import/export workflows.
/// </summary>
public sealed record ProjectExportDto
{
    /// <summary>
    /// Gets project identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets project name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets project color.
    /// </summary>
    public string Color { get; init; } = string.Empty;

    /// <summary>
    /// Gets project icon.
    /// </summary>
    public string Icon { get; init; } = string.Empty;

    /// <summary>
    /// Gets project note.
    /// </summary>
    public string Note { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this is the default project.
    /// </summary>
    public bool IsDefault { get; init; }

    /// <summary>
    /// Gets project preferred view type.
    /// </summary>
    public ProjectViewType ViewType { get; init; }

    /// <summary>
    /// Gets project creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets project tags.
    /// </summary>
    public List<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets top-level tasks belonging to this project.
    /// </summary>
    public List<TaskExportDto> Tasks { get; init; } = [];
}
