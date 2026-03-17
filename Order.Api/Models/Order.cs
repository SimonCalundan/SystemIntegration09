namespace Order.Api.Models;

public class Order
{
    public Guid Id { get; set; }
    public required string CustomerName { get; set; }
    public DateTime OrderDate { get; set; }
    public int TotalAmount { get; set; }

    public Order(string customerName, DateTime orderDate, int totalAmount)
    {
        Id = Guid.NewGuid();
        CustomerName = customerName;
        OrderDate = orderDate;
        TotalAmount = totalAmount;
    }

    private Order()
    {
    }
}