using Estoque.Api.Data;
using Estoque.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Api.Services;

public class ProdutoService
{
    private readonly EstoqueDbContext _context;

    public ProdutoService(EstoqueDbContext context)
    {
        _context = context;
    }

    public async Task<List<Produto>> ListarAsync()
    {
        return await _context.Produtos.ToListAsync();
    }

    public async Task<Produto> ObterPorIdAsync(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto is null)
            throw new KeyNotFoundException($"Produto com Id {id} não encontrado.");

        return produto;
    }

    public async Task<Produto> CadastrarAsync(Produto produto)
    {
        if (string.IsNullOrWhiteSpace(produto.Codigo))
            throw new ArgumentException("Código do produto é obrigatório.");

        if (string.IsNullOrWhiteSpace(produto.Descricao))
            throw new ArgumentException("Descrição do produto é obrigatória.");

        if (produto.Saldo < 0)
            throw new ArgumentException("Saldo não pode ser negativo.");

        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        return produto;
    }

    public async Task AtualizarSaldoAsync(int produtoId, int quantidadeUtilizada)
    {
        var produto = await _context.Produtos.FindAsync(produtoId);

        if (produto is null)
            throw new KeyNotFoundException($"Produto com Id {produtoId} não encontrado.");

        if (produto.Saldo < quantidadeUtilizada)
            throw new InvalidOperationException(
                $"Saldo insuficiente para o produto {produto.Descricao}. " +
                $"Saldo atual: {produto.Saldo}, solicitado: {quantidadeUtilizada}.");

        produto.Saldo -= quantidadeUtilizada;
        await _context.SaveChangesAsync();
    }
}