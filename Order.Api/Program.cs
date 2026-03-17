
using Microsoft.EntityFrameworkCore;
using Order.Api.Data;
using Scalar.AspNetCore;

namespace Order.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();
        builder.AddRabbitMQClient("messaging");
        // Add services to the container.
        
        builder.Services.AddDbContext<OrderDbContext>(options =>
            options.UseInMemoryDatabase("OrdersDb"));

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
