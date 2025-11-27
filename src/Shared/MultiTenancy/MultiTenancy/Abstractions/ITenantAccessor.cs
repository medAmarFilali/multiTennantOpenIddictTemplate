using MultiTenancy.Models;

namespace MultiTenancy.Abstractions;

public interface ITenantAccessor
{
    TenantContext? TenantContext { get; }
    string? TenantId => TenantContext?.TenantId;
}
