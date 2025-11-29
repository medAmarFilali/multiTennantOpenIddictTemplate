using AuthServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Tenant entity.
/// This is a partial class - developers can extend it in separate files.
/// </summary>
/// <remarks>
/// To extend this configuration for custom Tenant properties:
/// 1. Create a partial class file (e.g., TenantConfiguration.Custom.cs)
/// 2. Override the ConfigureCustomProperties method
///
/// Example:
/// <code>
/// public partial class TenantConfiguration
/// {
///     protected override void ConfigureCustomProperties(EntityTypeBuilder<Tenant> builder)
///     {
///         // Configure custom properties
///         builder.Property(t => t.CompanySize).HasMaxLength(100);
///         builder.Property(t => t.Industry).HasMaxLength(100);
///         builder.Property(t => t.SubscriptionTien).HasMaxLength(50);
///
///         // Add custom indexes
///         builder.HasIndex(t => t.SubscriptionTier)
///             .hasDatabaseName("IX_Tenants_SubscriptionTier");
///     }
/// }
/// </code>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        ConfigureCoreProperties(builder);

    }

    /// <summary>
    /// Configures the core Tenant properties that come with the template.
    /// Virtual method = can be overridden if you need to customize core configuration.
    /// </summary>
    protected virtual void ConfigureCoreProperties(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        // Primary key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Id)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.Key)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt);

        // Indexes
        builder.HasIndex(t => t.Key)
            .IsUnique()
            .HasDatabaseName("IX_Tenants_Key");

        builder.HasIndex(t => t.IsActive)
            .HasDatabaseName("IX_Tenants_IsActive");
    }

    /// <summary>
    /// Hook method for configuring custom Tenant properties.
    /// Override this in a partial class to configure properties you've added to the Tenant entity.
    /// </summary>
    /// <example>
    /// <code>
    /// protected override void ConfigureCustomProperties(EntityTypeBuilder<Tenant> builder)
    /// {
    ///     builder.Property(t => t.CompanySize).HasMaxLength(100);
    ///     builder.Property(t => t.Industry).HasMaxLength(100);
    ///     builder.Property(t => t.SubscriptionTier).HasMaxLength(50);
    /// }
    /// </code>
    protected virtual void ConfigureCustomProperties(EntityTypeBuilder<Tenant> builder)
    {
        // Intentionally empty - for extensions in partial classes
    }

}
