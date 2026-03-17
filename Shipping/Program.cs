using Microsoft.EntityFrameworkCore;
using Shipping.Data;

namespace Shipping;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.AddServiceDefaults();
        builder.AddRabbitMQClient("messaging");
        
        builder.Services.AddDbContext<ShippingDbContext>(options =>
            options.UseInMemoryDatabase("ShippingDb"));
            
        builder.Services.AddHostedService<Worker>();

        var host = builder.Build();
        host.Run();
    }
}
