using Estoque.Api.Models;
using Estoque.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly ProdutoService _produtoService;

    public ProdutosController(ProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var produtos = await _produtoService.ListarAsync();
        return Ok(produtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        try
        {
            var produto = await _produtoService.ObterPorIdAsync(id);
            return Ok(produto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar([FromBody] Produto produto)
    {
        try
        {
            var produtoCriado = await _produtoService.CadastrarAsync(produto);
            return CreatedAtAction(nameof(ObterPorId), new { id = produtoCriado.Id }, produtoCriado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/atualizar-saldo")]
    public async Task<IActionResult> AtualizarSaldo(int id, [FromBody] int quantidadeUtilizada)
    {
        try
        {
            await _produtoService.AtualizarSaldoAsync(id, quantidadeUtilizada);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}