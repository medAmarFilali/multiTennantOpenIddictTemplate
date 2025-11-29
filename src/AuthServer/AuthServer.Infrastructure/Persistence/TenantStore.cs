using Microsoft.EntityFrameworkCore;
using MultiTenancy.Abstractions;
using MultiTenancy.Models;

namespace AuthServer.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core implementation of ITenantStore.
/// Provides tenant lookup by ID and key from the database.
/// </summary>
public class TenantStore : ITenantStore
{
    private readonly AuthServerDbContext _context;

    public TenantStore(AuthServerDbContext context)
    {
        _context = context;
    }


    public async Task<Tenant?> GetByIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant == null)
            return null;

        return new Tenant
        {
            Id = tenant.Id,
            Key = tenant.Key,
            Name = tenant.Name,
            IsActive = tenant.IsActive
        };
    }

    public async Task<Tenant?> GetByKeyAsync(string tenantKey, CancellationToken cancellationToken = default)
    {
        var normalizeKey = tenantKey.ToLowerInvariant();

        var tenant = await _context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Key == normalizeKey, cancellationToken);

        if (tenant == null)
            return null;

        return new Tenant
        {
            Id = tenant.Id,
            Key = tenant.Key,
            Name = tenant.Name,
            IsActive = tenant.IsActive
        };
    }
}
