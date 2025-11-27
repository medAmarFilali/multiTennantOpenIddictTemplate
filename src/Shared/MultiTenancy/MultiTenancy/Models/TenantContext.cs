namespace MultiTenancy.Models;

public class TenantContext
{
    public string? TenantId { get; set; }
    public string? TenantKey { get; set; }
    public bool IsResolved => !string.IsNullOrEmpty(TenantId);
}
