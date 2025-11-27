using Microsoft.AspNetCore.Http;
using MultiTenancy.Abstractions;
using MultiTenancy.Models;

namespace MultiTenancy.Resolvers;

public class CompositeTenantResolver : ITenantResolver
{
    private readonly IEnumerable<ITenantResolver> _resolvers;

    public CompositeTenantResolver(IEnumerable<ITenantResolver> resolvers)
    {
        _resolvers = resolvers;
    }

    public async Task<TenantContext?> ResolveAsync(HttpContext httpContext)
    {
        foreach (var resolver in _resolvers)
        {
            var tenantContext = await resolver.ResolveAsync(httpContext);
            if (tenantContext?.IsResolved == true)
            {
                return tenantContext;
            }
        }

        return null;
    }

}
