using FastEndpoints;
using OrderService.IMessageBus;
using RabbitMQ.Client;

namespace OrderService.OrderService;



public class CreateOrderModelReq
{
    public Guid CustomerId { get; set; }
    public decimal Price { get; set; }
    public int Count { get; set; }
}


public class CreateOrderModelRes
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal OrderTotal { get; set; }
}


public class OrderEndPoint : Endpoint<CreateOrderModelReq, CreateOrderModelRes>
{
    private readonly IOrderService _orderService;
    private readonly IMessageBus.IMessageBus _messageBus;
    public OrderEndPoint(IMessageBus.IMessageBus messageBus, IOrderService orderService)
    {
        _messageBus = messageBus;
        _orderService = orderService;
    }

    public override void Configure()
    {
        Post("/api/Order/create");
        AllowAnonymous();
    }

    public override async Task<CreateOrderModelRes> HandleAsync(CreateOrderModelReq request, CancellationToken cancellationToken)
    {
        var result = await _orderService.CreateOrder(new CreateOrderModelReq()
        {
            CustomerId = Guid.Parse("83ff740d-91b3-4f2c-8399-3d8fca0089e7"),
            Count = request.Count,
            Price = request.Price,
        });


        PushMessage(result);
        return result;
    }

    private void PushMessage(CreateOrderModelRes order)
    {
        var connection = _messageBus.GetConnection("localhost", "guest", "guest");

        using var channel = connection.CreateModel();

        channel.ExchangeDeclare("OrderSubmit", ExchangeType.Topic, true, false, null);

        var message = new OrderSubmitMessage
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            OrderTotal = order.OrderTotal,
        };

        var body = _messageBus.ConvertToBodyMessage(message);
        var prop = channel.CreateBasicProperties();
        prop.Persistent = true;
        channel.BasicPublish("OrderSubmit", "Order.Submit", prop, body);

    }
}




