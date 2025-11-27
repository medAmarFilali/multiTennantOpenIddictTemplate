using Microsoft.Extensions.DependencyInjection;
using MultiTenancy.Abstractions;
using Multitenancy.Models;
using MultiTenancy.Resolvers;

namespace MultiTenancy.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMultiTenancy(
        this IServiceCollection services,
        Action<MultiTenancyOptions>? configure = null)
    {
        var options = new MultiTenancyOptions();
        configure?.Invoke(options);

        services.AddSingleton<ITenantAccessor, TenantAccessor>();

        if (options.UseHeaderResolver)
        {
            services.AddSingleton<HeaderTenantResolver>();
        }

        if (options.UseRouteResolver)
        {
            services.AddSingleton<RouteTenantResolver>();
        }

        services.AddSingleton<ITenantResolver>(sp =>
        {
            var resolvers = new List<ITenantResolver>();

            if (options.UseHeaderResolver)
            {
                resolvers.Add(sp.GetRequiredService<HeaderTenantResolver>());
            }

            if (options.UseRouteResolver)
            {
                resolvers.Add(sp.GetRequiredService<RouteTenantResolver>());
            }

            return new CompositeTenantResolver(resolvers);
        });

        return services;
    }
}

public class MultiTenancyOptions
{
    public bool UseHeaderResolver { get; set; } = true;
    public bool UseRouteResolver { get; set; } = true;
}
