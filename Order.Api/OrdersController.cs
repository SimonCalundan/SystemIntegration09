using Microsoft.AspNetCore.Mvc;
using Order.Api.Data;
using Order.Api.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Order.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController(OrderDbContext dbContext, IConnection rabbitConnection, ILogger<OrdersController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(Models.Order order)
    {
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        await using var channel = await rabbitConnection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "orders",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var message = JsonSerializer.Serialize(order);
        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "orders",
            body: body);

        logger.LogInformation("[ORDER.API] Order {OrderId} created and published to RabbitMQ.", order.Id);
        return Created($"/orders/{order.Id}", order);
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        logger.LogInformation("[ORDER.API] Getting all orders.");
        return Ok(dbContext.Orders.ToList());
    }
}
