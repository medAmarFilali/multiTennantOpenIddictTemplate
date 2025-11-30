using AuthServer.Application.Common;
using AuthServer.Application.Users.DTOs;
using AuthServer.Domain.Entities;
using MultiTenancy.Abstractions;

namespace AuthServer.Application.Users;

/// <summary>
/// Sercice for managing users within a tenant.
/// All operations are autmatically scoped to the current tenant.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantAccessor _tenantAccessor;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITenantAccessor tenantAccessor)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tenantAccessor = tenantAccessor;
    }

    public async Task<Result<UserResponse>> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.TenantContext.TenantId;
        if (string.IsNullOrEmpty(tenantId))
            return Result.Failure<UserResponse>("Tenant context is not set.");

        // Check if username already exists in this tenant
        if (await _userRepository.UsernameExistsAsync(request.Username, cancellationToken))
            return Result.Failure<UserResponse>("Username already exists in this tenant");

        // Check if email already exists in this tenant
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
            return Result.Failure<UserResponse>("Email already exists in this tenant");

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var userId = Guid.NewGuid().ToString();
        var user = User.Create(userId, tenantId, request.Username, request.Email, passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToResponse(user));
    }

    public async Task<Result<UserResponse>> GetUserByIdAsync(string userId, CancellationToken cancellationtoken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationtoken);
        if (user == null)
            return Result.Failure<UserResponse>("User not found");

        return Result.Success(MapToResponse(user));
    }

    public async Task<Result<UserResponse>> GetUserByUsernameAsync(string username, CancellationToken cancellationtoken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationtoken);
        if (user == null)
            return Result.Failure<UserResponse>("User not found");
        return Result.Success(MapToResponse(user));
    }

    /// <summary>
    /// Maps a User domain entity to UserResponse DTO.
    /// Simple, explicit mapping - no reflection overhead.
    /// </summary>
    private static UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            TenantId = user.TenantId,
            Username = user.UserName,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
    }
}
