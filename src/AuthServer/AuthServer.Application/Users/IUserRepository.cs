using AuthServer.Domain.Entities;

namespace AuthServer.Application.Users;

/// <summary>
/// Repository for User entity operations.
/// Automatically scoped to the current tenant via global query filters.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by ID within the current tenant.
    /// </summary>
    Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by username withing the current tenant.
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email withing the current tenant.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a username exists withing the current tenant.
    /// </summary>
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email exists within the current tenant.
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new User.
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the database.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
