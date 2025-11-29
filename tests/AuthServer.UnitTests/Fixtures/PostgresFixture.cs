using AuthServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MultiTenancy.Abstractions;
using Multitenancy.Models;
using MultiTenancy.Models;
using Testcontainers.PostgreSql;

namespace AuthServer.UnitTests.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    public string ConnectionString { get; set; } = default;
    public ITenantAccessor TenantAccessor { get; set; } = new TenantAccessor();
    private PostgreSqlContainer _container = default!;


    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _container.StartAsync();

        ConnectionString = _container.GetConnectionString();

        // Run migrations or ensureCreated on your test DB
        var options = new DbContextOptionsBuilder<AuthServerDbContext>()
            .UseNpgsql(ConnectionString)
            .EnableServiceProviderCaching(false)  // Disable model caching for tests
            .Options;

        using var context = new AuthServerDbContext(options, TenantAccessor);
        context.Database.EnsureCreated();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
