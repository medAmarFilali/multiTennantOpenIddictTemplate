using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MultiTenancy.Abstractions;
using MultiTenancy.Models;
using MultiTenancy.Resolvers;

namespace MultiTenancy.Tests.Resolvers;

public class CompositeTenantResolverTest
{
    [Fact]
    public async Task ResolveAsync_ReturnsNull_WhenNoResolversProvided()
    {
        // Arrange
        var resolver = new CompositeTenantResolver(Array.Empty<ITenantResolver>());
        var context = new DefaultHttpContext();

        // Act
        var result = await resolver.ResolveAsync(context);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_ReturnsNull_WhenAllResolversReturnNull()
    {
        var mockResolver1 = new Mock<ITenantResolver>();
        mockResolver1.Setup(x => x.ResolveAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync((TenantContext?)null);

        var mockResolver2 = new Mock<ITenantResolver>();
        mockResolver2.Setup(x => x.ResolveAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync((TenantContext?)null);

        var resolver = new CompositeTenantResolver(new[] { mockResolver1.Object, mockResolver2.Object });
        var context = new DefaultHttpContext();

        var result = await resolver.ResolveAsync(context);

        result.Should().BeNull();
        mockResolver1.Verify(x => x.ResolveAsync(context), Times.Once);
        mockResolver2.Verify(x => x.ResolveAsync(context), Times.Once);
    }

    [Fact]
    public async Task ResolveAsync_ReturnsFirstSuccessfulResolution()
    {
        // Arrange
        var mockResolver1 = new Mock<ITenantResolver>();
        mockResolver1.Setup(x => x.ResolveAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync((TenantContext?)null);

        var mockResolver2 = new Mock<ITenantResolver>();
        mockResolver2.Setup(x => x.ResolveAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new TenantContext { TenantId = "tenant-123" });

        var mockResolver3 = new Mock<ITenantResolver>();
        mockResolver3.Setup(x => x.ResolveAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new TenantContext { TenantId = "tenant-456" });

        var resolver = new CompositeTenantResolver(new[]
        {
            mockResolver1.Object,
            mockResolver2.Object,
            mockResolver3.Object
        });
        var context = new DefaultHttpContext();

        var result = await resolver.ResolveAsync(context);

        result.Should().NotBeNull();
        result!.TenantId.Should().Be("tenant-123");

        mockResolver1.Verify(x => x.ResolveAsync(context), Times.Once);
        mockResolver2.Verify(x => x.ResolveAsync(context), Times.Once);
        mockResolver3.Verify(x => x.ResolveAsync(context), Times.Never, "Should stop after first successful resolution");
    }

    [Fact]
    public async Task ResolveAsync_SkipsUnresolvedContexts()
    {
        // Arrage
        var mockResolver1 = new Mock<ITenantResolver>();
        mockResolver1.Setup(x => x.ResolveAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new TenantContext { TenantId = null });

        var mockResolver2 = new Mock<ITenantResolver>();
        mockResolver2.Setup(x => x.ResolveAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new TenantContext { TenantId = "tenant-123" });

        var resolver = new CompositeTenantResolver(new[] { mockResolver1.Object, mockResolver2.Object });
        var context = new DefaultHttpContext();

        var result = await resolver.ResolveAsync(context);

        result.Should().NotBeNull();
        result!.TenantId.Should().Be("tenant-123");
        mockResolver2.Verify(x => x.ResolveAsync(context), Times.Once);
    }

    [Fact]
    public async Task ResolveAsync_RespectsResolverOrder()
    {
        // Arrange
        var order = new List<string>();

        var mockResolver1 = new Mock<ITenantResolver>();
        mockResolver1.Setup(x => x.ResolveAsync(It.IsAny<HttpContext>()))
            .Callback(() => order.Add("resolver1"))
            .ReturnsAsync((TenantContext?)null);

        var mockResolver2 = new Mock<ITenantResolver>();
        mockResolver2.Setup(x => x.ResolveAsync(It.IsAny<HttpContext>()))
            .Callback(() => order.Add("resolver2"))
            .ReturnsAsync(new TenantContext { TenantId = "tenant-123" });

        var resolver = new CompositeTenantResolver(new[] { mockResolver1.Object, mockResolver2.Object });
        var context = new DefaultHttpContext();

        await resolver.ResolveAsync(context);

        order.Should().Equal("resolver1", "resolver2");
    }
}
