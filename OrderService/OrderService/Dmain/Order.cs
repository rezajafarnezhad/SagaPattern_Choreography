using System.ComponentModel.DataAnnotations;

namespace OrderService.Dmain;

public class Order
{
    [Key]
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal OrderTotal { get; private set; }
    public decimal Price { get; private set; }
    public int Count { get; private set; }
    public OrderStatus OrderStatus { get; private set; }
    public DateTime CreationDateTime { get; private set; }

    public Order(Guid customerId, decimal price, int count)
    {
        CustomerId = customerId;
        Price = price;
        Count = count;
        OrderTotal = price * count;
        OrderStatus = OrderStatus.pending;
        CreationDateTime = DateTime.Now;
    }


    public void DoneOrder()
    {
        OrderStatus = OrderStatus.Done;
    }

    public void CancelOrder()
    {
        OrderStatus = OrderStatus.Cancel;
    }
}

public enum OrderStatus
{
    pending = 0,
    Done = 1,
    Cancel = 2,
}