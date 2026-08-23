using Faturamento.Api.Data;
using Faturamento.Api.Dtos;
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

    await ValidarProdutosExistemAsync(itens);
    await ValidarSaldoDisponivelAsync(itens);

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

    private async Task ValidarSaldoDisponivelAsync(List<ItemNotaFiscal> itens)
    {
        var client = _httpClientFactory.CreateClient("EstoqueApi");
        var insuficientes = new List<string>();

        foreach (var item in itens)
        {
            var response = await client.GetAsync($"api/Produtos/{item.ProdutoId}");

            if (!response.IsSuccessStatusCode)
                continue;

            var produto = await response.Content.ReadFromJsonAsync<ProdutoDto>();

            if (produto is not null && produto.Saldo < item.Quantidade)
            {
                insuficientes.Add(
                    $"{produto.Codigo} (disponível: {produto.Saldo}, solicitado: {item.Quantidade})");
            }
        }

    if (insuficientes.Any())
    {
        throw new ArgumentException(
            $"Saldo insuficiente para os produtos: {string.Join(", ", insuficientes)}");
    }
}

    private async Task ValidarProdutosExistemAsync(List<ItemNotaFiscal> itens)
    {
        var client = _httpClientFactory.CreateClient("EstoqueApi");
        var produtoIdsUnicos = itens.Select(i => i.ProdutoId).Distinct();
        var produtosInexistentes = new List<int>();

        foreach (var produtoId in produtoIdsUnicos)
        {
            HttpResponseMessage response;

            try
            {
                response = await client.GetAsync($"api/Produtos/{produtoId}");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException(
                    "Não foi possível conectar ao serviço de Estoque para validar os produtos. Tente novamente em instantes.", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                produtosInexistentes.Add(produtoId);
            }
        }

        if (produtosInexistentes.Count > 0)
        {
            throw new ArgumentException(
                $"Os seguintes produtos não foram encontrados: {string.Join(", ", produtosInexistentes)}.");
        }
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

    var insuficientes = new List<string>();

    foreach (var item in nota.Itens)
    {
        var produtoResponse = await client.GetAsync($"api/Produtos/{item.ProdutoId}");

        if (!produtoResponse.IsSuccessStatusCode)
            throw new InvalidOperationException($"Não foi possível consultar o produto {item.ProdutoId}.");

        var produto = await produtoResponse.Content.ReadFromJsonAsync<ProdutoDto>();

        if (produto is null || produto.Saldo < item.Quantidade)
        {
            insuficientes.Add($"Produto {item.ProdutoId} (disponível: {produto?.Saldo ?? 0}, solicitado: {item.Quantidade})");
        }
    }

    if (insuficientes.Any())
    {
        throw new InvalidOperationException(
            $"Saldo insuficiente para: {string.Join(", ", insuficientes)}");
    }

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