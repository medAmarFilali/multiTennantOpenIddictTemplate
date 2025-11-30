using AuthServer.Application.Common;
using AuthServer.Application.Users.DTOs;

namespace AuthServer.Application.Users;

/// <summary>
/// Service for managing users within a tenant.
/// All operations are automatically scoped to the current tenant via ITenantAccessor.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Registers a new user in the current tenant.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the created user or error message</returns>
    Task<Result<UserResponse>> RegisterUserAsync(RegisterUserRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID within the current tenant.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the user or error message</returns>
    Task<Result<UserResponse>> GetUserByIdAsync(string userId, CancellationToken cancellationtoken = default);

    /// <summary>
    /// Gets a user by username withn the current tenant.
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="cancellationtoken">Cancellation token</param>
    /// <returns>Result contaning the user or error message</returns>
    Task<Result<UserResponse>> GetUserByUsernameAsync(string username, CancellationToken cancellationtoken = default);

}
