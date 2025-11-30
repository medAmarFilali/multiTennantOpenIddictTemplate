using AuthServer.Application.Common;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Infrastructure.Services;

/// <summary>
/// Oasswird hasher implementation using ASP.BET Core Identity's PasswordHasher.
/// Uses PBKDF2 with HMAC-SHA256
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher;

    public PasswordHasher()
    {
        _hasher = new PasswordHasher<object>();
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        return _hasher.HashPassword(null!, password);
    }

    public bool VerifyHashedPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        if (string.IsNullOrEmpty(hash))
            throw new ArgumentException("Hash cannot be null or empty.", nameof(hash));

        var result = _hasher.VerifyHashedPassword(null!, hash, password);
        return result != PasswordVerificationResult.Failed;
    }
}
