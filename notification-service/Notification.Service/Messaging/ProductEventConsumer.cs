using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Notification.Service.Data;
using Notification.Service.Models;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly string _hostname = configuration["RabbitMQ:Host"] ?? "localhost";
        private readonly string _exchange = "inventory_exchange";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var factory = new ConnectionFactory() { HostName = _hostname };

            _logger.LogInformation("ProductEventConsumer is running and listening for messages...");
            _logger.LogInformation("ENV: {env}", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
            _logger.LogInformation("RabbitMQ Host: {host}", _hostname);
            _logger.LogInformation("Connection string: {cs}", _configuration.GetConnectionString("DefaultConnection") ?? "NULL");

            using var connection = await factory.CreateConnectionAsync(stoppingToken);
            using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);


            await DeclareQueueAndBindAsync(channel, "product.created.queue", "product.created");
            await DeclareQueueAndBindAsync(channel, "product.updated.queue", "product.updated");
            await DeclareQueueAndBindAsync(channel, "product.deleted.queue", "product.deleted");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
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
                    _logger.LogInformation("Event stored.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing message.");
                }
            };

            await channel.BasicConsumeAsync(queue: "product.created.queue", autoAck: true, consumer: consumer, cancellationToken: stoppingToken);
            await channel.BasicConsumeAsync(queue: "product.updated.queue", autoAck: true, consumer: consumer, cancellationToken: stoppingToken);
            await channel.BasicConsumeAsync(queue: "product.deleted.queue", autoAck: true, consumer: consumer, cancellationToken: stoppingToken);

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
    }
}
