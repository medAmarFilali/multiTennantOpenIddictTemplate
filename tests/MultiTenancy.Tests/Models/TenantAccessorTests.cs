using FluentAssertions;
using MultiTenancy.Abstractions;
using Multitenancy.Models;
using MultiTenancy.Models;

namespace MultiTenancy.Tests.Models;

public class TenantAccessorTests
{
    [Fact]
    public void TenantContext_CanBeSetAndRetrieved()
    {
        // Arrange
        var accessor = new TenantAccessor();
        var tenantContext = new TenantContext
        {
            TenantId = "tenant-123",
            TenantKey = "acme"
        };

        // Act
        accessor.TenantContext = tenantContext;

        // Assert
        accessor.TenantContext.Should().NotBeNull();
        accessor.TenantContext!.TenantId.Should().Be("tenant-123");
        accessor.TenantContext!.TenantKey.Should().Be("acme");
    }

    [Fact]
    public void TenantContext_IsNull_ByDefault()
    {
        // Arrange
        var accessor = new TenantAccessor();

        // Act & Assert
        accessor.TenantContext.Should().BeNull();
    }

    [Fact]
    public void TenantId_ReturnsNull_WhenTenantContextIsNull()
    {
        // Arrange
        var accessor = (ITenantAccessor)new TenantAccessor();

        // Act & Assert
        accessor.TenantId.Should().BeNull();
    }

    [Fact]
    public async Task TenantContext_IsIsolatedBetweenAsyncContexts()
    {
        // Arrange
        var accessor = new TenantAccessor();

        // Act
        var task1 = Task.Run(() =>
        {
            accessor.TenantContext = new TenantContext { TenantId = "tenant-1" };
            Thread.Sleep(50);
            return ((ITenantAccessor)accessor).TenantId;
        });

        var task2 = Task.Run(() =>
        {
            accessor.TenantContext = new TenantContext { TenantId = "tenant-2" };
            Thread.Sleep(50);
            return ((ITenantAccessor)accessor).TenantId;
        });

        var results = await Task.WhenAll(task1, task2);

        results[0].Should().Be("tenant-1", "Each async context should have its own tenant");
        results[1].Should().Be("tenant-2", "Each async context should have its own tenant");
    }

    [Fact]
    public void TenantContext_CanBeCleared()
    {
        // Arrange
        var accessor = new TenantAccessor();
        accessor.TenantContext = new TenantContext { TenantId = "tenant-123" };

        // Act
        accessor.TenantContext = null;

        // Assert
        accessor.TenantContext.Should().BeNull();
        ((ITenantAccessor)accessor).TenantId.Should().BeNull();
    }
}
