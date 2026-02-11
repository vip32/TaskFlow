namespace TaskFlow.Domain;

/// <summary>
/// Represents a missing domain entity.
/// </summary>
public sealed class EntityNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    /// <param name="entityName">Entity type name.</param>
    /// <param name="entityId">Missing entity identifier.</param>
    public EntityNotFoundException(string entityName, Guid entityId)
        : base($"{entityName} with id '{entityId}' was not found.")
    {
        this.EntityName = entityName;
        this.EntityId = entityId;
    }

    /// <summary>
    /// Gets the missing entity type name.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Gets the missing entity identifier.
    /// </summary>
    public Guid EntityId { get; }
}
