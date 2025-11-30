namespace AuthServer.Application.Users.DTOs;

/// <summary>
/// Request to register a new user
/// </summary>
public class RegisterUserRequest
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}
