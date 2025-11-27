using Microsoft.AspNetCore.Http;
using MultiTenancy.Abstractions;
using Multitenancy.Models;

namespace MultiTenancy.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        ITenantResolver TenantResolver,
        ITenantAccessor tenantAccessor
    )
    {
        var tenantContext = await TenantResolver.ResolveAsync(context);
        if (tenantAccessor is TenantAccessor accessor)
        {
            accessor.TenantContext = tenantContext;
        }

        await _next(context);
    }
}
