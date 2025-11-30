using System.Runtime.CompilerServices;
using AuthServer.Application.Common;
using AuthServer.Application.Users;
using AuthServer.Application.Users.DTOs;
using AuthServer.Domain.Entities;
using AuthServer.Infrastructure.Persistence;
using AuthServer.Infrastructure.Repositories;
using AuthServer.UnitTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MultiTenancy.Abstractions;
using MultiTenancy.Models;
using NSubstitute;
using Xunit;

namespace TenantApi.Application;

[Collection("Postgres collection")]
public class UserServiceTests
{
    private readonly PostgresFixture _fixture;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantAccessor _tenantAccessor;

    public UserServiceTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tenantAccessor = Substitute.For<ITenantAccessor>();

        // Setup default tenant context
        _tenantAccessor.TenantContext.Returns(new TenantContext
        {
            TenantId = "tenant-1",
            TenantKey = "acme"
        });

        // Setup password hasher to return predictable hashes
        _passwordHasher.HashPassword(Arg.Any<string>())
            .Returns(call => $"hashed_{call.Arg<string>()}");
    }

    private AuthServerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthServerDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .EnableServiceProviderCaching(false)
            .Options;

        return new AuthServerDbContext(options, _tenantAccessor);
    }

    private UserService CreateUserService(AuthServerDbContext context)
    {
        var repository = new UserRepository(context);
        return new UserService(repository, _passwordHasher, _tenantAccessor);
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldSucceed_WhenValidRequest()
    {
        // Arrange
        await _fixture.CleanDatabaseAsync();
        var context = CreateDbContext();
        var service = CreateUserService(context);

        var request = new RegisterUserRequest
        {
            Username = "johndoe",
            Email = "john@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var result = await service.RegisterUserAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Username.Should().Be("johndoe");
        result.Value.Email.Should().Be("john@example.com");
        result.Value.TenantId.Should().Be("tenant-1");
        result.Value.IsActive.Should().BeTrue();

        // Verify password was hashed
        _passwordHasher.Received(1).HashPassword("SecurePassword123!");
    }

    [Fact]
    public async Task RegisterUserAsync_ShouldFaile_WhenUsernameAlreadyExists()
    {
        // Arrange
        await _fixture.CleanDatabaseAsync();
        var context = CreateDbContext();
        var service = CreateUserService(context);

        // Create existing user
        var existingUser = User.Create("user-1", "tenant-1", "johndoe", "john@example.com", "hash");
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var request = new RegisterUserRequest
        {
            Username = "johndoe",
            Email = "different@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var result = await service.RegisterUserAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Username");
        result.Error.Should().Contain("already exists");

    }

    [Fact]
    public async Task RegisterUserAsync_ShouldFail_WhenEmailAlreadyExists()
    {
        // Arrange
        await _fixture.CleanDatabaseAsync();
        var context = CreateDbContext();
        var service = CreateUserService(context);

        // Create existing user
        var existingUser = User.Create("user-1", "tenant-1", "johndoe", "john@example.com", "hash");
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var request = new RegisterUserRequest
        {
            Username = "differentuser",
            Email = "john@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var result = await service.RegisterUserAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Email");
        result.Error.Should().Contain("already exists");

    }

    [Fact]
    public async Task RegisterUserAsync_ShouldAllowSameUsernameInDifferentTenant()
    {
        await _fixture.CleanDatabaseAsync();
        var context = CreateDbContext();

        // Create user in tenant-2
        var tenant2User = User.Create("user-2", "tenant-2", "johndoe", "john@tenant2.com", "hash");
        context.Users.Add(tenant2User);
        await context.SaveChangesAsync();

        // Current tenant is tenant-1 (from setup)
        var service = CreateUserService(context);

        var request = new RegisterUserRequest
        {
            Username = "johndoe", // Same username but different tenant
            Email = "john@tenant1.com",
            Password = "SecurePassword123!"
        };

        // Act
        var result = await service.RegisterUserAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue("same username is allowed in different tenants");
        result.Value!.Username.Should().Be("johndoe");
        result.Value.TenantId.Should().Be("tenant-1");

    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldSucceed_WhenUserExistsInCurrentTenant()
    {
        // Arrange
        await _fixture.CleanDatabaseAsync();
        var context = CreateDbContext();
        var service = CreateUserService(context);

        var user = User.Create("user-1", "tenant-1", "johndoe", "john@example.com", "hash");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUserByIdAsync("user-1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be("user-1");
        result.Value.Username.Should().Be("johndoe");
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldFail_WhenUserDoesNotExist()
    {
        // Arrange
        await _fixture.CleanDatabaseAsync();
        var context = CreateDbContext();
        var service = CreateUserService(context);

        // Act
        var result = await service.GetUserByIdAsync("non-existent", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldFail_WhenUserExistsInDifferentTenant()
    {
        // Arrange
        await _fixture.CleanDatabaseAsync();
        var context = CreateDbContext();
        var service = CreateUserService(context);

        // Create user in different tenant
        var user = User.Create("user-2", "tenant-2", "janedoe", "jane@example.com", "hash");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act - Current tenant is tenant-1
        var result = await service.GetUserByIdAsync("user-2",  CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue("user from different tenant should not be accessible");
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldSucceed_WhenUserExistsInCurrentTenant()
    {
        // Arrange
        await _fixture.CleanDatabaseAsync();
        var context = CreateDbContext();
        var service = CreateUserService(context);

        var user = User.Create("user-1", "tenant-1", "johndoe", "john@example.com", "hash");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetUserByUsernameAsync("johndoe", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Username.Should().Be("johndoe");

    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldFail_WhenUserDoesNotExist()
    {
        // Arrange
        await _fixture.CleanDatabaseAsync();
        var context = CreateDbContext();
        var service = CreateUserService(context);

        // Act
        var result = await service.GetUserByUsernameAsync("non-existent", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");

    }



}
