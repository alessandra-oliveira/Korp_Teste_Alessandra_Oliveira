using Faturamento.Api.Data;
using Faturamento.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Faturamento.Api.Services;

public class NotaFiscalService
{
    private readonly FaturamentoDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public NotaFiscalService(FaturamentoDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
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

    public async Task FecharAsync(int notaId)
    {
        var nota = await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == notaId);

        if (nota is null)
            throw new KeyNotFoundException($"Nota fiscal com Id {notaId} não encontrada.");

        if (nota.Status != StatusNotaFiscal.Aberta)
            throw new InvalidOperationException("Apenas notas com status Aberta podem ser fechadas.");

        var client = _httpClientFactory.CreateClient("EstoqueApi");

        foreach (var item in nota.Itens)
        {
            HttpResponseMessage response;

            try
            {
                response = await client.PutAsJsonAsync(
                    $"api/Produtos/{item.ProdutoId}/atualizar-saldo",
                    item.Quantidade);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException(
                    "Não foi possível conectar ao serviço de Estoque. Tente novamente em instantes.", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Falha ao atualizar saldo do produto {item.ProdutoId}: {erro}");
            }
        }

        nota.Status = StatusNotaFiscal.Fechada;
        await _context.SaveChangesAsync();
    }
}