using Microsoft.EntityFrameworkCore;
using Shipping.Models;

namespace Shipping.Data;

public class ShippingDbContext : DbContext
{
    public ShippingDbContext(DbContextOptions<ShippingDbContext> options) : base(options)
    {
    }
    public DbSet<Order> Orders => Set<Order>();
}
