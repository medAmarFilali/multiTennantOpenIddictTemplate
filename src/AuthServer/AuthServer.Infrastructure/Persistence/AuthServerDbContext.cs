using AuthServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MultiTenancy.Abstractions;
using MultiTenancy.Entities;

namespace AuthServer.Infrastructure.Persistence;

/// <summary>
/// Database context the AuthServer with built-in multi-tenancy support.
/// Automatically filters queries by tenant and sets TenantId on entities.
/// </summary>
public class AuthServerDbContext : DbContext
{
    private readonly ITenantAccessor? _tenantAccessor;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Constructor for use with dependency injection in the application.
    /// </summary>
    public AuthServerDbContext(
        DbContextOptions<AuthServerDbContext> options,
        ITenantAccessor? tenantAccessor)
        : base(options)
    {
        _tenantAccessor = tenantAccessor;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthServerDbContext).Assembly);

        // Apply global query filter for multi-tenancy
        // This automatically filters all queries for entities implementing ITenantEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AuthServerDbContext)
                    .GetMethod(nameof(SetTenantFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    /// <summary>
    /// Sets up global query filter for tenant isolation.
    /// Queries will autmatically filter by the current tenant.
    /// </summary>
    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ITenantEntity
    {
        // Capture a reference to avoid null reference issues in the expression
        var accessor = _tenantAccessor;

        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            accessor == null ||
            accessor.TenantContext == null ||
            string.IsNullOrEmpty(accessor.TenantContext.TenantId) ||
            e.TenantId == accessor.TenantContext.TenantId
        );
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set TenantId for new entities if not already set
        if (_tenantAccessor?.TenantContext?.TenantId != null)
        {
            var currentTenantId = _tenantAccessor.TenantContext.TenantId;
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                if (entry.State == EntityState.Added && string.IsNullOrEmpty(entry.Entity.TenantId))
                {
                    entry.Entity.TenantId = currentTenantId;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
