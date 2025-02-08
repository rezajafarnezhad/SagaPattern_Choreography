namespace CustomerService.MessageBus;

public class OrderSubmitMessage : BaseMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal OrderTotal { get; set; }
}


public class ValidateOrderSubmitResponseMessage : BaseMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public bool IsSuccess { get; set; }
}