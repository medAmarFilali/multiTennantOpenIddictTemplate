using MultiTenancy.Abstractions;
using MultiTenancy.Models;

namespace Multitenancy.Models;

public class TenantAccessor : ITenantAccessor
{
    private static readonly AsyncLocal<TenantContext?> _tenantContext = new();

    public TenantContext? TenantContext
    {
        get => _tenantContext.Value;
        set => _tenantContext.Value = value;
    }
}
