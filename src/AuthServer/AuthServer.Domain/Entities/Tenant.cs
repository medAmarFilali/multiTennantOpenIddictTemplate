using AuthServer.Domain.Common;

namespace AuthServer.Domain.Entities;

/// <summary>
///  Represents a tenant in a multi-tenant application.
/// This is a partial class - developers can extend it in separate files.
/// </summary>
/// <remarks>
/// To extend this entity:
/// 1. Create a partial class file (e.g., Tenant.Custom.cs)
/// 2. Add you custom properties/methods
/// 3. override virtual methods to customize behavior
///
/// Example
/// <code>
/// public partial class Tenant
/// {
///     public string? CustomField { get; set; }
///
///     protected override void OnActivate()
///     {
/// 	    base.OnActivate();
///         // Custom logic here
///     }
/// }
/// </code>
/// </remarks>

public class Tenant : BaseEntity
{
    /// <summary>
    /// Unique key identifier for the tenant (e.g. "acme", "contoso").
    /// Always normalized to lowercase.
    /// </summary>
    public string Key { get; protected set; } = null!;

    /// <summary>
    /// Display name of the tenant.
    /// </summary>
    public string Name { get; protected set; } = null!;

    /// <summary>
    /// Indicates whether the tenant is active.
    /// Inactive tenants should not be able to access the application.
    /// </summary>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Protected constructor for creating a tenant.
    /// Derived classes can call this to customize initialization.
    /// </summary>
    protected Tenant(string id, string key, string name)
    {
        Id = id;
        Key = key.ToLowerInvariant();
        Name = name;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a new tenant.
    /// Override this in a partial calss to customize creation logic.
    /// </summary>
    public static Tenant Create(string id, string key, string name)
    {
        ValidateKey(key);
        ValidateName(name);

        return new Tenant(id, key, name);
    }

    /// <summary>
    /// Deactivates the tenant.
    /// Virtual method - can be overridden to add custom deactivation logic.
    /// </summary>
    public virtual void Deactivate()
    {
        IsActive = false;
        OnDeactivate();
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates the tenant.
    /// Virtual method - can be overridden to add custom activation logic.
    /// </summary>
    public virtual void Activate()
    {
        IsActive = true;
        OnActivate();
        MarkAsUpdated();
    }

    /// <summary>
    /// Hook method called when tenant is deactivated.
    /// Override this to add custom logic without replacing the entire method.
    /// </summary>
    protected virtual void OnDeactivate()
    {
        /// Intentionally empty - for derived classes to override
    }

    /// <summary>
    /// Updates the tenant name.
    /// Virtual method - can be overridden to add validation or business logic.
    /// </summary>
    public virtual void UpdateName(string name)
    {
        ValidateName(name);
        Name = name;
        MarkAsUpdated();
    }

    /// <summaru>
    /// Hook method called when tenant is activated.
    /// Override this to add custom logic wihtout replacing the entire activate method.
    /// </summaru>
    protected virtual void OnActivate()
    {
        /// Intentionally empty - for derived classes to override
    }

    /// <summary>
    /// Validates the tenant key.
    /// Can be replaced in a partial class to add custom validation.
    /// </summary>
    protected static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Tenant key cannot be null or whitespace.", nameof(key));
    }

    /// <summary>
    /// Validates the tenant name.
    /// Can be replaced in a partial class to add custom validation.
    /// </summary>
    protected static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be null or whitespace.", nameof(name));
    }
}
