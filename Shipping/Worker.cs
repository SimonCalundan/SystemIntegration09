using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shipping.Data;
using Shipping.Models;

namespace Shipping;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConnection _connection;
    private readonly IServiceProvider _serviceProvider;
    private IChannel? _channel;

    public Worker(ILogger<Worker> logger, IConnection connection, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _connection = connection;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: "orders",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("[SHIPPING] Order received: {Message}", message);

            try
            {
                var order = JsonSerializer.Deserialize<Order>(message);
                if (order != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ShippingDbContext>();
                    
                    dbContext.Orders.Add(order);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    
                    _logger.LogInformation("[SHIPPING] Order {OrderId} saved to Shipping database.", order.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SHIPPING] Error processing order message.");
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "orders",
            autoAck: true,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("[SHIPPING] Worker listening on queue: orders");

        // Keep the worker running until stoppingToken is canceled
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken: cancellationToken);
        }
        await base.StopAsync(cancellationToken);
    }
}
