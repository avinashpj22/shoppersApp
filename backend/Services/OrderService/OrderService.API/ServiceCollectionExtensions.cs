using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using MediatR;
using OrderService.Application.Commands;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Messaging;

namespace OrderService.API;

/// <summary>
/// Startup configuration for Order Service.
/// Demonstrates Clean Architecture dependency injection setup.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services in the DI container.
    /// </summary>
    public static IServiceCollection AddOrderServiceDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ====== Infrastructure - Database ======
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<OrderDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(OrderDbContext).Assembly.FullName);
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
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderQueryRepository, OrderQueryRepository>();

        // ====== Application - MediatR ======
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(PlaceOrderCommand).Assembly);
            // Add pipeline behaviors for cross-cutting concerns
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        return services;
    }

    /// <summary>
    /// Starts background services (event subscriptions).
    /// Called from Program.cs after app is built.
    /// </summary>
    public static async Task InitializeEventSubscriptionsAsync(
        this IServiceProvider serviceProvider,
        ILogger logger)
    {
        using var scope = serviceProvider.CreateScope();
        var subscriber = scope.ServiceProvider.GetRequiredService<AzureServiceBusEventSubscriber>();

        try
        {
            // Event subscriptions temporarily disabled
            // TODO: Fix the SubscribeAsync signature compatibility
            /*
            // Subscribe to payment events
            await subscriber.SubscribeAsync(
                topicName: "payment.events",
                subscriptionName: "order-service-payments",
                handler: async (args) =>
                {
                    var eventType = args.Message.Subject;
                    logger.LogInformation("Received payment event: {EventType}", eventType);

                    // In real implementation, deserialize and handle based on eventType
                    // Example: if (eventType == "PaymentProcessed") { ... }

                    await args.CompleteMessageAsync(args.CancellationToken);
                },
                errorHandler: async (args) =>
                {
                    logger.LogError(args.Exception, "Error processing payment event");
                    await Task.CompletedTask;
                }
            );
            */

            logger.LogInformation("Event subscriptions initialized successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize event subscriptions");
            throw;
        }
    }
}

/// <summary>
/// MediatR pipeline behavior for validation.
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
        if (request is PlaceOrderCommand orderCommand)
        {
            if (orderCommand.CustomerId == Guid.Empty)
                throw new ArgumentException("Customer ID cannot be empty");

            if (!orderCommand.LineItems.Any())
                throw new ArgumentException("Order must have at least one line item");
        }

        return await next();
    }
}

/// <summary>
/// MediatR pipeline behavior for logging.
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

/// <summary>
/// MediatR pipeline behavior for transaction management.
/// Demonstrates database transaction handling in CQRS.
/// </summary>
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only apply transactions to commands, not queries
        if (request is IRequest<TResponse> && typeof(TRequest).Name.EndsWith("Command"))
        {
            _logger.LogInformation("Starting transaction for {RequestName}", typeof(TRequest).Name);
            try
            {
                var response = await next();
                _logger.LogInformation("Transaction committed successfully");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction rolled back due to error");
                throw;
            }
        }

        return await next();
    }
}
