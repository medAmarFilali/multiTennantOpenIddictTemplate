using AuthServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MultiTenancy.Abstractions;
using AuthServer.Domain.Entities;
using FluentAssertions;

namespace AuthServer.UnitTests.Infrastrcture;

public class TenantStoreTests
{
    private AuthServerDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthServerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AuthServerDbContext(options, null);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTenant_WhenTenantExists()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var tenant = Tenant.Create("tenant-123", "acme", "Acme Corporation");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var store = new TenantStore(context);

        // Act
        var result = await store.GetByIdAsync("tenant-123");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("tenant-123");
        result.Key.Should().Be("acme");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTenantDoesNotExist()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var store = new TenantStore(context);

        // Act
        var result = await store.GetByIdAsync("non-existent-tenant");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByKeyAsync_ShouldReturntenant_WhenTenantExists()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var tenant = Tenant.Create("tenant-456", "contoso", "Contoso Ltd");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        var store = new TenantStore(context);

        // Act
        var result = await store.GetByKeyAsync("contoso");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("tenant-456");
        result.Key.Should().Be("contoso");
    }

    [Fact]
    public async Task GetByKeyAsync_ShouldReturnNull_WhenTenantDoesNotExist()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var store = new TenantStore(context);

        // Act
        var result = await store.GetByKeyAsync("non-existent-tenant");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByKeyAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var tenant = Tenant.Create("tenant-789", "fabrikam", "Fabrikam Inc.");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var store = new TenantStore(context);

        // Act
        var result = await store.GetByKeyAsync("FABRIKAM");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("tenant-789");
        result.Key.Should().Be("fabrikam");
    }

    [Fact]
    public async Task getByIdAsync_ShouldReturnInactiveTenant()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var tenant = Tenant.Create("tenant-inactive", "inactive", "Inactive Corporation");
        tenant.Deactivate();
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var store = new TenantStore(context);

        // Act
        var result = await store.GetByIdAsync("tenant-inactive");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("tenant-inactive");
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldImplementITenantStore()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var store = new TenantStore(context);

        // Act
        store.Should().BeAssignableTo<ITenantStore>();
    }
}
