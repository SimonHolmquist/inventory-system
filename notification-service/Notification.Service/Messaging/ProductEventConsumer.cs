using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Notification.Service.Data;
using Notification.Service.Models;
using Microsoft.Extensions.DependencyInjection;
using Notification.Service.Resilience;
using System.Threading;

namespace Notification.Service.Messaging
{
    public class ProductEventConsumer(
        IServiceProvider serviceProvider,
        ILogger<ProductEventConsumer> logger,
        IConfiguration configuration) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<ProductEventConsumer> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly string _hostname = configuration["RabbitMQ:Host"] ?? "rabbitmq";
        private readonly string _exchange = configuration["RabbitMQ:Exchange"] ?? "inventory_exchange";
        private readonly SimpleCircuitBreaker _circuitBreaker = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var factory = new ConnectionFactory
            {
                HostName = _hostname,
            };

            _logger.LogInformation("ProductEventConsumer is running and listening for messages...");
            _logger.LogInformation("ENV: {env}", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
            _logger.LogInformation("RabbitMQ Host: {host}", _hostname);
            _logger.LogInformation("Connection string: {cs}", _configuration.GetConnectionString("DefaultConnection") ?? "NULL");

            using var connection = await TryConnectWithRetryAsync(factory, cancellationToken: stoppingToken);
            using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
            await channel.ExchangeDeclareAsync(exchange: _exchange, type: ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

            await DeclareQueueAndBindAsync(channel, "product.created.queue", "product.created");
            await DeclareQueueAndBindAsync(channel, "product.updated.queue", "product.updated");
            await DeclareQueueAndBindAsync(channel, "product.deleted.queue", "product.deleted");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {

                if (!_circuitBreaker.CanExecute())
                {
                    _logger.LogWarning("Circuit breaker is open. Skipping message.");
                    return;
                }

                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    _logger.LogInformation("Received event: {rk} -> {msg}", ea.RoutingKey, body);

                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

                    db.ProductEvents.Add(new ProductEvent
                    {
                        EventType = ea.RoutingKey,
                        Payload = body,
                        ReceivedAt = DateTime.UtcNow
                    });

                    await db.SaveChangesAsync();
                    _circuitBreaker.RecordSuccess();
                    _logger.LogInformation("Event stored.");
                }
                catch (Exception ex)
                {
                    _circuitBreaker.RecordFailure();
                    _logger.LogError(ex, "Error while processing message.");
                }
            };

            await channel.BasicConsumeAsync(queue: "product.created.queue", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            await channel.BasicConsumeAsync(queue: "product.updated.queue", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            await channel.BasicConsumeAsync(queue: "product.deleted.queue", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task DeclareQueueAndBindAsync(IChannel channel, string queueName, string routingKey)
        {
            await channel.QueueDeclareAsync(queue: queueName,
                                            durable: true,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

            await channel.QueueBindAsync(queue: queueName,
                                         exchange: _exchange,
                                         routingKey: routingKey);
        }

        private async Task<IConnection> TryConnectWithRetryAsync(ConnectionFactory factory, CancellationToken cancellationToken, int maxRetries = 5, int delaySeconds = 5)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    Console.WriteLine($"[RabbitMQ] Attempt {i + 1} to connect...");
                    return await factory.CreateConnectionAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RabbitMQ] Connection failed: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }

            throw new Exception("Failed to connect to RabbitMQ after several attempts.");
        }

    }
}
