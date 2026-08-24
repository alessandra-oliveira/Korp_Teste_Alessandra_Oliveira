using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Faturamento.Api.Messaging;

public class PublicadorRabbitMq
{
    private readonly string _hostName;
    private readonly string _userName;
    private readonly string _password;
    private const string QueueName = "atualizar-saldo";

    public PublicadorRabbitMq(IConfiguration configuration)
    {
        _hostName = configuration["RabbitMq:HostName"] ?? "localhost";
        _userName = configuration["RabbitMq:UserName"] ?? "admin";
        _password = configuration["RabbitMq:Password"] ?? "admin";
    }

    public async Task PublicarAsync(AtualizarSaldoMensagem mensagem)
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            UserName = _userName,
            Password = _password
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var json = JsonSerializer.Serialize(mensagem);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: QueueName,
            body: body);
    }
}