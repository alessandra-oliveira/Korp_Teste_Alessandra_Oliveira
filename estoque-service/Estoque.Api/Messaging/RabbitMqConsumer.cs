using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Estoque.Api.Services;

namespace Estoque.Api.Messaging;

public class ConsumidorRabbitMq : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _hostName;
    private readonly string _userName;
    private readonly string _password;
    private const string QueueName = "atualizar-saldo";

    public ConsumidorRabbitMq(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _hostName = configuration["RabbitMq:HostName"] ?? "localhost";
        _userName = configuration["RabbitMq:UserName"] ?? "admin";
        _password = configuration["RabbitMq:Password"] ?? "admin";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            UserName = _userName,
            Password = _password
        };

        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var mensagem = JsonSerializer.Deserialize<AtualizarSaldoMensagem>(json);

            if (mensagem is not null)
            {
                using var scope = _serviceProvider.CreateScope();
                var produtoService = scope.ServiceProvider.GetRequiredService<ProdutoService>();

                foreach (var item in mensagem.Itens)
                {
                    await produtoService.AtualizarSaldoAsync(item.ProdutoId, item.Quantidade);
                }
                var publisher = scope.ServiceProvider.GetRequiredService<PublicadorRabbitMq>();
                await publisher.PublicarNotaProcessadaAsync(mensagem.NotaFiscalId);
            }

            await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        };

        await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}