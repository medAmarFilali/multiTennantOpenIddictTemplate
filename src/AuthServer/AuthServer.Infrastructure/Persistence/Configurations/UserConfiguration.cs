using AuthServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the User entity.
/// This is a partial class - developers can extend it in separate files.
/// </summary>
/// <remarks>
/// To extend this configuration for custom User properties:
/// 1. Create a partial class file (e.g., UserConfiguration.Custom.cs)
/// 2. Override the configureCustomProperties method
///
/// Example:
/// <code>
/// public partial class UserConfiguration
/// {
///     protected override void ConfigureCustomProperties(EntityTypeBuilder<User> builder)
///     {
///         // Configure custom properties
///         builder.Property(u => u.FirstName).HasMaxLength(100);
///         builder.Property(u => u.LastName).HasMaxLength(100);
///         builder.Property(u => u.PhoneNumber).HasMaxLength(20);
///
///         // Add custom indexes
///         builder.HasIndex(u => u.PhoneNumber)
///             .HasDatabaseName("IX_Users_PhoneNumber");
///     }
/// }
/// </code>
/// </remarks>
///
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        ConfigureCoreProperties(builder);
        ConfigureCustomProperties(builder);
    }

    /// <summary>
    /// Configures the core User properties that come with the template.
    /// Vitual method - can be overridden if you need to customize core configuration.
    /// </summary>
    protected virtual void ConfigureCoreProperties(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        // Primary key
        builder.HasKey(u => u.Id);

        // Properties
        builder.Property(u => u.Id)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.TenantId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.UserName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);

        // Indexes
        builder.HasIndex(u => u.TenantId)
            .HasDatabaseName("IX_Users_TenantId");

        builder.HasIndex(u => new { u.TenantId, u.UserName })
            .IsUnique()
            .HasDatabaseName("IX_Users_TenantId_UserName");

        builder.HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique()
            .HasDatabaseName("IX_Users_TenantId_Email");

        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_Users_IsActive");
    }

    /// <summary>
    /// Hook method for configuring custom User properties.
    /// Override this in a partial class to configure properties you've added to the User entity.
    /// </summary>
    /// <example>
    /// <code>
    /// protected override void ConfigureCustomProperties(EntityTypeBuilder<User> builder)
    /// {
    ///     builder.Property(u => u.FirstName).HasMaxLength(100);
    ///     builder.Property(u => u.LastName).HasMaxLength(100);
    /// }
    /// </code>
    /// </example>
    protected virtual void ConfigureCustomProperties(EntityTypeBuilder<User> builder)
    {
        // Intentionally empty - for extensions in partial classes
    }
}
