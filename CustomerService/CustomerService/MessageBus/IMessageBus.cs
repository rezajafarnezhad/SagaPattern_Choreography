using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace CustomerService.MessageBus;


public class BaseMessage
{
    public Guid MessageId { get; } = Guid.NewGuid();
    public DateTime MessageDateTime { get; } = DateTime.Now;

}

public interface IMessageBus
{
    IConnection GetConnection(string host, string userName, string pass);
    byte[] ConvertToBodyMessage(BaseMessage message);
}


public class MessageBus : IMessageBus
{
    private IConnection _connection;
    public IConnection GetConnection(string host, string userName, string pass)
    {
        if (_connection is null || !_connection.IsOpen)
            return CreateConnection(host, userName, pass: pass);

        return _connection;
    }

    public byte[] ConvertToBodyMessage(BaseMessage message)
    {
        var json = JsonConvert.SerializeObject(message);
        return Encoding.UTF8.GetBytes(json);
    }

    private IConnection CreateConnection(string host, string userName, string pass)
    {
        var connection = new ConnectionFactory()
        {
            HostName = host,
            UserName = userName,
            Password = pass,
        };
        _connection = connection.CreateConnection();
        return _connection;
    }
}