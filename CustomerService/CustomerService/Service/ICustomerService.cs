using CustomerService.Context;
using CustomerService.MessageBus;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Data;
using System.Text;

namespace CustomerService.Service;

public interface ICustomerService
{
    Task HandleSubmitOrder();
}

public class CustomerService : ICustomerService
{
    private readonly CustomerDbContext _context;
    private readonly IMessageBus _messageBus;
    public CustomerService(CustomerDbContext context, IMessageBus messageBus)
    {
        _context = context;
        _messageBus = messageBus;
    }

    public async Task HandleSubmitOrder()
    {
        var connection = _messageBus.GetConnection("localhost", "guest", "guest");
        var channel = connection.CreateModel();

        channel.ExchangeDeclare("OrderSubmit", ExchangeType.Topic, true, false, null);
        channel.QueueDeclare("SubmitOrderCustomer", true, false, false, null);
        channel.QueueBind("SubmitOrderCustomer", "OrderSubmit", "Order.Submit");


        var consumer = new EventingBasicConsumer(channel);


        consumer.Received += async (sender, args) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(args.Body.ToArray());
                var message = JsonConvert.DeserializeObject<OrderSubmitMessage>(body);
                var result = await CheckOrder(message, channel);
                if (result)
                {
                    channel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
                }
                else
                {
                    channel.BasicNack(deliveryTag: args.DeliveryTag, multiple: false, true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"log : {e}");
            }
        };

        channel.BasicConsume(
            queue: "SubmitOrderCustomer",
            autoAck: false,
            consumerTag: string.Empty,
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: consumer
        );
    }

    private async Task<bool> CheckOrder(OrderSubmitMessage message, IModel channel)
    {
        var response = await _context.ExecuteTransactionalAsync(async () =>
        {

            var messageResponse = new ValidateOrderSubmitResponseMessage()
            {
                CustomerId = message.CustomerId,
                OrderId = message.OrderId
            };

            var customer = await _context.Customers.SingleOrDefaultAsync(c => c.Id == message.CustomerId);

            if (customer.Credit < message.OrderTotal)
                messageResponse.IsSuccess = false;
            else
            {
                customer.UpdateCredit(message.OrderTotal);
                await _context.SaveChangesAsync();
                messageResponse.IsSuccess = true;
            }

            return messageResponse;

        }, isolationLevel: IsolationLevel.ReadCommitted);


        channel.QueueDeclare("validateOrder", true, false, false, null);
        var body = _messageBus.ConvertToBodyMessage(response!);
        var prop = channel.CreateBasicProperties();
        prop.Persistent = true;
        channel.BasicPublish("", "validateOrder", false, prop, body);


        return true;
    }

}

