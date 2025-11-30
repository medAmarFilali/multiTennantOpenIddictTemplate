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

    /// <summary>
    /// Cleans all data from test tables while preserving schema.
    /// Call this at the start of each test to ensure clean state.
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<AuthServerDbContext>()
            .UseNpgsql(ConnectionString)
            .EnableServiceProviderCaching(false)
            .Options;

        // Use accessor with null context to bypass tenant filtering
        var noTenantAccessor = new TenantAccessor { TenantContext = null };
        using var context = new AuthServerDbContext(options, noTenantAccessor);

        // Use reflection to find all DBSet properties and clear them
        var dbSetProperties = context.GetType()
            .GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        foreach (var property in dbSetProperties)
        {
            var dbSet = property.GetValue(context);
            if (dbSet == null) continue;

            // Get the entity type
            var entityType = property.PropertyType.GetGenericArguments()[0];

            // Call Set<T>().IgnoreQueryFilters() dynamically
            var setMethod = context.GetType().GetMethod(nameof(context.Set), Type.EmptyTypes)!
                .MakeGenericMethod(entityType);
            var set = setMethod.Invoke(context, null);

            // Call IgnoreFilters()
            var ignoreFiltersMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethod(nameof(EntityFrameworkQueryableExtensions.IgnoreQueryFilters))!
                .MakeGenericMethod(entityType);
            var filteredSet = ignoreFiltersMethod.Invoke(context, new[] { set });

            // Call RemoveRange()
            var removeRangeMethod = context.GetType()
                .GetMethod(nameof(context.RemoveRange), new[] { typeof(IEnumerable<object>) });
            removeRangeMethod.Invoke(context, new[] { filteredSet });
        }

        await context.SaveChangesAsync();
    }

    public Task DisposeAsync() => _container?.DisposeAsync().AsTask() ?? Task.CompletedTask;
}
