namespace AuthServer.Domain.Common;

/// <summary>
/// Base entity class with common audit properties
/// Developers can inherit from this to add custom audit fields.
/// </summary>
public class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public string Id { get; protected set; } = null!;
    /// <summary>
    /// UTC Timestamp when the entity was creted.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// UTC timestamp when the entity was last updated.
    /// Null is never updated.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Marks the entity as updated with current UTC timestamp.
    /// Can be overriden to add custom update logic.
    /// </summary>
    protected virtual void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

