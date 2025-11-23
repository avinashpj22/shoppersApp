using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using MediatR;
using ProductService.Application.Commands;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Messaging;

namespace ProductService.API;

/// <summary>
/// Startup configuration for Product Service.
/// Demonstrates Clean Architecture dependency injection setup.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services in the DI container.
    /// </summary>
    public static IServiceCollection AddProductServiceDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ====== Infrastructure - Database ======
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ProductDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ProductDbContext).Assembly.FullName);
                sqlOptions.CommandTimeout(30);
            });
        });

        // ====== Infrastructure - Messaging ======
        var serviceBusConnectionString = configuration.GetConnectionString("ServiceBusConnection")
            ?? throw new InvalidOperationException("Connection string 'ServiceBusConnection' not found.");

        var serviceBusClient = new Azure.Messaging.ServiceBus.ServiceBusClient(
            serviceBusConnectionString,
            new Azure.Messaging.ServiceBus.ServiceBusClientOptions
            {
                TransportType = Azure.Messaging.ServiceBus.ServiceBusTransportType.AmqpWebSockets
            });

        services.AddSingleton(serviceBusClient);
        services.AddScoped<IEventPublisher, AzureServiceBusEventPublisher>();
        services.AddScoped<AzureServiceBusEventSubscriber>();

        // ====== Application - Repositories ======
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductQueryRepository, ProductQueryRepository>();

        // ====== Application - MediatR ======
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly);
            // Add pipeline behaviors for cross-cutting concerns
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        return services;
    }
}

/// <summary>
/// Example MediatR pipeline behavior for validation.
/// Demonstrates cross-cutting concerns in CQRS.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validating request: {RequestType}", typeof(TRequest).Name);

        // Add custom validation logic here
        // Example: Check if request properties are valid

        return await next();
    }
}

/// <summary>
/// Example MediatR pipeline behavior for logging.
/// Demonstrates cross-cutting concerns in CQRS.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}", requestName);

        var startTime = DateTime.UtcNow;

        try
        {
            var response = await next();
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Handled {RequestName} successfully in {Duration}ms",
                requestName, duration.TotalMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(
                ex,
                "Error handling {RequestName} after {Duration}ms",
                requestName, duration.TotalMilliseconds);

            throw;
        }
    }
}
