namespace MultiTenancy.Models;

public class Tenant
{
    public required string Id { get; init; }
    public required string Key { get; init; }
    public required string Name { get; init; }
    public bool IsActive { get; init; } = true;
}
