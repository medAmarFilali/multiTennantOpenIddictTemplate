using MultiTenancy.Models;

namespace MultiTenancy.Abstractions;

public interface ITenantStore
{
    Task<Tenant?> GetByIdAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByKeyAsync(string tenantKey, CancellationToken cancellationToken = default);
}
