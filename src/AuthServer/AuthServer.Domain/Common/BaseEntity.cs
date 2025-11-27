namespace AuthServer.Domain.Common;

/// <summary>
///  Base entity class with common audit properties and generic ID support.
/// Developers can use any ID type: string, int, GUID, long, or Custom type.
/// </summary>
/// <typeparam name="TId">the type of the entity's identifier (e.g., string, Guid, int)</typeparam>
/// <remarks>
/// examples:
/// <code>
/// // Using string IDs (default in this template)
/// public class Tenant : BaseEntity<string>
///
/// // Using Guid IDs
/// public class Product : BaseEntity<Guid>
///
/// // Using int IDs (auto-increment databases)
/// public class Order : BaseEntity<int>
///
/// // Using custom composite key
/// public class CompositeEntity : BaseEntity<MyCustomId> { }
/// </code>
/// </remarks>
public class BaseEntity<TId> where TId : notnull
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;
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

