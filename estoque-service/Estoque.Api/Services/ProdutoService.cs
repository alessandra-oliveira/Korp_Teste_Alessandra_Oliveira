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
        return await _context.Produtos
            .OrderBy(p => p.CreatedAt)
            .ThenBy(p => p.Id)
            .ToListAsync();
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

        var codigoJaExiste = await _context.Produtos
            .AnyAsync(p => p.Codigo == produto.Codigo);

        if (codigoJaExiste)
            throw new ArgumentException($"Já existe um produto cadastrado com o código '{produto.Codigo}'.");

        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        return produto;
    }
        public async Task<Produto> AtualizarAsync(int id, Produto produtoAtualizado)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto is null)
            throw new KeyNotFoundException($"Produto com Id {id} não encontrado.");

        if (string.IsNullOrWhiteSpace(produtoAtualizado.Codigo))
            throw new ArgumentException("Código do produto é obrigatório.");

        if (string.IsNullOrWhiteSpace(produtoAtualizado.Descricao))
            throw new ArgumentException("Descrição do produto é obrigatória.");

        if (produtoAtualizado.Saldo < 0)
            throw new ArgumentException("Saldo não pode ser negativo.");

        var codigoEmUsoPorOutro = await _context.Produtos
            .AnyAsync(p => p.Codigo == produtoAtualizado.Codigo && p.Id != id);

        if (codigoEmUsoPorOutro)
            throw new ArgumentException($"Já existe outro produto cadastrado com o código '{produtoAtualizado.Codigo}'.");

        produto.Codigo = produtoAtualizado.Codigo;
        produto.Descricao = produtoAtualizado.Descricao;
        produto.Saldo = produtoAtualizado.Saldo;

        await _context.SaveChangesAsync();

        return produto;
    }

    public async Task ExcluirAsync(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto is null)
            throw new KeyNotFoundException($"Produto com Id {id} não encontrado.");

        _context.Produtos.Remove(produto);
        await _context.SaveChangesAsync();
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