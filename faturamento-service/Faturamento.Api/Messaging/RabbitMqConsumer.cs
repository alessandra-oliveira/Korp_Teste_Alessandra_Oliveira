using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Faturamento.Api.Data;
using Faturamento.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Faturamento.Api.Messaging;

public class ConsumidorRabbitMq : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _hostName, _userName, _password;
    private const string QueueName = "nota-processada";

    public ConsumidorRabbitMq(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _hostName = configuration["RabbitMq:HostName"] ?? "localhost";
        _userName = configuration["RabbitMq:UserName"] ?? "admin";
        _password = configuration["RabbitMq:Password"] ?? "admin";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = _hostName, UserName = _userName, Password = _password };
        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var dados = JsonSerializer.Deserialize<Dictionary<string, int>>(json);

            if (dados is not null && dados.TryGetValue("NotaFiscalId", out var notaId))
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FaturamentoDbContext>();
                var nota = await context.NotasFiscais.FindAsync(notaId);
                if (nota is not null)
                {
                    nota.Status = StatusNotaFiscal.Processada;
                    await context.SaveChangesAsync();
                }
            }

            await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        };

        await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}