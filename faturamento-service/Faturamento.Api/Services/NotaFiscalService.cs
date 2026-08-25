using Faturamento.Api.Data;
using Faturamento.Api.Messaging;
using Faturamento.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Faturamento.Api.Services;

public class NotaFiscalService
{
    private readonly FaturamentoDbContext _context;
    private readonly PublicadorRabbitMq _publisher;

    public NotaFiscalService(FaturamentoDbContext context, PublicadorRabbitMq publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<List<NotaFiscal>> ListarAsync()
    {
        return await _context.NotasFiscais
            .Include(n => n.Itens)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<NotaFiscal> ObterPorIdAsync(int id)
    {
        var nota = await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (nota is null)
            throw new KeyNotFoundException($"Nota fiscal com Id {id} não encontrada.");

        return nota;
    }

    public async Task<NotaFiscal> CriarAsync(List<ItemNotaFiscal> itens)
    {
        if (itens is null || itens.Count == 0)
            throw new ArgumentException("A nota fiscal precisa ter ao menos um item.");

        var ultimoNumero = await _context.NotasFiscais
            .OrderByDescending(n => n.Numero)
            .Select(n => n.Numero)
            .FirstOrDefaultAsync();

        var nota = new NotaFiscal
        {
            Numero = ultimoNumero + 1,
            Status = StatusNotaFiscal.Aberta,
            Itens = itens
        };

        _context.NotasFiscais.Add(nota);
        await _context.SaveChangesAsync();

        return nota;
    }

    public async Task ProcessarAsync(int notaId)
    {
        var nota = await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == notaId);

        if (nota is null)
            throw new KeyNotFoundException($"Nota fiscal com Id {notaId} não encontrada.");

        if (nota.Status != StatusNotaFiscal.Aberta)
            throw new InvalidOperationException("Apenas notas com status Aberta podem ser processadas.");

        nota.Status = StatusNotaFiscal.Processando;
        await _context.SaveChangesAsync();

        var mensagem = new AtualizarSaldoMensagem
        {
            NotaFiscalId = nota.Id,
            Itens = nota.Itens.Select(i => new ItemMensagem
            {
                ProdutoId = i.ProdutoId,
                Quantidade = i.Quantidade
            }).ToList()
        };

        await _publisher.PublicarAsync(mensagem);
    }

    public async Task FecharAsync(int notaId)
    {
        var nota = await _context.NotasFiscais
            .FirstOrDefaultAsync(n => n.Id == notaId);

        if (nota is null)
            throw new KeyNotFoundException(
                $"Nota fiscal com Id {notaId} não encontrada.");

        if (nota.Status != StatusNotaFiscal.Processada)
            throw new InvalidOperationException(
                "Apenas notas com status Processada podem ser fechadas.");

        nota.Status = StatusNotaFiscal.Fechada;

        await _context.SaveChangesAsync();
    }
}