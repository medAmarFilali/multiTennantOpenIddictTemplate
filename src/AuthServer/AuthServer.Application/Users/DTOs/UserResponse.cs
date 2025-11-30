namespace AuthServer.Application.Users.DTOs;

public class UserResponse
{
    public required string Id { get; set; }
    public required string TenantId { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
