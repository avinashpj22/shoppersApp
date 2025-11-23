using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ProductService.Domain.Entities;
using ProductService.Application.Commands;

namespace ProductService.Infrastructure.Messaging;

/// <summary>
/// Event publisher implementation using Azure Service Bus.
/// Publishes domain events to topics for asynchronous processing by other services.
/// </summary>
public class AzureServiceBusEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<AzureServiceBusEventPublisher> _logger;
    private Dictionary<string, ServiceBusSender>? _senders;

    public AzureServiceBusEventPublisher(
        ServiceBusClient serviceBusClient,
        ILogger<AzureServiceBusEventPublisher> logger)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
        _senders = new Dictionary<string, ServiceBusSender>();
    }

    /// <summary>
    /// Publishes a domain event to Azure Service Bus.
    /// Each event type is published to a named topic.
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent).Name;
        var topicName = GetTopicName(eventType);

        try
        {
            var sender = await GetSenderAsync(topicName);
            var message = new ServiceBusMessage(JsonSerializer.Serialize(@event))
            {
                ContentType = "application/json",
                Subject = eventType,
                MessageId = Guid.NewGuid().ToString(),
                CorrelationId = @event.AggregateId.ToString(),
                ApplicationProperties =
                {
                    { "EventType", eventType },
                    { "AggregateId", @event.AggregateId.ToString() },
                    { "OccurredAt", @event.OccurredAt.ToString("O") }
                }
            };

            await sender.SendMessageAsync(message, cancellationToken);
            _logger.LogInformation("Event published: {EventType}, AggregateId: {AggregateId}", eventType, @event.AggregateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event: {EventType}", eventType);
            throw;
        }
    }

    /// <summary>
    /// Gets or creates a sender for the specified topic.
    /// </summary>
    private async Task<ServiceBusSender> GetSenderAsync(string topicName)
    {
        if (_senders!.TryGetValue(topicName, out var sender))
        {
            return sender;
        }

        var newSender = _serviceBusClient.CreateSender(topicName);
        _senders[topicName] = newSender;
        return newSender;
    }

    /// <summary>
    /// Maps event type names to topic names.
    /// Example: ProductCreatedEvent → products.events
    /// </summary>
    private static string GetTopicName(string eventType) =>
        eventType switch
        {
            nameof(ProductCreatedEvent) => "products.events",
            nameof(ProductUpdatedEvent) => "products.events",
            nameof(InventoryReservedEvent) => "inventory.events",
            nameof(InventoryReleasedEvent) => "inventory.events",
            nameof(ProductDeactivatedEvent) => "products.events",
            _ => "generic.events"
        };

    public async ValueTask DisposeAsync()
    {
        if (_senders != null)
        {
            foreach (var sender in _senders.Values)
            {
                await sender.CloseAsync();
            }
            _senders.Clear();
        }

        await _serviceBusClient.DisposeAsync();
    }
}

/// <summary>
/// Event handler subscriber for Azure Service Bus.
/// Listens to events and invokes registered handlers.
/// </summary>
public class AzureServiceBusEventSubscriber : IAsyncDisposable
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<AzureServiceBusEventSubscriber> _logger;
    private Dictionary<string, ServiceBusProcessor>? _processors;

    public AzureServiceBusEventSubscriber(
        ServiceBusClient serviceBusClient,
        ILogger<AzureServiceBusEventSubscriber> logger)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
        _processors = new Dictionary<string, ServiceBusProcessor>();
    }

    /// <summary>
    /// Subscribes to events from a topic/subscription.
    /// </summary>
    public async Task SubscribeAsync(
        string topicName,
        string subscriptionName,
        Func<ProcessMessageEventArgs, Task> handler,
        Func<ProcessErrorEventArgs, Task> errorHandler)
    {
        try
        {
            var processor = _serviceBusClient.CreateProcessor(topicName, subscriptionName);
            processor.ProcessMessageAsync += handler;
            processor.ProcessErrorAsync += errorHandler;

            await processor.StartProcessingAsync();
            _processors![GetProcessorKey(topicName, subscriptionName)] = processor;

            _logger.LogInformation("Subscribed to topic: {TopicName}, subscription: {SubscriptionName}", topicName, subscriptionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to topic: {TopicName}", topicName);
            throw;
        }
    }

    /// <summary>
    /// Stops listening to events.
    /// </summary>
    public async Task UnsubscribeAsync(string topicName, string subscriptionName)
    {
        var key = GetProcessorKey(topicName, subscriptionName);
        if (_processors!.TryGetValue(key, out var processor))
        {
            await processor.StopProcessingAsync();
            await processor.CloseAsync();
            _processors.Remove(key);
        }
    }

    private static string GetProcessorKey(string topicName, string subscriptionName) => $"{topicName}:{subscriptionName}";

    public async ValueTask DisposeAsync()
    {
        if (_processors != null)
        {
            foreach (var processor in _processors.Values)
            {
                await processor.StopProcessingAsync();
                await processor.CloseAsync();
            }
            _processors.Clear();
        }

        await _serviceBusClient.DisposeAsync();
    }
}

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
}
