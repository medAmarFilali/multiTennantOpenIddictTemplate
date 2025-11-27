using Microsoft.AspNetCore.Http;
using MultiTenancy.Abstractions;
using MultiTenancy.Models;

namespace MultiTenancy.Resolvers;

public class HeaderTenantResolver : ITenantResolver
{
    private const string TenantIdHeaderName = "X-Tenant-Id";

    public Task<TenantContext?> ResolveAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(TenantIdHeaderName, out var tenantIdValues))
        {
            return Task.FromResult<TenantContext?>(null);
        }

        var tenantId = tenantIdValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Task.FromResult<TenantContext?>(null);
        }

        var tenantContext = new TenantContext
        {
            TenantId = tenantId
        };

        return Task.FromResult<TenantContext?>(tenantContext);
    }
}
