using FluentAssertions;
using MultiTenancy.Models;

namespace MultiTenancy.Tests.Models;

public class TenantContextTests
{
    [Fact]
    public void IsResolved_ReturnsFalse_WhenTenantIdIsNull()
    {
        // Arrange
        var context = new TenantContext
        {
            TenantId = null
        };

        // Act
        var isResolved = context.IsResolved;

        // Assert
        isResolved.Should().BeFalse();
    }

    [Fact]
    public void IsResolved_ReturnsFalse_WhenTenantIdIsEmpty()
    {
        // Arrange
        var context = new TenantContext
        {
            TenantId = string.Empty
        };

        // Act
        var isResolved = context.IsResolved;

        // Assert
        isResolved.Should().BeFalse();
    }

    [Fact]
    public void IsResolved_ReturnsTrue_WhenTenantIdIsSet()
    {
        var context = new TenantContext
        {
            TenantId = "tenant-123"
        };

        var isResolved = context.IsResolved;

        // Assert
        isResolved.Should().BeTrue();
    }

    [Fact]
    public void TenantContext_CanSetTenantKey()
    {
        // Arrange
        var context = new TenantContext
        {
            TenantId = "tenant-123",
            TenantKey = "acme"
        };

        // Assert
        context.TenantKey.Should().Be("acme");
    }
}
