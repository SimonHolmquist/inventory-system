using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Inventory.API.Messaging;

public class InventoryEventPublisher(IConfiguration config)
{
    private readonly string _hostname = config["RabbitMQ:Host"] ?? "localhost";
    private readonly string _exchange = "inventory_exchange";

    public async Task Publish(object data, string routingKey)
    {
        var factory = new ConnectionFactory() { HostName = _hostname };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange: _exchange, type: ExchangeType.Direct);

        await DeclareQueueAndBindAsync(channel, "product.created.queue", "product.created");
        await DeclareQueueAndBindAsync(channel, "product.updated.queue", "product.updated");
        await DeclareQueueAndBindAsync(channel, "product.deleted.queue", "product.deleted");

        var message = JsonSerializer.Serialize(data);
        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: _exchange,
            routingKey: routingKey,
            body: body
        );
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
