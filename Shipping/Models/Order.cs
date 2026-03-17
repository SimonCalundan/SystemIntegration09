namespace Shipping.Models;

public class Order
{
    public Guid Id { get; set; }
    public required string CustomerName { get; set; }
    public DateTime OrderDate { get; set; }
    public int TotalAmount { get; set; }
}
