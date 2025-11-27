using Microsoft.AspNetCore.Builder;
using MultiTenancy.Middleware;

namespace MultiTenancy.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantResolutionMiddleware>();
    }
}
