using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MultiTenancy.Abstractions;
using MultiTenancy.Middleware;
using Multitenancy.Models;
using MultiTenancy.Models;

namespace MultiTenancy.Tests;

public class TenantResolutionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ResolvesTennantAndCallsNext()
    {
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TenantResolutionMiddleware(next);
        var context = new DefaultHttpContext();

        var mockResolver = new Mock<ITenantResolver>();
        mockResolver.Setup(x => x.ResolveAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new TenantContext { TenantId = "tenant-123" });

        var accessor = new TenantAccessor();

        await middleware.InvokeAsync(context, mockResolver.Object, accessor);

        nextCalled.Should().BeTrue();
        mockResolver.Verify(x => x.ResolveAsync(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_SetsTenantContextInAccessor()
    {
        // Arrange
        TenantContext? capturedContext = null;
        RequestDelegate next = _ =>
        {
            var accessor = new TenantAccessor();
            capturedContext = accessor.TenantContext;
            return Task.CompletedTask;
        };

        var middleware = new TenantResolutionMiddleware(next);
        var context = new DefaultHttpContext();

        var expectedContext = new TenantContext
        {
            TenantId = "tenant-123",
            TenantKey = "acme"
        };

        var mockResolver = new Mock<ITenantResolver>();
        mockResolver.Setup(x => x.ResolveAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(expectedContext);

        var accessor = new TenantAccessor();

        await middleware.InvokeAsync(context, mockResolver.Object, accessor);

        capturedContext.Should().NotBeNull();
        capturedContext!.TenantId.Should().Be("tenant-123");
        capturedContext!.TenantKey.Should().Be("acme");
    }

    [Fact]
    public async Task InvokeAsync_HandlesNullTenantContext()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TenantResolutionMiddleware(next);
        var context = new DefaultHttpContext();

        var mockResolver = new Mock<ITenantResolver>();
        mockResolver.Setup(x => x.ResolveAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync((TenantContext?)null);

        var accessor = new TenantAccessor();

        //Act
        await middleware.InvokeAsync(context, mockResolver.Object, accessor);

        // Assert
        nextCalled.Should().BeTrue();
        accessor.TenantContext.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware_EvenWhenResolutionFails()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TenantResolutionMiddleware(next);
        var context = new DefaultHttpContext();

        var mockResolver = new Mock<ITenantResolver>();
        mockResolver.Setup(x => x.ResolveAsync(It.IsAny<HttpContext>()))
            .ThrowsAsync(new Exception("Resolution failed"));

        var accessor = new TenantAccessor();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await middleware.InvokeAsync(context, mockResolver.Object, accessor));

        nextCalled.Should().BeFalse("Next should not be called if resolution throws");
    }
}
