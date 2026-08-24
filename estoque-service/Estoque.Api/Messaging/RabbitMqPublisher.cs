using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Estoque.Api.Messaging;

public class PublicadorRabbitMq
{
    private readonly string _hostName;
    private readonly string _userName;
    private readonly string _password;
    private const string QueueName = "nota-processada";

    public PublicadorRabbitMq(IConfiguration configuration)
    {
        _hostName = configuration["RabbitMq:HostName"] ?? "localhost";
        _userName = configuration["RabbitMq:UserName"] ?? "admin";
        _password = configuration["RabbitMq:Password"] ?? "admin";
    }

    public async Task PublicarNotaProcessadaAsync(int notaFiscalId)
    {
        var factory = new ConnectionFactory { HostName = _hostName, UserName = _userName, Password = _password };
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { NotaFiscalId = notaFiscalId }));
        await channel.BasicPublishAsync(exchange: string.Empty, routingKey: QueueName, body: body);
    }
}