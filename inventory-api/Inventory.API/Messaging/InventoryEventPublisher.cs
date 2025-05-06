using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Inventory.API.Messaging;

public class InventoryEventPublisher(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly string _hostName = configuration["RabbitMQ:Host"]!;
    private readonly int _port = int.Parse(configuration["RabbitMQ:Port"]!);
    private readonly string _username = configuration["RabbitMQ:Username"]!;
    private readonly string _password = configuration["RabbitMQ:Password"]!;
    private readonly string _exchange = configuration["RabbitMQ:Exchange"]!;

    public async Task Publish(object data, string routingKey)
    {
        Console.WriteLine($"Connecting to RabbitMQ at {_hostName}:{_port}...");

        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            Port = _port,
            UserName = _username,
            Password = _password
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();
      
        await channel.ExchangeDeclareAsync(exchange: _exchange, type: ExchangeType.Topic, durable: true);

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
}
