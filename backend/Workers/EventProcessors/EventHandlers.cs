using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace OrderService.EventProcessors;

/// <summary>
/// Azure Function to handle PaymentProcessed events.
/// Demonstrates serverless event processing for microservices.
/// 
/// This function:
/// 1. Receives PaymentProcessed events from Service Bus
/// 2. Calls Order Service to confirm the order
/// 3. Publishes OrderConfirmed event if successful
/// </summary>
public class PaymentProcessedEventHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PaymentProcessedEventHandler> _logger;

    public PaymentProcessedEventHandler(IHttpClientFactory httpClientFactory, ILogger<PaymentProcessedEventHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [Function("PaymentProcessedEventHandler")]
    public async Task Run(
        [ServiceBusTrigger("payment.events", "order-service-payments", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Received payment event: MessageId={MessageId}, Subject={Subject}", 
                message.MessageId, message.Subject);

            // Deserialize the event
            var eventData = JsonSerializer.Deserialize<PaymentProcessedEvent>(message.Body.ToString());
            
            if (eventData == null)
            {
                _logger.LogError("Failed to deserialize payment event");
                throw new InvalidOperationException("Invalid event data");
            }

            // Call Order Service to confirm the order
            var client = _httpClientFactory.CreateClient();
            var orderServiceUrl = Environment.GetEnvironmentVariable("ORDER_SERVICE_URL") 
                ?? throw new InvalidOperationException("ORDER_SERVICE_URL not configured");

            var confirmUrl = $"{orderServiceUrl}/api/v1/orders/{eventData.OrderId}/confirm";
            var response = await client.PutAsync(confirmUrl, null, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to confirm order {OrderId}: {StatusCode}", eventData.OrderId, response.StatusCode);
                throw new InvalidOperationException($"Failed to confirm order: {response.StatusCode}");
            }

            _logger.LogInformation("Order {OrderId} confirmed successfully after payment", eventData.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment event");
            throw; // Function will retry or move to dead-letter queue
        }
    }
}

/// <summary>
/// Azure Function to handle OrderCreated events.
/// Sends confirmation email to customer.
/// </summary>
public class OrderCreatedEmailNotifier
{
    private readonly ILogger<OrderCreatedEmailNotifier> _logger;

    public OrderCreatedEmailNotifier(ILogger<OrderCreatedEmailNotifier> logger)
    {
        _logger = logger;
    }

    [Function("OrderCreatedEmailNotifier")]
    public async Task Run(
        [ServiceBusTrigger("orders.events", "email-service-orders", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            if (message.Subject != "OrderCreatedEvent")
                return; // Not interested in other events

            _logger.LogInformation("Processing order created event: MessageId={MessageId}", message.MessageId);

            var eventData = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Body.ToString());
            
            if (eventData == null)
            {
                _logger.LogError("Failed to deserialize order created event");
                throw new InvalidOperationException("Invalid event data");
            }

            // In real implementation, send email via SendGrid, Azure Communication Services, etc.
            _logger.LogInformation(
                "Email notification sent for OrderId={OrderId}, CustomerId={CustomerId}, Total={TotalAmount}",
                eventData.OrderId, eventData.CustomerId, eventData.TotalAmount);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order created event");
            throw;
        }
    }
}

/// <summary>
/// Azure Function to handle OrderShipped events.
/// Sends shipment notification email with tracking number.
/// </summary>
public class OrderShippedNotifier
{
    private readonly ILogger<OrderShippedNotifier> _logger;

    public OrderShippedNotifier(ILogger<OrderShippedNotifier> logger)
    {
        _logger = logger;
    }

    [Function("OrderShippedNotifier")]
    public async Task Run(
        [ServiceBusTrigger("orders.events", "email-service-shipments", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            if (message.Subject != "OrderShippedEvent")
                return;

            _logger.LogInformation("Processing order shipped event: MessageId={MessageId}", message.MessageId);

            var eventData = JsonSerializer.Deserialize<OrderShippedEvent>(message.Body.ToString());
            
            if (eventData == null)
            {
                _logger.LogError("Failed to deserialize order shipped event");
                throw new InvalidOperationException("Invalid event data");
            }

            // Send shipping notification email
            _logger.LogInformation(
                "Shipment notification sent for OrderId={OrderId}, Tracking={TrackingNumber}",
                eventData.OrderId, eventData.TrackingNumber);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order shipped event");
            throw;
        }
    }
}

/// <summary>
/// Azure Function to log analytics/metrics for orders.
/// Could feed into Application Insights or data warehouse.
/// </summary>
public class OrderAnalyticsLogger
{
    private readonly ILogger<OrderAnalyticsLogger> _logger;

    public OrderAnalyticsLogger(ILogger<OrderAnalyticsLogger> logger)
    {
        _logger = logger;
    }

    [Function("OrderAnalyticsLogger")]
    public async Task Run(
        [ServiceBusTrigger("orders.events", "analytics-service", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Logging analytics for event: Subject={Subject}", message.Subject);

            // Track different event types
            var eventType = message.Subject;
            
            switch (eventType)
            {
                case "OrderCreatedEvent":
                    var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Body.ToString());
                    _logger.LogInformation("Analytics: Order Created - Total={TotalAmount}, Items={ItemCount}",
                        orderCreated?.TotalAmount, orderCreated?.LineItemCount);
                    break;

                case "OrderCompletedEvent":
                    _logger.LogInformation("Analytics: Order Completed");
                    break;

                case "OrderCanceledEvent":
                    _logger.LogInformation("Analytics: Order Canceled");
                    break;
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging analytics");
            throw;
        }
    }
}

// Event DTOs for deserialization
public record PaymentProcessedEvent(
    Guid OrderId,
    decimal Amount,
    DateTime ProcessedAt
);

public record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    int LineItemCount,
    DateTime OccurredAt
);

public record OrderShippedEvent(
    Guid OrderId,
    string TrackingNumber,
    DateTime OccurredAt
);
