using AuthServer.Domain.Entities;
using FluentAssertions;

namespace AuthServer.UnitTests.Domain.Entities;

public class TenantTests
{
    [Fact]
    public void Create_ShouldCreatetenantWithRequiredProperties()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var key = "acme";
        var name = "Acme Corporation";

        // Act
        var tenant = Tenant.Create(id, key, name);

        // Assert
        tenant.Should().NotBeNull();
        tenant.Id.Should().Be(id);
        tenant.Key.Should().Be(key);
        tenant.Name.Should().Be(name);
        tenant.IsActive.Should().BeTrue("new tenants should be active by default");
        tenant.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        tenant.UpdatedAt.Should().BeNull("New tenants haven't been updated yet");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("     ")]
    public void Create_ShouldThrowArgumentException_WhenKeyIsNullOrEmpty(string invalidKey)
    {
        /// Arrange
        var id = Guid.NewGuid().ToString();
        var name = "Acme Corporation";

        // Act
        var act = () => Tenant.Create(id, invalidKey, name);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*key*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowArgumentException_WhenNameIsNullOrWhitespace(string invalidName)
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var key = "acme";

        // Act
        var act = () => Tenant.Create(id, key, invalidName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var tenant = Tenant.Create(Guid.NewGuid().ToString(), "acme", "Acme Corporation");

        // Act
        tenant.Deactivate();

        // Assert
        tenant.IsActive.Should().BeFalse();
        tenant.UpdatedAt.Should().NotBeNull();
        tenant.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var tenant = Tenant.Create(Guid.NewGuid().ToString(), "acme", "Acme Corporation");
        tenant.Deactivate();

        // Act
        tenant.Activate();

        // Assert
        tenant.IsActive.Should().BeTrue();
        tenant.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateName_ShouldUpdateTenantName()
    {
        // Arrange
        var tenant = Tenant.Create(Guid.NewGuid().ToString(), "acme", "Acme Corporation");
        var newName = "Acme Corporation, Inc.";

        // Act
        tenant.UpdateName(newName);

        // Assert
        tenant.Name.Should().Be(newName);
        tenant.UpdatedAt.Should().NotBeNull();
        tenant.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_ShouldThrowArgumentException_WHenNameIsNullOrWhitespace(string invalidName)
    {
        // Arrange
        var tenant = Tenant.Create(Guid.NewGuid().ToString(), "acme", "Acme Corporation");

        // Act
        var act = () => tenant.UpdateName(invalidName);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*");
    }

    [Fact]
    public void Key_ShouldBeLowerCase()
    {
        // Arrange & Act
        var tenant = Tenant.Create(Guid.NewGuid().ToString(), "ACME", "Acme Corporation");

        // Assert
        tenant.Key.Should().Be("acme");
    }
    

}
