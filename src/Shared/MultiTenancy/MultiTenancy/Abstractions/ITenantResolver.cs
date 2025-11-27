using Microsoft.AspNetCore.Http;
using MultiTenancy.Models;

namespace MultiTenancy.Abstractions;

public interface ITenantResolver
{
    Task<TenantContext?> ResolveAsync(HttpContext context);
}
