using Microsoft.AspNetCore.Http;
using MultiTenancy.Abstractions;
using MultiTenancy.Models;

namespace MultiTenancy.Extensions;

public class RouteTenantResolver : ITenantResolver
{
    private readonly ITenantStore? _tenantStore;
    private readonly string _tenantPrefix = "t";

    public RouteTenantResolver(ITenantStore? tenantStore = null)
    {
        _tenantStore = tenantStore;
    }

    public async Task<TenantContext?> ResolveAsync(HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value;
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        // Check for /{tenantPrefix}/{tenantKey} pattern
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2 || !segments[0].Equals(_tenantPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var tenantKey = segments[1];
        if (string.IsNullOrWhiteSpace(tenantKey))
        {
            return null;
        }

        // If tenant store is available, resolve tenant ID from key
        if (_tenantStore != null)
        {
            var tenant = await _tenantStore.GetByKeyAsync(tenantKey);
            if (tenant != null)
            {
                return new TenantContext
                {
                    TenantId = tenant.Id,
                    TenantKey = tenantKey
                };
            }
        }

        // Otherwise, use the key as both ID and key
        return new TenantContext
        {
            TenantId = tenantKey,
            TenantKey = tenantKey
        };
    }

}
