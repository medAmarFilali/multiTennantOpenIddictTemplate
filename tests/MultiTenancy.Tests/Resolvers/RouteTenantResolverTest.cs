using System.Xml.XPath;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MultiTenancy.Abstractions;
using MultiTenancy.Extensions;
using MultiTenancy.Models;

namespace MultiTenancy.Tests.Resolvers;

public class RouteTenantResolverTest
{
    [Fact]
    public async Task ResolveAsync_ReturnsNull_WhenPathIsEmpty()
    {
        var resolver = new RouteTenantResolver();
        var context = new DefaultHttpContext();
        context.Request.Path = "/";

        var result = await resolver.ResolveAsync(context);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_ReturnsNull_WhenPathDoesNotStartWithTenantPrefix()
    {
        var resolver = new RouteTenantResolver();
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/values";

        var result = await resolver.ResolveAsync(context);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_ReturnsTenantContext_WhenValidTenantKeyInPath()
    {
        // Arrange
        var resolver = new RouteTenantResolver();
        var context = new DefaultHttpContext();
        context.Request.Path = "/t/acme/api/users";

        // Act
        var result = await resolver.ResolveAsync(context);

        // Assert
        result.Should().NotBeNull();
        result!.TenantId.Should().Be("acme");
        result.TenantKey.Should().Be("acme");
        result.IsResolved.Should().BeTrue();
    }

    [Fact]
    public async Task ResolveAsync_ReturnsTenantContext_WithJustTenantKey()
    {
        // Arrange
        var resolver = new RouteTenantResolver();
        var context = new DefaultHttpContext();
        context.Request.Path = "/t/acme";

        // Act
        var result = await resolver.ResolveAsync(context);

        // Assert
        result.Should().NotBeNull();
        result!.TenantId.Should().Be("acme");
        result.TenantKey.Should().Be("acme");
    }

    [Fact]
    public async Task ResolveAsync_usesTenantStore_WhenProvided()
    {
        var mockStore = new Mock<ITenantStore>();
        mockStore.Setup(x => x.GetByKeyAsync("acme", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant
            {
                Id = "tenant-123",
                Key = "acme",
                Name = "Acme Corp"
            });

        var resolver = new RouteTenantResolver(mockStore.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/t/acme/api/users";

        var result = await resolver.ResolveAsync(context);

        result.Should().NotBeNull();
        result!.TenantId.Should().Be("tenant-123");
        result.TenantKey.Should().Be("acme");
        mockStore.Verify(x => x.GetByKeyAsync("acme", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResolveAsync_FallbackToKey_WhenTenantStoreReturnsNull()
    {
        // Arrange
        var mockStore = new Mock<ITenantStore>();
        mockStore.Setup(x => x.GetByKeyAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var resolver = new RouteTenantResolver(mockStore.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/t/unknown/api/users";

        var result = await resolver.ResolveAsync(context);

        result.Should().NotBeNull();
        result!.TenantId.Should().Be("unknown");
        result.TenantKey.Should().Be("unknown");
    }

    [Theory]
    [InlineData("/T/acne/api/users")]
    [InlineData("/T/ACME")]
    public async Task ResolveAsync_IsCaseInsensitive(string path)
    {
        // Arrange
        var resolver = new RouteTenantResolver();
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        // Act
        var result = await resolver.ResolveAsync(context);

        // Assert
        result.Should().NotBeNull();
        result!.IsResolved.Should().BeTrue();
    }
}
