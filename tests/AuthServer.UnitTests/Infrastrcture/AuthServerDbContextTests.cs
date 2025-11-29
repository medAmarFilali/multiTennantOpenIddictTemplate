using AuthServer.Infrastructure.Persistence;
using AuthServer.UnitTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MultiTenancy.Abstractions;
using MultiTenancy.Models;
using DomainTenant = AuthServer.Domain.Entities.Tenant;
using DomainUser = AuthServer.Domain.Entities.User;

namespace AuthServer.UnitTests.Infrastrcture;

[Collection("Postgres collection")]
public class AuthServerDbContextTests
{
    private readonly PostgresFixture _fixture;

    public AuthServerDbContextTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    private class TestTenantAccessor : ITenantAccessor
    {
        public TenantContext? TenantContext { get; set; }
    }

    private AuthServerDbContext CreateContext(ITenantAccessor? tenantAccessor = null)
    {
        var options = new DbContextOptionsBuilder<AuthServerDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .EnableServiceProviderCaching(false)
            .Options;
        return new AuthServerDbContext(options, tenantAccessor ?? _fixture.TenantAccessor);
    }

    [Fact]
    public async Task Users_ShouldBeFilteredByTenant_WhenTenantContextIsSet()
    {
        // Arrange
        var tenantAccessor = new TestTenantAccessor
        {
            TenantContext = new TenantContext { TenantId = "tenant-1", TenantKey = "acme" }
        };

        await using var context = CreateContext(tenantAccessor);

        // Create users for different tenants
        var user1 = DomainUser.Create("user-1", "tenant-1", "john.doe", "john@acme.com", "hash1");
        var user2 = DomainUser.Create("user-2", "tenant-1", "jane.doe", "jane@acme.com", "hash2");
        var user3 = DomainUser.Create("user-3", "tenant-2", "bob.smith", "bob@contoso.com", "hash3");

        context.Users.AddRange(user1, user2, user3);
        await context.SaveChangesAsync();

        // Clear change tracker to simulate new query
        context.ChangeTracker.Clear();

        // Act
        var users = await context.Users.ToListAsync();

        // Assert
        users.Should().HaveCount(2, "only users from tenant-1 should be returned");
        users.Should().Contain(u => u.Id == "user-1");
        users.Should().Contain(u => u.Id == "user-2");
        users.Should().NotContain(u => u.Id == "user-3");
    }

    [Fact]
    public async Task Users_ShouldReturnAllUsers_WhenTenantContextIsNull()
    {
        // Arrange
        var context = CreateContext(null);

        // Create users for different tenants
        var user1 = DomainUser.Create("user-1", "tenant-1", "john.doe", "john@acme.com", "hash1");
        var user2 = DomainUser.Create("user-2", "tenant-2", "jane.doe", "jane@contoso.com", "hash2");

        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        // Clear change tracker to simulate a new query
        context.ChangeTracker.Clear();

        // Act
        var users = await context.Users.ToListAsync();

        // Assert
        users.Should().HaveCount(2, "all users should be returned when no tenant context");
    }

    [Fact]
    public async Task Users_ShouldReturnDifferentUsers_WhenTenantContextChanges()
    {
        // Arrange
        var setupContext = CreateContext();
        var user1 = DomainUser.Create("user-1", "tenant-1", "john.doe", "john@acme.com", "hash1");
        var user2 = DomainUser.Create("user-2", "tenant-2", "jane.doe", "jane@contoso.com", "hash2");

        setupContext.Users.AddRange(user1, user2);
        await setupContext.SaveChangesAsync();
        await setupContext.DisposeAsync();

        // Act - Query with tenant-1 using a new context
        var tenantAccessor1 = new TestTenantAccessor
        {
            TenantContext = new TenantContext { TenantId = "tenant-1", TenantKey = "acme" }
        };
        var context1 = CreateContext(tenantAccessor1);
        var tenant1Users = await context1.Users.ToListAsync();
        await context1.DisposeAsync();

        // Act - Query with tenant-2 using a new context
        var tenantAccessor2 = new TestTenantAccessor
        {
            TenantContext = new TenantContext { TenantId = "tenant-2", TenantKey = "contoso" }
        };
        var context2 = CreateContext(tenantAccessor2);
        var tenant2Users = await context2.Users.ToListAsync();
        await context2.DisposeAsync();

        // Act
        tenant1Users.Should().HaveCount(1);
        tenant1Users.First().Id.Should().Be("user-1");

        tenant2Users.Should().HaveCount(1);
        tenant2Users.First().Id.Should().Be("user-2");
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotOverrideTenantId_WhenTenantIdIsAlreadySet()
    {
        // Arrange
        var tenantAccessor = new TestTenantAccessor
        {
            TenantContext = new TenantContext { TenantId = "tenant-auto", TenantKey = "auto" }
        };

        var context = CreateContext(tenantAccessor);

        // Create user with explicit TenantId
        var user = DomainUser.Create("user-explicit", "tenant-explicit", "test.user", "test@example.com", "hash");

        // Act
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Assert
        user.TenantId.Should().Be("tenant-explicit", "SaveChangesAsync should not override explicitly set TenantId");
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotOverrideTenantId_WhenModifyingUser()
    {
        var tenantAccessor = new TestTenantAccessor
        {
            TenantContext = new TenantContext { TenantId = "tenant-1", TenantKey = "acme"}
        };

        var context = CreateContext(tenantAccessor);

        var user = DomainUser.Create("user-1", "tenant-1", "john.doe", "john@acme.com", "hash");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Change tenant context
        tenantAccessor.TenantContext = new TenantContext { TenantId = "tenant-2", TenantKey = "contoso" };

        // Act - Modify existing user
        user.UpdateEmail("newemail@acme.com");
        await context.SaveChangesAsync();

        // Assert
        user.TenantId.Should().Be("tenant-1", "Tenant should not change wqhen modifying existing entities");
    }

    [Fact]
    public async Task Tenants_ShouldNotBeFilteredByTenant()
    {
        // Arrange
        var tenantAccessor = new TestTenantAccessor
        {
            TenantContext = new TenantContext { TenantId = "tenant-1", TenantKey = "acme" }
        };

        var context = CreateContext(tenantAccessor);

        // Create tenants
        var tenant1 = DomainTenant.Create("tenant-1", "acme", "Acme Corporation");
        var tenant2 = DomainTenant.Create("tenant-2", "contoso", "Contoso Ltd");

        context.Tenants.AddRange(tenant1, tenant2);
        await context.SaveChangesAsync();

        // Clear change tracker
        context.ChangeTracker.Clear();

        // Act
        var tenants = await context.Tenants.ToListAsync();

        // Assert
        tenants.Should().HaveCount(2, "Tenant entity should not be filtered by tenant context");
    }

    [Fact]
    public async Task Users_CanBeQueriedIgnoringQueryFilters()
    {
        // Arrange
        var tenantAccessor = new TestTenantAccessor
        {
            TenantContext = new TenantContext { TenantId = "tenant-1", TenantKey = "acme" }
        };

        var context = CreateContext(tenantAccessor);

        // Create users for different tenants
        var user1 = DomainUser.Create("user-1", "tenant-1", "john.doe", "johm@acme.com", "hash1");
        var user2 = DomainUser.Create("user-2", "tenant-2", "jane.doe", "jane@contoso.com", "hash2");

        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        // Clear change tracker
        context.ChangeTracker.Clear();

        // Act
        var allUsers = await context.Users.IgnoreQueryFilters().ToListAsync();

        // Assert
        allUsers.Should().HaveCount(2, "IgnoreQueryFilters should return all users regardless of tenant context");
    }


}
