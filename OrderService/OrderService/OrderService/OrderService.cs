using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderService.Context;
using OrderService.Dmain;
using OrderService.IMessageBus;
using RabbitMQ.Client.Events;
using System.Text;

namespace OrderService.OrderService;


public interface IOrderService
{
    Task<CreateOrderModelRes> CreateOrder(CreateOrderModelReq model);
    Task HandleValidationOrder();
}

public class OrderService : IOrderService
{

    private readonly OrderDbContext _context;
    private readonly IMessageBus.IMessageBus _messageBus;
    public OrderService(OrderDbContext context, IMessageBus.IMessageBus messageBus)
    {
        _context = context;
        _messageBus = messageBus;
    }

    public async Task<CreateOrderModelRes> CreateOrder(CreateOrderModelReq model)
    {
        var order = new Order(model.CustomerId, model.Price, model.Count);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return new CreateOrderModelRes()
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            OrderTotal = order.OrderTotal,
        };
    }

    public async Task HandleValidationOrder()
    {
        var connection = _messageBus.GetConnection("localhost", "guest", "guest");
        var channel = connection.CreateModel();
        channel.QueueDeclare("validateOrder", true, false, false, null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (sender, args) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonConvert.DeserializeObject<ValidateOrderSubmitResponseMessage>(body);
                var result = await UpdateOrder(message);
                if (result)
                {
                    channel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
                }
                else
                {
                    channel.BasicNack(args.DeliveryTag, false, true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        };

        channel.BasicConsume(
            queue: "validateOrder",
            autoAck: false,
            consumerTag: string.Empty,
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: consumer);
    }

    private async Task<bool> UpdateOrder(ValidateOrderSubmitResponseMessage model)
    {
        var order = await _context.Orders.Where(c => c.Id == model.OrderId && c.CustomerId == model.CustomerId)
            .SingleOrDefaultAsync();

        if (model.IsSuccess)
            order.DoneOrder();
        else
            order.CancelOrder();

        await _context.SaveChangesAsync();

        return true;
    }
}