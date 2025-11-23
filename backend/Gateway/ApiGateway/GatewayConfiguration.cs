using Yarp.ReverseProxy.Configuration;

namespace ApiGateway.Configuration;

/// <summary>
/// YARP configuration for API Gateway.
/// Defines route mappings, cluster definitions, and routing policies.
/// </summary>
public static class GatewayConfiguration
{
    public static IServiceCollection AddGatewayConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));

        return services;
    }
}
