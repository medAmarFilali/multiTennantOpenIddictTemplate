using AuthServer.Domain.Entities;
using FluentAssertions;
using MultiTenancy.Entities;

namespace AuthServer.UnitTests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void Create_ShouldCreateUserWithRequiredProperties()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var tenantId = "tenant-123";
        var username = "john.doe";
        var email = "john.doe@acme.com";
        var passwordHash = "hashed_password";

        // Act
        var user = User.Create(id, tenantId, username, email, passwordHash);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().Be(id);
        user.TenantId.Should().Be(tenantId);
        user.UserName.Should().Be(username);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.IsActive.Should().BeTrue("new users should be active by default");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void User_ShouldImplementITenantEntity()
    {
        // Arrange
        var user = User.Create(
            Guid.NewGuid().ToString(),
            "tenant-123",
            "john.doe",
            "john@example.com",
            "hash"
        );

        // Assert
        user.Should().BeAssignableTo<ITenantEntity>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowArgumentException_WhenTenantIdIsNullOrWhitespace(string invalidTenantId)
    {
        // Act
        var act = () => User.Create(
            Guid.NewGuid().ToString(),
            invalidTenantId,
            "username",
            "email@example.com",
            "hash"
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*tenantId*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowArgumentException_WhenUsernameIsNullOrWhitespace(string invalidUsername)
    {
        // Act
        var act = () => User.Create(
            Guid.NewGuid().ToString(),
            "tenant-123",
            invalidUsername,
            "email@example.com",
            "hash"
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*username*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowArgumentException_WhenEmailIsNullOrWhitespace(string invalidEmail)
    {
        // Act
        var act = () => User.Create(
            Guid.NewGuid().ToString(),
            "tenant-123",
            "username",
            invalidEmail,
            "hash"
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*email*");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = User.Create(
            Guid.NewGuid().ToString(),
            "tenant-123",
            "john.doe",
            "john@example.com",
            "hash"
        );

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = User.Create(
            Guid.NewGuid().ToString(),
            "tenant-123",
            "john.doe",
            "john@example.com",
            "hash"
        );
        user.Deactivate();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateEmail_ShouldUpdateUserEmail()
    {
        // Arrange
        var user = User.Create(
            Guid.NewGuid().ToString(),
            "tenant-123",
            "john.doe",
            "john@example.com",
            "hash"
        );
        var newEmail = "john.doe.new@acme.com";

        // Act
        user.UpdateEmail(newEmail);

        // Assert
        user.Email.Should().Be(newEmail);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateEmail_ShouldThrowArgumentException_WhenEmailIsNullOrWhitespace(string invalidEmail)
    {
        // Arrange
        var user = User.Create(
            Guid.NewGuid().ToString(),
            "tenant-123",
            "john.doe",
            "john@acme.com",
            "hash"
        );

        // Act
        var act = () => user.UpdateEmail(invalidEmail);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*email*");
    }

    [Fact]
    public void UpdatePasswordHash_ShouldUpdateUserPasswordHash()
    {
        // Arrange
        var user = User.Create(
            Guid.NewGuid().ToString(),
            "tenant-123",
            "john.doe",
            "john@acme.com",
            "old_hash"
        );
        var newHash = "new_hash";

        // Act
        user.UpdatePasswordHash(newHash);

        // Assert
        user.PasswordHash.Should().Be(newHash);
        user.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdatePassowrdHash_ShouldThrowArgumentException_WhenHashIsNullOrWhitespace(string invalidHash)
    {
        // Arrange
        var user = User.Create(
            Guid.NewGuid().ToString(),
            "tenant-123",
            "john.doe",
            "doe@acme.com",
            "hash"
        );

        // Act
        var act = () => user.UpdatePasswordHash(invalidHash);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*password*");
    }
}
