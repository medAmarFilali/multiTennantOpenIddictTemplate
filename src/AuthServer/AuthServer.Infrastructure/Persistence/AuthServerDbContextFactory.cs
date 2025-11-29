using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthServer.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating AuthServerDbContext instances during migrations.
/// This is only used by EF Core tools (dotnet ef migrations).
/// </summary>
public class AuthServerDbContextFactory : IDesignTimeDbContextFactory<AuthServerDbContext>
{
    public AuthServerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthServerDbContext>();

        // Use PostgreSQL with a connection string for migrations
        // This connection string is only used for generating migrations, not a runtime
        optionsBuilder.UseNpgsql("Host=localhost;Database=authserver;Username=postgres;Password=postgres",
            b => b.MigrationsAssembly(typeof(AuthServerDbContext).Assembly.FullName));

        return new AuthServerDbContext(optionsBuilder.Options, null!);
    }
}
