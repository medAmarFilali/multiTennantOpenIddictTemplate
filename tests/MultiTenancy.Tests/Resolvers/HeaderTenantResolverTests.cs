using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MultiTenancy.Resolvers;

namespace MultiTenancy.Tests.Resolvers;

public class HeaderTenantResolverTests
{
    private readonly HeaderTenantResolver _resolver;

    public HeaderTenantResolverTests()
    {
        _resolver = new HeaderTenantResolver();
    }

    [Fact]
    public async Task ResolveAsync_ReturnsNull_WhenHeaderIsMissing()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = await _resolver.ResolveAsync(context);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_ReturnsNull_WhenHeaderIsEmpty()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Tenant-Id"] = string.Empty;

        // Act
        var result = await _resolver.ResolveAsync(context);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_ReturnsNull_WhenHeaderIsWhitespace()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Tenant-Id"] = "   ";

        // Act
        var result = await _resolver.ResolveAsync(context);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_ReturnsNull_WhenHeaderIsPresent()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Tenant-Id"] = "tenant-123";
        context.Request.Headers["Authorization"] = "Bearer 123";

        // Act
        var result = await _resolver.ResolveAsync(context);

        // Assert
        result.Should().NotBeNull();
        result!.TenantId.Should().Be("tenant-123");
        result.IsResolved.Should().BeTrue();
    }

    [Fact]
    public async Task ResolveAsync_UsesFirstValue_WhenMultipleHeaderValuesProvided()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Tenant-Id"] = new[] { "tenant-123", "tenant-456" };

        // Act
        var result = await _resolver.ResolveAsync(context);

        // Assert
        result.Should().NotBeNull();
        result!.TenantId.Should().Be("tenant-123");
    }

}
