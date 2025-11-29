using AuthServer.Domain.Common;
using MultiTenancy.Entities;

namespace AuthServer.Domain.Entities;

/// <summary>
///  Represents a user in the authentication system.
/// This is a partial class - developers can extend it in separate files.
/// Implements <see cref="ITenantEntity"/> for multi-tenant isolation.
/// </summary>
/// <remarks>
/// To extend this entity:
/// 1. Create a partial class file (e.g., User.Custom.cs
/// 2. Add your custom properties (e.g., FirstName, LastName, Phone)
/// 3. Override virtual methods to customize behavior
///
/// Example
/// <code>
/// public partial class User
/// {
///     public string? FirstName { get; set; }
///     public string? LastName { get; set; }
///     public List[string] Roles { get; set; } = new();
///
///     protected override void OnActivate()
///     {
///         base.OnActivate();
///         // Sen welcome email, etc.
///     }
/// }
/// </code>
/// </remarks>
public partial class User : BaseEntity<string>, ITenantEntity
{
    /// <summary>
    /// The tenant this user belongs to.
    /// Required for multi-tenant isolation.
    /// </summary>
    public string TenantId { get; set; } = null!;

    /// <summary>
    /// Unique username for authentication.
    /// Should be unique within the tenant.
    /// </summary>
    public string UserName { get; protected set; } = null!;

    /// <summary>
    /// User's email address.
    /// Can be used for authentication.
    /// </summary>
    public string Email { get; protected set; } = null!;

    /// <summary>
    /// Hashed password for authentication.
    /// Never store plain text passwords.
    /// </summary>
    public string PasswordHash { get; protected set; } = null!;

    /// <summary>
    /// Indicates whther the user account is active.
    /// Inactive users cannot authenticate.
    /// </summary>
    public bool IsActive { get; protected set; }

    /// <summary>
    /// Protected constructor for creating a new user.
    /// Derived classes can call this customize initialization.
    /// </summary>
    protected User(string id, string tenantId, string userName, string email, string passwordHash)
    {
        Id = id;
        TenantId = tenantId;
        UserName = userName;
        Email = email;
        PasswordHash = passwordHash;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a new user.
    /// Override this in a partial class to customize create logic.
    /// </summary>
    public static User Create(string id, string tenantId, string username, string email, string passwordHash)
    {
        ValidateTenantId(tenantId);
        ValidateUsername(username);
        ValidateEmail(email);
        ValidatePasswordHash(passwordHash);

        return new User(id, tenantId, username, email, passwordHash);
    }

    /// <summary>
    /// Deactivates the user account.
    /// Virtual method - can be overridden to add custom deactivation logic.
    /// </summary>
    public virtual void Deactivate()
    {
        IsActive = false;
        OnDeactivate();
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates the user account.
    /// Virtual method - can be overridden to add custom activation logic.
    /// </summary>
    public virtual void Activate()
    {
        IsActive = true;
        OnActivate();
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the user's email address.
    /// Virtual method - can be overridden to add validation or send notifications.
    /// </summary>
    public virtual void UpdateEmail(string email)
    {
        ValidateEmail(email);
        Email = email;
        OnEmailUpdated();
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the user's password hash.
    /// Virtual method - can be overriden to add password history, etc.
    /// </summary>
    public virtual void UpdatePasswordHash(string passwordHash)
    {
        ValidatePasswordHash(passwordHash);
        PasswordHash = passwordHash;
        OnPasswordUpdated();
        MarkAsUpdated();
    }

    /// <summary>
    /// Hook method called when user is deactivated.
    /// Override this to add custom logic (e.g., revoke tokens, send notifications).
    /// </summary>
    protected virtual void OnDeactivate()
    {
        /// Intentionally empty - for derived classes to override
    }

    /// <summary>
    /// Hook method called when user is activated.
    /// Override this to add custom logic (e.g., send welcome email).
    /// </summary>
    protected virtual void OnActivate()
    {
        /// Intentionally empty - for derived classes to override
    }

    /// <summary>
    /// Hook method called when email is updated.
    /// override this to add custom logic (e.g., invalidate sessions).
    /// </summary>
    protected virtual void OnEmailUpdated()
    {
        /// Intentionally empty - for derived classes to override
    }

    /// <summary>
    /// Hook method called when password is updated.
    /// Override this to add custom logic (e.g., invalidate sessions).
    /// </summary>
    protected virtual void OnPasswordUpdated()
    {
        /// Intentionally empty - for derived classes to override
    }

    /// <summary>
    /// Validates the tenant ID.
    /// can be replaced in a partial class to add custom validation.
    /// </summary>
    protected static void ValidateTenantId(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant ID cannot be null or whitespace.", nameof(tenantId));
    }

    /// <summary>
    /// Validates the username.
    /// Can be replaced in a partial class to add custom validation (e.g., format, length).
    /// </summary>
    protected static void ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be nuill or whitespace.", nameof(username));
    }

    /// <summary>
    /// Validates the email address.
    /// Can be replaced in a partial class to add custom validation (e.g., format, domain restrictions).
    /// </summary>
    protected static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email address cannot be null or whitespace.", nameof(email));
    }

    /// <summary>
    /// Validates the password hash.
    /// Can be replaced in a partial class to add custom validation.
    /// </summary>
    protected static void ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be null or whitespace.", nameof(passwordHash));
    }


}
